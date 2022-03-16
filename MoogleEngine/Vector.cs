namespace MoogleEngine;

public class Vector
{
    //Constructor
    private double[] elements;
    public Vector(double[] elements)
    {
        if (elements == null)
            throw new ArgumentException("The input Vector can't be null");

        this.elements = elements;
    }

    //Properties
    public int Size
    {
        get { return this.elements.Length; }
    }
    public double[] Elements
    {
        get { return this.elements; }

        set { this.elements = value; }
    }

    //Indexer
    public double this[int i]
    {
        get
        {
            if (i < 0 || i >= this.Size)
                throw new IndexOutOfRangeException();
            return this.elements[i];

        }
        set
        {
            if (i < 0 || i >= this.Size)
                throw new IndexOutOfRangeException();
            this.elements[i] = value;
        }
    }

    #region Statics Methods

    static private double Calc_ScalarProduct(Vector v1, Vector v2)
    {
        CheckNullVector(v1);
        CheckNullVector(v2);

        if (v1.Size != v2.Size)
            throw new ArgumentException("Incompatible vectors");

        double result = 0d;

        for (int i = 0; i < v1.Size; i++)
        {
            result += (v1[i] * v2[i]);
        }

        return result;
    }

    static private double Calc_VectorsModule(Vector v) //Norma del Vector
    {
        CheckNullVector(v);

        double result = 0d;

        for (int i = 0; i < v.Size; i++)
        {
            result += Math.Pow(v[i], 2);
        }

        return Math.Sqrt(result);
    }

    static public double GetSimilarity(Vector v1, Vector v2)
    {
        return Calc_ScalarProduct(v1, v2) / Calc_VectorsModule(v1) * Calc_VectorsModule(v2);
    }

    #endregion

    #region Helper methods
    private static void CheckNullVector(Vector v)
    {
        // Verificar los valores de entrada
        if (v == null)
        {
            throw new ArgumentException("Operands cannot be null");
        }
    }
    #endregion
}