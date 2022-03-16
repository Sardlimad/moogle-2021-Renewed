namespace MoogleEngine;

public class Operators
{

    public static Dictionary<string, string> Word_Operators = new Dictionary<string, string>();

    public static List<string> Words_Ignore = new List<string>();

    public static List<string> Words_Required = new List<string>();

    static Dictionary<string, int> Words_Priority = new Dictionary<string, int>();
    //Detectar Operadores
    public static void RunOperators(string query)
    {
        string clean_query = NoMarks(query);

        for (int i = 0; i < clean_query.Length; i++)
        {
            DetectOperators(clean_query, clean_query[i], i);
        }

    }

    #region ProcessText

    private static string NoMarks(string text) //Método para eliminar todos los símbolos. 
    {
        string new_text = "";

        for (int j = 0; j < text.Length; j++)
        {
            if (!Char.IsLetterOrDigit(text[j]) && text[j] != '!' && text[j] != '*' && text[j] != '^' && text[j] != '~')
                new_text += ' ';            //Si el caracter no es una letra, un numero o un operador entonces lo reemplazamos por espacio y es lo que agregamos a la nueva cadena de texto.
            else
                new_text += text[j];         //de lo contrario lo agregamos a la nueva cadena de texo.
        }

        return new_text.ToLower();
    }

    public static string[] ProcessQuery(string query) //procesador de texto para string
    {
        return NoMarks(query).Split(' ');
    }

    #endregion

    public static bool IsOperator(char a)
    {
        if (a == '!' || a == '^' || a == '*' || a == '~') return true;

        return false;
    }
    static void DetectOperators(string query, char a, int position)
    {
        switch (a)
        {
            case '!':
                Ignore(query, position);
                break;
            case '^':
                Required(query, position);
                break;
            case '*':
                Priority(query, position);
                break;
            //case '~': Nearness()
            //break;
            default: break;
        }
    }

    static void Ignore(string query, int position) //Operador: !
    {
        string word = "";

        for (int i = position + 1; i < query.Length; i++)
        {
            if (query[i] == ' ') break;

            word += query[i];
        }

        Words_Ignore.Add(word);
    }

    static void Required(string query, int position) //Operador: ^
    {
        string word = "";

        for (int i = position + 1; i < query.Length; i++)
        {
            if (query[i] == ' ') break;

            word += query[i];
        }

        Words_Required.Add(word);
    }

    static void Priority(string query, int position) //Operador: *
    {
        string word = "";

        int priority_level = 1; //Cantidad de asteriscos. Le llamo Nivel de prioridad.

        for (int i = position + 1; i < query.Length; i++)
        {
            if (query[i] == '*')
            {
                priority_level++;
                continue;
            }
            if (query[i] == ' ') break;

            word += query[i];
        }

        if(!Words_Priority.ContainsKey(word))
            Words_Priority.Add(word, priority_level);
    }

    #region Métodos para que los operadores cumplan su función...

    public static bool CheckIgnore(string text)
    {
        for (int i = 0; i < Words_Ignore.Count; i++)
        {
            if (text.Contains(Words_Ignore[i], System.StringComparison.CurrentCultureIgnoreCase)) return false;
        }
        return true;
    }

    public static bool CheckRequired(string text)
    {
        for (int i = 0; i < Words_Required.Count; i++)
        {
            if (!text.Contains(Words_Required[i], System.StringComparison.CurrentCultureIgnoreCase)) return false;
        }
        return true;
    }

     public static void CheckPriority()
    {
        foreach (var item in Words_Priority)
        {
            //if(!text.Contains(item.Key)) continue;

            int pos = Moogle.Words.IndexOf(item.Key); //Número de la Columna de la Matrix que representa los valores de TF-IDF de la palabra en cuestión

            for (int i = 0; i < Moogle.tfidf_matrix_aux.Length-1; i++)
            {
                Console.WriteLine("Antes "+Moogle.tfidf_matrix_aux[i][pos]);
                Moogle.tfidf_matrix_aux[i].Elements[pos] *= Math.Pow(3,item.Value);
                Console.WriteLine("Después "+Moogle.tfidf_matrix_aux[i][pos]);
            }
            //Moogle.tfidf_matrix_aux[Moogle.tfidf_matrix.Length+1][pos] *=  Math.Pow(2,item.Value);
        }
    }

    #endregion

    public static void Restart()
    {
        Words_Ignore.RemoveRange(0, Words_Ignore.Count);
        Words_Required.RemoveRange(0, Words_Required.Count);
        Words_Priority.Clear();
    }
}