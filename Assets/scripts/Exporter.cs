using System.Collections.Generic;
using UnityEngine;

// takes in the resulting molecule, and exports it
public class Exporter : MonoBehaviour
{
    private static Exporter _instance;

    public static Exporter Instance
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

    public float scalingFactor;

    public Transform t_exportContainer;

    public MeshFilter toCut;
    public Mesh sphereMesh;
    public Mesh cubeMesh;
    public Transform cutter;

    private str_molecule toExport;
    public void RunExport(str_molecule input)
    {
        toExport = input;

        int[] atomCounts = CountAtoms(input.atomIndices.ToArray()); 

        Debug.Log("EXPORTING MOLECULE!!!");

        List<Mesh> meshes = new List<Mesh>();
        List<Vector3> offsets = new List<Vector3>();
        List <float> scales = new List<float>();
        
        float inBetweenSpacing = 4f; // 4mm

        float bondWidth = 4f; // 6mm

        float xLimit = 130f;

        float largestX = 0;

        float x = 0;
        float y = 0;
        for (int i = 0; i < input.atomIndices.Count; i++) // 1 for now
        {
            float scl = MoleculeConstructor.Instance.atomSizes[input.atomIndices[i].type] * 10;

            if (scl > largestX)
            {
                largestX = scl;
            }

            scales.Add(scl);
            offsets.Add(new Vector3(x, 0, y));

            toCut.mesh = sphereMesh;
            
            for (int j = 0; j < input.bonds.Count; j++)
            {
                // if a bond connects to this atom, stamp it in
                if (input.bonds[j].a == i)
                {
                    // go to where the middle of the bond would be
                    Vector3 v = (input.positions[input.bonds[j].b].ToVector3() - input.positions[input.bonds[j].a].ToVector3());
                    cutter.position = v/2;
                    cutter.up = v;
                    cutter.localScale = new Vector3(bondWidth/scl, v.magnitude * 0.6f, bondWidth/scl);

                    toCut.mesh = MeshUtils.BooleanCut(toCut.mesh, cutter);
                }
                if (input.bonds[j].b == i)
                {
                    Vector3 v = (input.positions[input.bonds[j].a].ToVector3() - input.positions[input.bonds[j].b].ToVector3());
                    cutter.position = v/2;
                    cutter.up = v;
                    cutter.localScale = new Vector3(bondWidth/scl, v.magnitude * 0.6f, bondWidth/scl);

                    toCut.mesh = MeshUtils.BooleanCut(toCut.mesh, cutter);
                }
            }

            // stamping is now done

            // now for the slicing

            meshes.Add(toCut.mesh);

            x += scl + inBetweenSpacing;

            if (x >= xLimit)
            {
                x = 0;
                y += largestX+inBetweenSpacing;
                largestX = 0;
            }
        }

        // now that we have all the balls (and hopefully we won't need to cut them),
        // we can add the bond objects themselves to the plate
        
        for (int i = 0; i < input.bonds.Count; i++)
        {
            scales.Add(1);
            offsets.Add(new Vector3(x, 0, y));

            meshes.Add(MeshUtils.ScaleMesh(cubeMesh, new Vector3(bondWidth, (input.positions[input.bonds[i].a].ToVector3() - input.positions[input.bonds[i].b].ToVector3()).magnitude * 10f, bondWidth)));

            x += bondWidth + inBetweenSpacing;

            if (x >= xLimit)
            {
                x = 0;
                y += largestX+inBetweenSpacing;
                largestX = 0;
            }
        }

        // we combine the bonds and balls into one plate
        Mesh plate = MeshUtils.CombineMeshes(meshes.ToArray(), offsets.ToArray(), scales.ToArray());

        OBJExporter.ExportMeshToObj(plate, "C:\\Users\\maxim\\Desktop\\molecule export\\export.obj");
    }

    // ewww duplicate function from MoleculeConstructor.cs
    int[] CountAtoms(str_atom[] atoms)
    {
        int[] result = new int[atoms.Length];

        for (int i = 0; i < atoms.Length; i++)
        {
            result[atoms[i].type]++;
        }

        return result;
    }
}
