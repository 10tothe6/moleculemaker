using UnityEngine;

[System.Serializable]
public class str_bond
{
    public float bondLength;

    public int a;
    public int b;

    public str_bond()
    {}

    public str_bond(int a, int b)
    {
        this.a = a;
        this.b = b;
        this.bondLength = 1.25f;
    }
    public str_bond(int a, int b, float bondLength)
    {
        this.a = a;
        this.b = b;
        this.bondLength = bondLength;
    }
}
