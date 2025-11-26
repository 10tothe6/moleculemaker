using UnityEngine;
using System.IO;
using System.Text;

public static class OBJExporter
{
    public static void ExportMeshToObj(Mesh mesh, string filePath)
    {
        StringBuilder sb = new StringBuilder();

        // Write vertices
        foreach (Vector3 v in mesh.vertices)
        {
            sb.AppendLine($"v {v.x} {v.y} {v.z}");
        }

        // Write normals
        foreach (Vector3 n in mesh.normals)
        {
            sb.AppendLine($"vn {n.x} {n.y} {n.z}");
        }

        // Write UVs
        foreach (Vector2 uv in mesh.uv)
        {
            sb.AppendLine($"vt {uv.x} {uv.y}");
        }

        // Write faces (triangles)
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                // OBJ indices are 1-based
                int v1 = triangles[j] + 1;
                int v2 = triangles[j + 1] + 1;
                int v3 = triangles[j + 2] + 1;

                sb.AppendLine($"f {v1}/{v1}/{v1} {v2}/{v2}/{v2} {v3}/{v3}/{v3}");
            }
        }

        File.WriteAllText(filePath, sb.ToString());
    }
}