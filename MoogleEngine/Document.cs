namespace MoogleEngine;
public class Document
{
    //Constructor
    public Document(string Path, string Content)
    {
        this.Path = Path;
        this.Content = Content;
        this.Processed = ProcessText(Content);
        this.Marks = NoMarks(Content);
    }

    //Properties
    public string Path
    {
        get; 
        private set;
    }
    public string Content
    {
        get; 
        private set;
    }
    public string[] Processed
    {
        get;
        private set;
    }

    public string Marks
    {
        get;
        private set;
    }

    #region ProcessText
    private static string NoMarks(string text) //Método para eliminar todos los símbolos. 
    {
        string new_text = "";

        for (int j = 0; j < text.Length; j++)
        {
            if (!Char.IsLetter(text[j]) && !Char.IsWhiteSpace(text[j])) /*&& text[j] != 'á' && text[j] != 'é' && text[j] != 'í' && text[j] != 'ó' && text[j] != 'ú'*/
                continue;            //Si el caracter no es una letra entonces lo reemplazamos por espacio y es lo que agregamos a la nueva cadena de texto.
            else
                new_text += text[j];        //si el caracter es una letra pues la agregamos a la nueva cadena de texo.
        }

        return new_text.ToLower();
    }

    public static string[] ProcessText(string query) //procesador de texto para string
    {
        return NoMarks(query).Split();
    }
    #endregion

}