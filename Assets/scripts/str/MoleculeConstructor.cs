using System.Collections.Generic;
using System.Data;
using TMPro;
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
    public float boundingBoxLineWidth;

    // SUPPORTS
    public Transform t_supportContainer;
    public float supportWidth;

    public VisualAtom interactingAtom;

    public TextMeshProUGUI atomCountDisplay;

    private int[] atomCounts;

    void Start()
    {
        atomCounts = new int[atomNames.Length];
        Application.targetFrameRate = 60;
        atomObjects = new List<Transform>();

        LoadMolecule(rw_utils.LoadMolecule());
    }

    void OnApplicationQuit()
    {
        rw_utils.SaveMolecule(GetMolecule());
    }

    // molecule editing ****

    str_molecule GetMolecule()
    {
        str_molecule result = new str_molecule();

        result.bonds = bonds;
        
        for (int i = 0; i < atomObjects.Count; i++)
        {
            result.positions.Add(new DoubleVector3(atomObjects[i].position));
            result.atomIndices.Add(new str_atom(atomObjects[i].GetComponent<VisualAtom>().type));
        }

        return result;
    }

    void CountAtoms()
    {
        atomCounts = new int[atomNames.Length];

        for (int i = 0; i < atomObjects.Count; i++)
        {
            atomCounts[atomObjects[i].GetComponent<VisualAtom>().type]++;
        }
    }

    public void DeleteAllAtoms()
    {
        CanvasUtils.DestroyChildren(t_atomContainer.gameObject);
        CanvasUtils.DestroyChildren(t_supportContainer.gameObject);
        CanvasUtils.DestroyChildren(t_bondContainer.gameObject);

        bonds = new List<str_bond>();
        atomObjects = new List<Transform>();
    }

    public void LoadMolecule(str_molecule _mol)
    {
        if (_mol == null) return;

        // clear all existing atoms
        CanvasUtils.DestroyChildren(t_atomContainer.gameObject);
        CanvasUtils.DestroyChildren(t_supportContainer.gameObject);
        
        for (int i = 0; i < _mol.positions.Count; i++)
        {
            MakeNewAtom(_mol.atomIndices[i].type, _mol.positions[i].ToVector3(), false);
        }

        bonds = _mol.bonds;
        GenerateBondLines();
    }

    void MakeNewAtom(int type, Vector3 pos, bool addToHand)
    {
        GameObject g_newAtom = Instantiate(p_atom, t_atomContainer);
        
        VisualAtom comp = g_newAtom.GetComponent<VisualAtom>();
        comp.type = type;
        comp.SetType();

        if (addToHand)
        {
            atomInHand = comp;

            if (atomObjects.Count == 0)
            {
                atomInHand = null;
            }
        }
        
        g_newAtom.transform.position = pos;
        atomObjects.Add(g_newAtom.transform);

        CountAtoms();
    }

    public void GrabAtom(int type)
    {
        if (atomInHand != null) {return;} // can't have more than one atom in hand

        MakeNewAtom(type, Vector3.zero, true);
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
    Vector2[] GetBounds()
    {
        Vector2[] result = new Vector2[]
        {
            new Vector2(9999,-9999),
            new Vector2(9999,-9999),
            new Vector2(9999,-9999)
        };

        for (int i = 0; i < atomObjects.Count; i++)
        {
            if (atomObjects[i].position.x < result[0].x)
            {
                result[0].x = atomObjects[i].position.x;
            }
            if (atomObjects[i].position.x > result[0].y)
            {
                result[0].y = atomObjects[i].position.x;
            }

            if (atomObjects[i].position.y < result[1].x)
            {
                result[1].x = atomObjects[i].position.y;
            }
            if (atomObjects[i].position.y > result[1].y)
            {
                result[1].y = atomObjects[i].position.y;
            }

            if (atomObjects[i].position.z < result[2].x)
            {
                result[2].x = atomObjects[i].position.z;
            }
            if (atomObjects[i].position.z > result[2].y)
            {
                result[2].y = atomObjects[i].position.z;
            }
        }

        return result;
    }

    public float GetBondLength(int typeA, int typeB)
    {
        return 1.25f;
    }

    void GenerateSupport(VisualAtom target)
    {
        Transform t_newSupport = Instantiate(p_line, t_supportContainer).transform;

        Vector2[] b = GetBounds();
        t_newSupport.position = new Vector3(target.transform.position.x, (b[1].y - b[1].x)/2 + (target.transform.position.y - (b[1].y - b[1].x)/2) / 2, target.transform.position.z);
        t_newSupport.localScale = Vector3.one * supportWidth + Vector3.up * ((target.transform.position.y - (b[1].y - b[1].x)/2) - supportWidth);
    }

    // ****

    void AddBondToMolecule(VisualAtom a, VisualAtom b)
    {
        // adding a bond between two atoms, to make it cyclic
        int aIndex = -1;
        for (int i = 0; i < atomObjects.Count; i++)
        {
            if (atomObjects[i].GetComponent<VisualAtom>() == a)
            {
                aIndex = i;
                break;
            }
        }

        int bIndex = -1;
        for (int i = 0; i < atomObjects.Count; i++)
        {
            if (atomObjects[i].GetComponent<VisualAtom>() == b)
            {
                bIndex = i;
                break;
            }
        }

        if (aIndex != -1 && bIndex != -1)
        {
            bonds.Add(new str_bond(aIndex, bIndex));
            GenerateBondLines();
        }
    }

    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Exporter.Instance.RunExport(GetMolecule());
        }
        
        atomCountDisplay.text = "";
        for (int i = 0; i < atomCounts.Length; i++)
        {
            atomCountDisplay.text += atomNames[i] + ": " + atomCounts[i].ToString() + "     ";
        }
        atomCountDisplay.text += "Bonds: " + bonds.Count;

        // some quick hotkeys
        if (Keyboard.current.digit1Key.wasReleasedThisFrame)
        {
            GrabAtom(0);
        }
        else if (Keyboard.current.digit2Key.wasReleasedThisFrame)
        {
            GrabAtom(1);
        }
        else if (Keyboard.current.digit3Key.wasReleasedThisFrame)
        {
            GrabAtom(2);
        }
        else if (Keyboard.current.digit4Key.wasReleasedThisFrame)
        {
            GrabAtom(3);
        }
        else if (Keyboard.current.digit5Key.wasReleasedThisFrame)
        {
            GrabAtom(4);
        }
        else if (Keyboard.current.digit6Key.wasReleasedThisFrame)
        {
            GrabAtom(5);
        }


        if (atomObjects.Count > 1)
        {
            t_boundingLineContainer.gameObject.SetActive(true);

            // updating the bounding box
            Vector2[] bounds = GetBounds();
            t_boundingLineContainer.GetChild(0).position = new Vector3((bounds[0].x + bounds[0].y) / 2, bounds[1].x, bounds[2].x);
            t_boundingLineContainer.GetChild(0).up = Vector3.right;
            t_boundingLineContainer.GetChild(0).localScale = new Vector3(boundingBoxLineWidth, bounds[0].y - bounds[0].x, boundingBoxLineWidth);

            t_boundingLineContainer.GetChild(1).position = new Vector3(bounds[0].x, bounds[1].x, (bounds[2].x + bounds[2].y) / 2);
            t_boundingLineContainer.GetChild(1).up = Vector3.forward;
            t_boundingLineContainer.GetChild(1).localScale = new Vector3(boundingBoxLineWidth, bounds[2].y - bounds[2].x, boundingBoxLineWidth);
            
            t_boundingLineContainer.GetChild(2).position = new Vector3(bounds[0].x, (bounds[1].y + bounds[1].x)/2, bounds[2].x);
            t_boundingLineContainer.GetChild(2).up = Vector3.up;
            t_boundingLineContainer.GetChild(2).localScale = new Vector3(boundingBoxLineWidth, bounds[1].y - bounds[1].x, boundingBoxLineWidth);

            // originally I had a full horizontal rectangle, but now I only have 2 lines so here's the code I don't need:

            // t_boundingLineContainer.GetChild(1).position = new Vector3((bounds.x + bounds.y) / 2, boundingBoxOffset, bounds.w);
            // t_boundingLineContainer.GetChild(1).up = Vector3.right;
            // t_boundingLineContainer.GetChild(1).localScale = new Vector3(boundingBoxLineWidth, bounds.y - bounds.x, boundingBoxLineWidth);
            // t_boundingLineContainer.GetChild(3).position = new Vector3(bounds.y, boundingBoxOffset, (bounds.z + bounds.w) / 2);
            // t_boundingLineContainer.GetChild(3).up = Vector3.forward;
            // t_boundingLineContainer.GetChild(3).localScale = new Vector3(boundingBoxLineWidth, bounds.w - bounds.z, boundingBoxLineWidth);
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
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                RaycastHit hit;
                Vector3 mPos = Mouse.current.position.ReadValue();
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mPos.x, mPos.y, 10));
                if (Physics.Raycast(Camera.main.transform.position, worldPos - Camera.main.transform.position, out hit, Mathf.Infinity))
                {
                    VisualAtom comp = hit.collider.gameObject.GetComponent<VisualAtom>();
                    
                    if (interactingAtom == null)
                    {
                        interactingAtom = comp;
                    } else
                    {
                        if (interactingAtom == comp)
                        {
                            GenerateSupport(comp);
                            interactingAtom = null;
                        }
                        else
                        {
                            AddBondToMolecule(comp, interactingAtom);
                            interactingAtom = null;
                        }
                    }
                }
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
