namespace MoogleEngine;

public class Operators
{

    public static Dictionary<string, string> Word_Operators = new Dictionary<string, string>();

    public static List<string> Words_Ignore = new List<string>();

    public static List<string> Words_Required = new List<string>();

    static Dictionary<string, int> Words_Priority = new Dictionary<string, int>();

    static List<(string t1, string t2)> Words_Nearness = new List<(string, string)>();
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
            case '~':
                Nearness(query, position);
                break;
            default: break;
        }
    }

    static void Ignore(string query, int position) //Operador: !
    {
        string word = "";

        for (int i = position + 1; i < query.Length; i++)
        {
            if (!Char.IsLetterOrDigit(query[i])) break;

            word += query[i];
        }

        Words_Ignore.Add(word);
    }

    static void Required(string query, int position) //Operador: ^
    {
        string word = "";

        for (int i = position + 1; i < query.Length; i++)
        {
            if (!Char.IsLetterOrDigit(query[i])) break;

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
            if (!Char.IsLetterOrDigit(query[i])) break;

            word += query[i];
        }

        if (!Words_Priority.ContainsKey(word))
            Words_Priority.Add(word, priority_level);
    }
    static void Nearness(string query, int position) //Operador: ~
    {
        string word1 = "";

        string word2 = "";

        for (int i = position - 1; i >= 0; i--)
        {
            if (!Char.IsLetterOrDigit(query[i])) break;

            word1 += query[i];
        }

        char[] new_word1 = word1.ToCharArray();
        Array.Reverse(new_word1);

        word1 = "";

        for (int i = 0; i < new_word1.Length; i++)
        {
            word1 += new_word1[i];
        }

        for (int i = position + 1; i < query.Length; i++)
        {
            if (!Char.IsLetterOrDigit(query[i])) break;

            word2 += query[i];
        }

        Words_Nearness.Add((word1, word2));
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
            int pos = Moogle.Words.IndexOf(item.Key); //Número de la Columna de la Matrix que representa los valores de TF-IDF de la palabra en cuestión

            if (pos != -1)
            {
                for (int i = 0; i < Moogle.tfidf_matrix_aux.Length - 1; i++)
                {
                    Moogle.tfidf_matrix_aux[i].Elements[pos] *= Math.Pow(3, item.Value);
                }
            }
        }
    }

    public static float[] CheckNearness()
    {
        float[] increase = new float[Moogle.docs.Length];
        for (int i = 0; i < Words_Nearness.Count; i++)
        {
            if (!Moogle.Words_Docs_Pos.ContainsKey(Words_Nearness[i].t1) || !Moogle.Words_Docs_Pos.ContainsKey(Words_Nearness[i].t2)) break;

            for (int j = 0; j < Moogle.docs.Length; j++)
            {
                //if(!text.Contains(Words_Nearness[i].t1) || !text.Contains(Words_Nearness[i].t2)); continue;
                if (Moogle.Words_Docs_Pos[Words_Nearness[i].t1][j] is null || Moogle.Words_Docs_Pos[Words_Nearness[i].t2][j] is null) continue;

                int best_dist = BestDistance(Moogle.Words_Docs_Pos[Words_Nearness[i].t1][j], Moogle.Words_Docs_Pos[Words_Nearness[i].t2][j]);

                if (best_dist != 0) increase[j] = (float)10 / (float)best_dist;

            }
        }
        return increase;
    }

    static int BestDistance(List<int> word1, List<int> word2)
    {
        int best_dist = int.MaxValue;
        int temp_dist = 0;
        for (int i = 0; i < word1.Count; i++)
        {
            for (int j = 0; j < word2.Count; j++)
            {
                temp_dist = Math.Abs(word1[i] - word2[j]);
                if (temp_dist < best_dist)
                    best_dist = temp_dist;
            }
        }
        return best_dist;
    }

    #endregion

    public static void Restart()
    {
        Words_Ignore.RemoveRange(0, Words_Ignore.Count);
        Words_Required.RemoveRange(0, Words_Required.Count);
        Words_Priority.Clear();
        Words_Nearness.RemoveRange(0, Words_Nearness.Count);
    }
}