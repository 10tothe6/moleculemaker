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
    public Transform cutter2;

    private str_molecule toExport;

    int GetMatchingVector(Vector3[] data, Vector3 target)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (Vector3.Distance(data[i], target) < 0.05f || Vector3.Distance(data[i], -target) < 0.05f)
            {
                return i;
            }
        }

        return -1;
    }
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

        float xLimit = 150f;

        float largestX = 0;

        float x = 0;
        float y = 0;

        int hSkipCount = 42; // temp
        
        List<int> newPlatePoints = new List<int>();
        newPlatePoints.Add(0);

        for (int i = 0; i < input.atomIndices.Count; i++) // 1 for now
        {
            if (hSkipCount > 0 && input.atomIndices[i].type == 0)
            {
                hSkipCount--;
                continue;
            }

            float scl = MoleculeConstructor.Instance.atomSizes[input.atomIndices[i].type] * 10;

            if (scl > largestX)
            {
                largestX = scl;
            }

            scales.Add(scl);
            offsets.Add(new Vector3(x, 0, y));

            toCut.mesh = sphereMesh;

            List<bool> doubles = new List<bool>();
            List<Vector3> directions = new List<Vector3>();

            for (int j = 0; j < input.bonds.Count; j++)
            {
                // recording either single or double bonds to stamp
                if (input.bonds[j].a == i)
                {
                    // go to where the middle of the bond would be
                    Vector3 v = (input.positions[input.bonds[j].b].ToVector3() - input.positions[input.bonds[j].a].ToVector3());
                    int matching = GetMatchingVector(directions.ToArray(), v);
                    if (matching == -1)
                    {
                        directions.Add(v);
                        doubles.Add(false);
                    }
                    else
                    {
                        doubles[matching] = true;
                    }
                }
                if (input.bonds[j].b == i)
                {
                    Vector3 v = (input.positions[input.bonds[j].a].ToVector3() - input.positions[input.bonds[j].b].ToVector3());
                    int matching = GetMatchingVector(directions.ToArray(), v);
                    if (matching == -1)
                    {
                        directions.Add(v);
                        doubles.Add(false);
                    }
                    else
                    {
                        doubles[matching] = true;
                    }
                }
            }
            
            for (int j = 0; j < directions.Count; j++)
            {
                // go to where the middle of the bond would be
                Vector3 v = directions[j];

                if (doubles[j])
                {
                    cutter.position = v/2;
                    cutter.up = v;
                    cutter.localScale = new Vector3((bondWidth+0.3f)/scl, v.magnitude * 0.6f, (bondWidth+0.3f)/scl);

                    cutter2.position = cutter.position;
                    cutter2.up = v;
                    cutter2.localScale = new Vector3((bondWidth+0.3f)/scl, v.magnitude * 0.6f, (bondWidth+0.3f)/scl);

                    cutter.position += cutter.right * MoleculeConstructor.Instance.doubleBondSpacing;
                    cutter2.position -= cutter.right * MoleculeConstructor.Instance.doubleBondSpacing;

                    toCut.mesh = MeshUtils.BooleanCut(toCut.mesh, cutter);
                    toCut.mesh = MeshUtils.BooleanCut(toCut.mesh, cutter2);
                } else
                {
                    cutter.position = v/2;
                    cutter.up = v;
                    cutter.localScale = new Vector3((bondWidth+0.3f)/scl, v.magnitude * 0.6f, (bondWidth+0.3f)/scl);

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

            if (y >= xLimit)
            {
                x = 0;
                y = 0;

                newPlatePoints.Add(meshes.Count);
            }
        }

        // now that we have all the balls (and hopefully we won't need to cut them),
        // we can add the bond objects themselves to the plate

        // temp remove
        
        // for (int i = 0; i < input.bonds.Count; i++)
        // {
    
        //     scales.Add(1);
        //     offsets.Add(new Vector3(x, 0, y));

        //     meshes.Add(MeshUtils.ScaleMesh(cubeMesh, new Vector3(bondWidth, (input.positions[input.bonds[i].a].ToVector3() - input.positions[input.bonds[i].b].ToVector3()).magnitude * 10f, bondWidth)));

        //     x += bondWidth + inBetweenSpacing;

        //     if (x >= xLimit)
        //     {
        //         x = 0;
        //         y += largestX+inBetweenSpacing;
        //         largestX = bondWidth;
                
        //     }

        //     if (y >= xLimit)
        //     {
        //         x = 0;
        //         y = 0;

        //         newPlatePoints.Add(meshes.Count);
        //     }
        // }

        // we combine the bonds and balls into one plate
        for (int i = 0; i < newPlatePoints.Count; i++)
        {
            Mesh plate;
            if (i < newPlatePoints.Count - 1)
            {
                plate = MeshUtils.CombineMeshes(meshes.ToArray(), offsets.ToArray(), scales.ToArray(), newPlatePoints[i], newPlatePoints[i+1] - 1);
            }
            else
            {
                plate = MeshUtils.CombineMeshes(meshes.ToArray(), offsets.ToArray(), scales.ToArray(), newPlatePoints[i], meshes.Count - 1);
            }

            OBJExporter.ExportMeshToObj(plate, "C:\\Users\\maxim\\Desktop\\molecule export\\export" + i + ".obj");
        }
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
