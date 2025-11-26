using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class str_molecule
{
    public List<str_bond> bonds;
    public List<str_atom> atomIndices;
    public List<DoubleVector3> positions;

    public str_molecule()
    {
        bonds = new List<str_bond>();
        atomIndices = new List<str_atom>();
        positions = new List<DoubleVector3>();
    }
}
