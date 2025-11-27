using UnityEngine;

[System.Serializable]
public class str_lonepair
{
    public int objectIndex;
    public int atomObjectIndex;

    public str_lonepair(int objectIndex, int atomObjectIndex)
    {
        this.objectIndex = objectIndex;
        this.atomObjectIndex = atomObjectIndex;
    }
}
