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
    public List<str_bond> bonds;

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

    // BOND STUFF
    public Transform t_bondContainer;
    public GameObject p_line; // just used for visuals, the export process is different
    public float bondWidth;

    // BOUNDING BOX STUFF
    public Transform t_boundingLineContainer;
    public float boundingBoxOffset;
    public float boundingBoxLineWidth;

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

    void AttachAtomToMolecule(VisualAtom attachPoint)
    {
        Vector3 dir = atomInHand.transform.position - attachPoint.transform.position;

        float bondLength = GetBondLength(attachPoint.type, atomInHand.type);
        
        atomInHand.transform.position = attachPoint.transform.position +
        dir.normalized * bondLength;

        bonds.Add(new str_bond(atomObjects.Count - 1, atomObjects.IndexOf(attachPoint.transform), bondLength));

        atomInHand = null;

        GenerateBondLines();
    }

    void GenerateBondLines()
    {
        CanvasUtils.DestroyChildren(t_bondContainer.gameObject);
        for (int i = 0; i < bonds.Count; i++)
        {
            Vector3 a = atomObjects[bonds[i].a].position;
            Vector3 b = atomObjects[bonds[i].b].position;

            GameObject g_newBond = Instantiate(p_line, t_bondContainer);
            g_newBond.transform.position = (a + b) / 2;
            g_newBond.transform.up = b - a;
            g_newBond.transform.localScale = new Vector3(
                bondWidth,
                (b-a).magnitude,
                bondWidth
            );
        }
    }
    void RefreshBondLines()
    {
        for (int i = 0; i < bonds.Count; i++)
        {
            Vector3 a = atomObjects[bonds[i].a].position;
            Vector3 b = atomObjects[bonds[i].b].position;

            GameObject g_newBond = t_bondContainer.GetChild(i).gameObject;
            g_newBond.transform.position = (a + b) / 2;
            g_newBond.transform.up = b - a;
            g_newBond.transform.localScale = new Vector3(
                bondWidth,
                (b-a).magnitude,
                bondWidth
            );
        }
    }
    Vector4 GetBounds()
    {
        Vector4 result = new Vector4(9999, -9999, 9999, -9999); // xMin, xMAx, yMin, yMax
        for (int i = 0; i < atomObjects.Count; i++)
        {
            if (atomObjects[i].position.x < result.x)
            {
                result.x = atomObjects[i].position.x;
            }
            if (atomObjects[i].position.x > result.y)
            {
                result.y = atomObjects[i].position.x;
            }

            if (atomObjects[i].position.z < result.z)
            {
                result.z = atomObjects[i].position.z;
            }
            if (atomObjects[i].position.z > result.w)
            {
                result.w = atomObjects[i].position.z;
            }
        }

        return result;
    }

    float GetBondLength(int typeA, int typeB)
    {
        return 2;
    }

    // ****

    void Start()
    {
        Application.targetFrameRate = 60;
        atomObjects = new List<Transform>();
    }

    void Update()
    {
        if (atomObjects.Count > 1)
        {
            t_boundingLineContainer.gameObject.SetActive(true);

            // updating the bounding box
            Vector4 bounds = GetBounds();
            t_boundingLineContainer.GetChild(0).position = new Vector3((bounds.x + bounds.y) / 2, boundingBoxOffset, bounds.z);
            t_boundingLineContainer.GetChild(0).up = Vector3.right;
            t_boundingLineContainer.GetChild(0).localScale = new Vector3(boundingBoxLineWidth, bounds.y - bounds.x, boundingBoxLineWidth);
            t_boundingLineContainer.GetChild(1).position = new Vector3((bounds.x + bounds.y) / 2, boundingBoxOffset, bounds.w);
            t_boundingLineContainer.GetChild(1).up = Vector3.right;
            t_boundingLineContainer.GetChild(1).localScale = new Vector3(boundingBoxLineWidth, bounds.y - bounds.x, boundingBoxLineWidth);

            t_boundingLineContainer.GetChild(2).position = new Vector3(bounds.x, boundingBoxOffset, (bounds.z + bounds.w) / 2);
            t_boundingLineContainer.GetChild(2).up = Vector3.forward;
            t_boundingLineContainer.GetChild(2).localScale = new Vector3(boundingBoxLineWidth, bounds.w - bounds.z, boundingBoxLineWidth);
            t_boundingLineContainer.GetChild(3).position = new Vector3(bounds.y, boundingBoxOffset, (bounds.z + bounds.w) / 2);
            t_boundingLineContainer.GetChild(3).up = Vector3.forward;
            t_boundingLineContainer.GetChild(3).localScale = new Vector3(boundingBoxLineWidth, bounds.w - bounds.z, boundingBoxLineWidth);
        } else {

            t_boundingLineContainer.gameObject.SetActive(false);
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            pause = !pause;
        }

        if (atomInHand != null)
        {
            Vector3 mPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mPos.x, mPos.y, 10));
            atomInHand.transform.position = new Vector3(1000,1000,100);
            Physics.SyncTransforms();

            RaycastHit hit;
            bool hitOtherAtom = false;
            if (Physics.Raycast(Camera.main.transform.position, worldPos - Camera.main.transform.position, out hit, Mathf.Infinity))
            {
                atomInHand.transform.position = hit.point;
                hitOtherAtom = true;
            }
            else
            {
                atomInHand.transform.position = worldPos;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && hitOtherAtom)
            {
                AttachAtomToMolecule(hit.collider.gameObject.GetComponent<VisualAtom>());
            }
        }

        if (!pause)
        {
            GenerateRandomIndices();
            Adjust();
            Constrain();

            RefreshBondLines();
        }

        if (showBondLines)
        {
            for (int i = 0; i < bonds.Count; i++)
            {
                Debug.DrawLine(atomObjects[bonds[i].a].position, atomObjects[bonds[i].b].position);
            }
        }
    }

    void GenerateRandomIndices()
    {
        randomIndices = new int[bonds.Count];
        List<int> remainingIndices = new List<int>();
        for (int i = 0; i < bonds.Count; i++)
        {
            remainingIndices.Add(i);
        }

        for (int i = 0; i < bonds.Count; i++)
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
        for (int i = 0; i < bonds.Count; i++)
        {
            if (bonds[i].a == a)
            {
                connectingToA.Add(bonds[i].b);
            } else if (bonds[i].b == a)
            {
                connectingToA.Add(bonds[i].a);
            }
        }

        for (int i = 0; i < bonds.Count; i++)
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
        for (int i = 0; i < bonds.Count; i++)
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
