using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;

// all molecule editing code is in here, 
// I'd hate to have one big file like this but then again this is a small software so its chill

public class MoleculeConstructor : MonoBehaviour
{
    private static MoleculeConstructor _instance;

    public static MoleculeConstructor Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null)
            {
                _instance = value;
            }
            else if (_instance != value)
            {
                Debug.Log("Duplicate NetworkManager instance in scene!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    [Header("DATA")]
    public float atomSizeMultiplier;
    public float[] atomSizes;
    public str_atom[] atomPool;
    public str_bond[] bonds;

    public Sprite[] atomIcons;
    public string[] atomNames;
    
    public Material[] m_atoms;

    public List<Transform> atomObjects;

    public int simulationIterations;
    private int[] randomIndices;


    [Header("CONFIG")]
    public bool showBondLines;
    public bool pause;
    public float repulsionCoefficient;
    public float agitationCoefficient;
    public float separationCoefficient; // different bond heirarchies

    public VisualAtom atomInHand;
    public GameObject p_atom;
    public Transform t_atomContainer;

    // molecule editing ****

    public void GrabAtom(int type)
    {
        if (atomInHand != null) {return;} // can't have more than one atom in hand

        GameObject g_newAtom = Instantiate(p_atom, t_atomContainer);
        
        VisualAtom comp = g_newAtom.GetComponent<VisualAtom>();
        comp.type = type;
        comp.SetType();

        atomInHand = comp;
        
        if (atomObjects.Count == 0)
        {
            atomInHand = null;
            g_newAtom.transform.position = Vector3.zero;
        }
        atomObjects.Add(g_newAtom.transform);
    }

    // ****

    void Start()
    {
        Application.targetFrameRate = 60;
        atomObjects = new List<Transform>();
    }

    void Update()
    {
        if (atomInHand != null)
        {
            Vector3 mPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mPos.x, mPos.y, 10));
            atomInHand.transform.position = new Vector3(1000,1000,100);
            Physics.SyncTransforms();

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, worldPos - Camera.main.transform.position, out hit, Mathf.Infinity))
            {
                atomInHand.transform.position = hit.point;
            }
            else
            {
                atomInHand.transform.position = worldPos;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                atomInHand = null;
            }
        }

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
        Vector3[] force = new Vector3[atomObjects.Count];
        for (int i = 0; i < atomObjects.Count; i++)
        {
            force[i] = Vector3.zero;
            for (int j = 0; j < atomObjects.Count; j++)
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


        for (int i = 0; i < atomObjects.Count; i++)
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
        for (int i = 0; i < atomObjects.Count; i++)
        {
            m += atomObjects[i].localPosition;
        }
        m/=atomObjects.Count;

        for (int i = 0; i < atomObjects.Count; i++)
        {
            atomObjects[i].position -= m;
        }
    }
}
