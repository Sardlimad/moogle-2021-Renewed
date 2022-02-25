﻿namespace MoogleEngine;

public static class Moogle
{

    #region Variables necesarias
    private static int count_matches = 0; //Contador de cantidad de coincidencias.
    private static string query2 = "";

    static Document[] docs = new Document[ReadFolder().Length];
    static List<string> Words = new List<string>();

    static Vector[] tfidf_matrix = new Vector[docs.Length + 1];


    public static void Start()
    {
        docs = ReadFiles();
        Words = WordsCollection();
        tfidf_matrix = Create_TermDocumentMatrix();
    }

    #endregion

    static string[] ReadFolder() //Escanea las carpetas y subcarpetas en busca de archivos.
    {
        string[] files = Directory.GetFiles(@"../Content/", "*.txt", SearchOption.AllDirectories);

        return files;
    }

    static Document[] ReadFiles() //Leer y guardar el contenido de cada uno de los archivos de texto.
    {
        string[] files = ReadFolder();

        Document[] docs = new Document[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            string content = "";
            //Leer los archivos
            StreamReader reader = new StreamReader(files[i]);

            while (!reader.EndOfStream)
            {
                //Guardo el contenido del archivo
                content += reader.ReadLine() + " ";
            }

            docs[i] = new Document(files[i], content);

            reader.Close();
        }
        return docs;
    }

    public static SearchResult Query(string query)
    {
        count_matches = 0; // Llevar a cero todas las coincidencias antes de empezar una nueva búsqueda

        query2 = Suggest(query).TrimEnd();

        Create_QueryVector(query2);

        return new SearchResult(Items(ReadFiles()), query2);
    }

    public static SearchItem[] Items(Document[] docs) //Crear los elementos resultados
    {
        int[] words_positions = FindMatch(query2, docs);

        if (query2 == "")
        {
            SearchItem[] items = new SearchItem[1] { new SearchItem("Campo de Búsqueda Vacío", "😐", 0f) };

            return items;
        }
        else if (count_matches == 0)
        {
            SearchItem[] items = new SearchItem[1] { new SearchItem("No se han encontrado coincidencias", "😕", 0f) };

            return items;
        }
        else
        {
            SearchItem[] old_items = new SearchItem[count_matches];

            int count = count_matches - 1;

            float[] scores = new float[count_matches];

            for (int i = 0; i < words_positions.Length; i++)
            {
                if (words_positions[i] != -1)
                {
                    scores[count] = Calc_Score(tfidf_matrix[docs.Length], tfidf_matrix[i]);
                    old_items[count] = new SearchItem(Path.GetFileName(docs[i].Path) + scores[count], Create_Snippet(words_positions[i], docs[i].Content), scores[count]);
                    count--;
                }
            }

            SearchItem[] items = new SearchItem[count_matches];

            Array.Sort(scores); //Ordeno el array de scores

            for (int i = 0; i < scores.Length; i++)
            {
                for (int j = 0; j < old_items.Length; j++)
                {
                    if (scores[i] == old_items[j].Score)
                    {
                        items[i] = old_items[j];
                        break;
                    }

                }
            }
            
            return items;
        }
    }


    #region ProcessText

    private static string NoMarks(string text) //Método para eliminar todos los símbolos. 
    {
        string new_text = "";

        for (int j = 0; j < text.Length; j++)
        {
            if (!Char.IsLetter(text[j]))
                new_text += ' ';            //Si el caracter no es una letra entonces lo reemplazamos por espacio y es lo que agregamos a la nueva cadena de texto.
            else
                new_text += text[j];         //si el caracter es una letra pues la agregamos a la nueva cadena de texo.
        }

        return new_text.ToLower();
    }

    public static string[] ProcessText(string query) //procesador de texto para string
    {
        return NoMarks(query).Split(' ');
    }

    #endregion

    static List<string> WordsCollection() //Conjunto de todas las palabras del corpus
    {
        Document[] docs = ReadFiles();
        List<string> WordsCollection = new List<string>();

        for (int i = 0; i < docs.Length; i++)
        {
            string[] doc_words = docs[i].Processed;

            for (int j = 0; j < doc_words.Length; j++)
            {
                if (!WordsCollection.Contains(doc_words[j].ToLower()))
                    WordsCollection.Add(doc_words[j].ToLower());
            }
        }

        return WordsCollection;
    }

    #region Suggestion Group
    static int Calc_LevenshteinDistance(string word1, string word2)
    {
        int costo = 0; //Cantidad de operaciones necesarias para convertir la cadena "word1" en la cadena "word2".
        int m = word1.Length;
        int n = word2.Length;

        int[,] matrix = new int[m + 1, n + 1];

        //Verificar que exista algo para comparar
        if (n == 0) return m;
        if (m == 0) return n;

        // Llena la primera columna y la primera fila.
        for (int i = 0; i <= m; matrix[i, 0] = i++) ;
        for (int j = 0; j <= n; matrix[0, j] = j++) ;

        //Recorrer la matriz llenando cada uno de los pesos.
        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                // si son iguales en posiciones equidistantes el peso es 0 de lo contrario el peso aumenta en uno.
                costo = (word1[i - 1] == word2[j - 1]) ? 0 : 1;
                matrix[i, j] = System.Math.Min(System.Math.Min(matrix[i - 1, j] + 1,  //Eliminación
                              matrix[i, j - 1] + 1),                                  //Inserción 
                              matrix[i - 1, j - 1] + costo);                          //Sustitución
            }
        }

        return matrix[m, n];
    }

    static bool DoSuggestion(string inicial_w, string final_w)
    {
        if (Math.Abs(final_w.Length - inicial_w.Length) <= 3)
            return true;
        return false;
    }
    static string Suggest(string query)
    {
        string[] sub_query = ProcessText(query);

        string suggestion = "";

        for (int i = 0; i < sub_query.Length; i++)
        {

            string word_suggested = sub_query[i]; //Palabra más parecida a la palabra introducida por el usuario, inicialmente es la misma palabra.
            int lower_cost = int.MaxValue;

            foreach (var word in Words)
            {
                int cost = int.MaxValue;

                if (DoSuggestion(sub_query[i], word))
                {
                    cost = Calc_LevenshteinDistance(sub_query[i].ToLower(), word);

                    if (cost < 4 && cost < lower_cost)
                    {
                        lower_cost = cost;
                        word_suggested = word;
                    }
                }

                if (cost == 0) break; //Si encuentro una palabra idéntica, o sea con coste 0, entonces no seguir iterando.
            }

            suggestion += word_suggested + " ";
        }

        return suggestion;
    }

    #endregion

    static int[] FindMatch(string query, Document[] docs)
    {
        int[] word_position = new int[docs.Length];

        string[] sub_query = ProcessText(query);

        for (int i = 0; i < docs.Length; i++) //Iterando por cada documento
        {
            for (int j = 0; j < sub_query.Length; j++) //Iterando por cada palabra del query
            {
                //if (IsOperator(sub_query[j][1]))
                word_position[i] = docs[i].Content.IndexOf(sub_query[j], System.StringComparison.CurrentCultureIgnoreCase);

                if (word_position[i] != -1) { count_matches++; break; }
            }
        }

        return word_position;
    }

    #region Operators
    // static bool IsOperator(char m) //Comprobamos si un caracter es un operador: ! ~ ^ *
    // {
    //     if (m == '!' || m == '*' || m == '^' || m == '~')
    //         return true;
    //     return false;
    // }

    // static void Detect_Operator(string[] words)
    // {
    //     for (int i = 0; i < words.Length; i++)
    //     {
    //         switch (words[i][1])
    //         {
    //             case !:

    //             case *:

    //             case ^:

    //             case ~:

    //         }
    //     }
    // }

    // static void Ignore_Operator()
    // {

    // }

    #endregion
    static string Create_Snippet(int position, string Text)
    {
        return Text.Substring(Math.Max(0, position - 50), Math.Min(150, Text.Length - position));
    }

    #region TF-IDF 

    //Contar Cantidad de Veces que un término aparece en un Documento
    private static int Repeat(string term, Document document)
    {
        int ted = 0;  //Cantidad de veces que un término figura en un documento.
        string[] words = document.Processed;

        for (int i = 0; i < words.Length; i++)
        {
            // if (words[i].ToLower() == term.ToLower()) ted++;
            if (words[i].Equals(term, System.StringComparison.CurrentCultureIgnoreCase)) ted++;
        }

        //Console.WriteLine("Funciona Repeat");

        return ted;
    }

    private static float TF(string term, Document document)
    {
        int ted = Repeat(term, document);

        string[] words = document.Processed;
        int TED = words.Length; //Cantidad de palabras totales en el documento.

        float TF = (float)ted / (float)TED;

        //Console.WriteLine("Funciona TF");

        return TF;
    }

    private static float IDF(string term) // Inverse Document Frequency  IDF = log(D/deD) donde D = cantidad de documentos y deD = cantidad de veces que se repite el termino en el corpus
    {

        int D = docs.Length; //Cantidad total de documentos. "docs" es variable global.

        int deD = 0; //Suma de todas las veces que figura el término en todos los documentos del corpus.

        // string[] new_content = NewContent(docs);

        for (int i = 0; i < D; i++)
        {
            deD += Repeat(term, docs[i]);
        }

        float IDF = (float)Math.Log((float)D / (float)deD);

        //if (deD == 0)
        Console.WriteLine("Funciona IDF: " + term + "--" + deD);

        return IDF;

    }

    public static float TF_IDF(string term, Document doc)
    {
        float a = TF(term, doc) * IDF(term);
        Console.WriteLine("Funciona TF-IDF:" + term + "--" + a);
        return a;
    }

    #endregion

    #region Score

    static float Calc_Score(Vector v_query, Vector v_doc)
    {
        Console.WriteLine("Funciona Calc_Score");

        return (float)Vector.GetSimilarity(v_query, v_doc);
    }

    private static Vector Vectorize(Document doc) //Crear Vector del documento de dimesión = cantidad de palabras en el corpus.
    {
        double[] vector = new double[Words.Count];

        for (int i = 0; i < Words.Count; i++)
        {
            if (doc.Content.Contains(Words[i]))
            {
                vector[i] = TF_IDF(Words[i], doc);
            }
        }

        return new Vector(vector);
    }

    public static Vector[] Create_TermDocumentMatrix() //Crear matriz de 
    {

        Vector[] vectors = new Vector[docs.Length + 1];

        for (int i = 0; i < docs.Length; i++)
        {
            vectors[i] = Vectorize(docs[i]);
        }

        Console.WriteLine("Funciona Matrix");
        return vectors;
    }

    public static void Create_QueryVector(string query)
    {
        Document doc_query = new Document("query", query);

        tfidf_matrix[docs.Length] = Vectorize(doc_query);
    }

    #endregion
}