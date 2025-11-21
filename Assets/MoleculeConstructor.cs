using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class MoleculeConstructor : MonoBehaviour
{
    [Header("DATA")]
    public str_atom[] atomPool;
    public str_bond[] bonds;
    
    public Material[] m_atoms;

    

    public GameObject p_atom;

    public Transform[] atomObjects;

    public int simulationIterations;
    private int[] randomIndices;


    [Header("CONFIG")]
    public bool showBondLines;
    public bool pause;
    public float repulsionCoefficient;
    public float agitationCoefficient;
    public float separationCoefficient; // different bond heirarchies

    void Start()
    {
        Application.targetFrameRate = 60;

        atomObjects = new Transform[atomPool.Length];
        for (int i = 0; i < atomPool.Length; i++)
        {
            atomObjects[i] = Instantiate(p_atom, new Vector3(Random.Range(-2,2), Random.Range(-2,2), Random.Range(-2,2)), Quaternion.identity).transform;
            atomObjects[i].SetParent(transform);

            atomObjects[i].GetComponent<MeshRenderer>().sharedMaterial = m_atoms[atomPool[i].type];
        }
    }

    void Update()
    {
        if (!pause)
        {
            GenerateRandomIndices();
            Adjust();
            Constrain();
        }

        if (showBondLines)
        {
            for (int i = 0; i < bonds.Length; i++)
            {
                Debug.DrawLine(atomObjects[bonds[i].a].position, atomObjects[bonds[i].b].position);
            }
        }
    }

    void GenerateRandomIndices()
    {
        randomIndices = new int[bonds.Length];
        List<int> remainingIndices = new List<int>();
        for (int i = 0; i < bonds.Length; i++)
        {
            remainingIndices.Add(i);
        }

        for (int i = 0; i < bonds.Length; i++)
        {
            int index = Random.Range(0, remainingIndices.Count);
            randomIndices[i] = remainingIndices[index];
            remainingIndices.RemoveAt(index);
        }
    }

    void Adjust()
    {
        Vector3[] force = new Vector3[atomObjects.Length];
        for (int i = 0; i < atomObjects.Length; i++)
        {
            force[i] = Vector3.zero;
            for (int j = 0; j < atomObjects.Length; j++)
            {
                if (j != i)
                {
                    Vector3 v = atomObjects[i].position - atomObjects[j].position;
                    Vector3 f = v.normalized / Mathf.Pow(v.magnitude, 2) * repulsionCoefficient;
                    if (!IsSameBondingHeirarchy(i, j))
                    {
                        f *= separationCoefficient;
                    }
                    force[i] += f;
                }
            }
        }


        for (int i = 0; i < atomObjects.Length; i++)
        {
            atomObjects[i].position += force[i] + new Vector3(Random.Range(-1,1), Random.Range(-1,1), Random.Range(-1,1)) * agitationCoefficient;
        }
    }

    bool IsSameBondingHeirarchy(int a, int b)
    {
        List<int> connectingToA = new List<int>();
        for (int i = 0; i < bonds.Length; i++)
        {
            if (bonds[i].a == a)
            {
                connectingToA.Add(bonds[i].b);
            } else if (bonds[i].b == a)
            {
                connectingToA.Add(bonds[i].a);
            }
        }

        for (int i = 0; i < bonds.Length; i++)
        {
            if (bonds[i].a == b)
            {
                if (connectingToA.Contains(bonds[i].b))
                {
                    return true;
                }
            } else if (bonds[i].b == b)
            {
                if (connectingToA.Contains(bonds[i].a))
                {
                    return true;
                }
            }
        }

        return false;
    }

    void Constrain()
    {
        for (int i = 0; i < bonds.Length; i++)
        {
            Vector3 a = atomObjects[bonds[randomIndices[i]].a].position;
            Vector3 b = atomObjects[bonds[randomIndices[i]].b].position;

            atomObjects[bonds[randomIndices[i]].a].position = (a + b) / 2 + (a - b).normalized * bonds[randomIndices[i]].bondLength / 2;
            atomObjects[bonds[randomIndices[i]].b].position = (a + b) / 2 + (b - a).normalized * bonds[randomIndices[i]].bondLength / 2;
        }

        Vector3 m = Vector3.zero;
        for (int i = 0; i < atomObjects.Length; i++)
        {
            m += atomObjects[i].localPosition;
        }
        m/=atomObjects.Length;

        for (int i = 0; i < atomObjects.Length; i++)
        {
            atomObjects[i].position -= m;
        }
    }
}
