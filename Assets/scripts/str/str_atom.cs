using UnityEngine;

[System.Serializable]
public class str_atom
{
    // H, C, N, O, P, Mg
    // 0, 1, 2, 3, 4, 5
    public int type; 

    public str_atom(int type)
    {
        this.type = type;
    }
}
