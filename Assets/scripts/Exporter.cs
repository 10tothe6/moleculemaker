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
        
        float spacing = 1f; // radius plus a buffer

        int x = 0;
        int y = 0;
        for (int i = 0; i < input.atomIndices.Count; i++)
        {
            scales.Add(MoleculeConstructor.Instance.atomSizes[input.atomIndices[i].type] * 10);
            offsets.Add(Vector3.zero);

            toCut.mesh = sphereMesh;
            
        }

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
