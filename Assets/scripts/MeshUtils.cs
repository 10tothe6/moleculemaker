using System.Collections.Generic;
using UnityEngine;

public class MeshUtils
{
    public static Mesh CombineMeshes(Mesh[] input)
    {
        Vector3[] vo = new Vector3[input.Length];
        float[] vs = new float[input.Length];

        for (int i = 0; i < vo.Length; i++) {
            vo[i] = Vector3.zero;
            vs[i] = 1;
            }
        return CombineMeshes(input, vo, vs);
    }

    public static Mesh CombineMeshes(Mesh[] input, Vector3[] offsets, float[] scales)
    {
        Mesh result = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        int triangleIndexOffset = 0;
        for (int i = 0; i < input.Length; i++)
        {
            List<Vector2> currentUVs = new List<Vector2>();
            input[i].GetUVs(0, currentUVs);

            for (int j = 0; j < input[i].vertices.Length; j++)
            {
                verts.Add(input[i].vertices[j] * scales[i] + offsets[i]);
                norms.Add(input[i].normals[j]);
                uvs.Add(currentUVs[j]);
            }

            int[] currentTris = input[i].GetTriangles(0);
            for (int j = 0; j < currentTris.Length; j++)
            {
                tris.Add(currentTris[j] + triangleIndexOffset);
            }

            triangleIndexOffset = verts.Count;
        }

        result.SetVertices(verts);
        result.SetNormals(norms);
        result.SetUVs(0, uvs);
        result.SetTriangles(tris, 0);

        return result;
    }

    public static Mesh BooleanCut(Mesh source, Transform tool)
    {
        Mesh result = new Mesh();

        // first, we have to define a few lists for the new data to go into
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<int> tris = new List<int>();
        int[] oldTris = source.GetTriangles(0);

        List<ChangedVertex> changedVerts = new List<ChangedVertex>();
        List<ChangedVertex> changedVerts2 = new List<ChangedVertex>();

        List<int> adjustments = new List<int>();
        List<int> adjustmentValues = new List<int>();

        List<ChangedVertex> movedVertices = new List<ChangedVertex>();

        // STEP 1: make the new vertex list
        for (int i = 0; i < source.vertices.Length; i++)
        {
            // vertices and normals can both be added
            verts.Add(source.vertices[i]);
            norms.Add(source.normals[i]);
            
            // now, we want to remove a vertex if its inside our tool, and replace it with other new vertices
            // this function checks if the vertex is inside the tool
            if (IsPointInRect(source.vertices[i], tool))
            {
                Vector3 oldVertex = verts[verts.Count - 1]; // we'll need this later
                verts.RemoveAt(verts.Count - 1);
                Vector3 oldNorm = norms[norms.Count - 1]; // we'll need this later
                norms.RemoveAt(norms.Count - 1);

                adjustments.Add(i);
                adjustmentValues.Add(-1);

                changedVerts.Add(new ChangedVertex(i));

                // now that the old vertex is gone, we need to figure out where the new one(s) will be

                // we do this by fetching the indices of the vertices its connected to,
                // according to the triangles array
                // this makes it really easy to avoid making duplicate vertices,
                // avoiding weeding them out later
                int[] allConnectingIndices = GetConnectingIndices(i, oldTris);
                List<int> validConnectingIndices = new List<int>();

                // not only are we weeding dupes here, 
                // we're also weeding out any vertices that are INSIDE the tool
                for (int j = 0; j < allConnectingIndices.Length; j++)
                {
                    // the vertex list at this point is NOT complete,
                    // so we use the original array

                    if (!IsPointInRect(source.vertices[allConnectingIndices[j]], tool))
                    { // again, only outside vertices are allowed
                        
                        if (!validConnectingIndices.Contains(allConnectingIndices[j]))
                        { // and no dupes
                            
                            validConnectingIndices.Add(allConnectingIndices[j]);
                        }
                    }
                }

                // so, we've removed the vertices and we now have the valid connecting indicess
                // all we have to do now is make the new vertex positions!
                for (int j = 0; j < validConnectingIndices.Count; j++)
                {
                    Vector3 otherPoint = source.vertices[validConnectingIndices[j]];

                    verts.Add(GetPointInRect(otherPoint, oldVertex, tool));
                    norms.Add(oldNorm); // the old normal will probably be close enough

                    //debugPoints.Add(verts[verts.Count - 1]);
                    
                    adjustments.Add(i);
                    adjustmentValues.Add(1);

                    changedVerts[changedVerts.Count - 1].newIndices.Add(verts.Count-1);
                    changedVerts[changedVerts.Count - 1].connectingIndices.Add(validConnectingIndices[j]);
                }
            } 
        }

        // okay, cool, we cut a hole in the mesh.
        // NOW what we need to do is add the floor to that hole. 

        // because we need those verts that we removed after all, 
        // the simple thing to do would be to just move them insteaad of deleting them in the first place...

        // ... but I'm just going to re-add them.
        // this code only runs once, right?
        for (int i = 0; i < source.vertices.Length; i++)
        {
            for (int j = 0; j < changedVerts.Count; j++)
            {
                if (changedVerts[j].oldVertIndex == i)
                {
                    Vector3 p = source.vertices[i];
                    Vector3 d = -tool.up;
                    
                    Vector3 np = GetPointInRectSimple(p + d, tool);

                    movedVertices.Add(new ChangedVertex(i));
                    movedVertices[movedVertices.Count - 1].newIndices.Add(verts.Count);

                    // literally adding back the vertices that we removed
                    verts.Add(np);
                    norms.Add(-d);

                    //debugPoints.Add(np);

                    break;
                }
            }
        }

        // now adding another ring of changed vertices at the bottom
        for (int i = 0; i < changedVerts.Count; i++)
        {
            changedVerts2.Add(new ChangedVertex(changedVerts[i].oldVertIndex));

            for (int j = 0; j < changedVerts[i].newIndices.Count; j++)
            {
                Vector3 p = verts[changedVerts[i].newIndices[j]];
                Vector3 d = -tool.up;

                
                changedVerts2[changedVerts2.Count - 1].newIndices.Add(verts.Count);
                changedVerts2[changedVerts2.Count - 1].connectingIndices.Add(changedVerts[i].connectingIndices[j]);
                
                Vector3 np = GetPointInRectSimple(p + d, tool);

                // literally adding back the vertices that we removed
                verts.Add(np);
                norms.Add(-d);

                //debugPoints.Add(np);
            }
        }

        // and now onto the hard part, triangles
        // this is why we kept the data on what vertices came from what,
        // so that we could replace all the indices in the old list with the new vertices
        for (int i = 0; i < oldTris.Length; i+=3)
        {
            bool triangleValid = true;
            bool trianglePure = true;

            ChangedVertex[] changed = new ChangedVertex[3];
            ChangedVertex[] changed2 = new ChangedVertex[3];

            int badVertexCount = 0;

            int changeCount = 0; 
            for (int j = 0; j < 3; j++)
            {
                ChangedVertex found = FindChangedVertexMatch(changedVerts.ToArray(), oldTris[i+j]);
                ChangedVertex found2 = FindChangedVertexMatch(changedVerts2.ToArray(), oldTris[i+j]);
                if (found != null)
                {
                    trianglePure = false;
                
                    changed[j] = found;
                    changed2[j]=found2;
                    changeCount++;
                    if (found.newIndices.Count == 0)
                    {
                        triangleValid = false;
                        badVertexCount++;
                    }
                }
                else
                {
                    changed[j] = null;
                    changed2[j] = null;
                }
            }

            if (changed[0] != null && changed[1] != null && changed[2] != null)
            {
                triangleValid = false;
            }

            if (trianglePure)
            {
                tris.Add(oldTris[i]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i]));
                tris.Add(oldTris[i+1]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i+1]));
                tris.Add(oldTris[i+2]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i+2]));
            }
            else if (triangleValid)
            {
                Color col = new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f));
                
                if (changeCount == 2) // 3 verts, 1 tri
                {

                    // THESE triangles will also be related to the quads that form walls

                    if (changed[0] == null)
                    {
                        tris.Add(oldTris[i] + GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed[1], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed[2], oldTris[i]));

                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i+1]));
                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed2[1], oldTris[i]));

                        tris.Add(FindCorrectNewVertex(changed2[2], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed2[1], oldTris[i]));
                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i+2]));

                        
                        
                        tris.Add(FindCorrectNewVertex(changed[2], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed[1], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed2[1], oldTris[i]));

                        
                        
                        tris.Add(FindCorrectNewVertex(changed[2], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed2[1], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed2[2], oldTris[i]));
                    } else if (changed[1] == null)
                    {
                        tris.Add(FindCorrectNewVertex(changed[0], oldTris[i+1]));
                        tris.Add(oldTris[i+1]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed[2], oldTris[i+1]));

                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i+2]));
                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed2[2], oldTris[i+1]));

                        tris.Add(FindCorrectNewVertex(changed2[0], oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed2[2], oldTris[i+1]));
                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i]));

                        
                        
                        tris.Add(FindCorrectNewVertex(changed[0], oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed[2], oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed2[2], oldTris[i+1]));
                        
                        tris.Add(FindCorrectNewVertex(changed[0], oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed2[2], oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed2[0], oldTris[i+1]));

                    } else if (changed[2] == null)
                    {
                        tris.Add(FindCorrectNewVertex(changed[0], oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed[1], oldTris[i+2]));
                        tris.Add(oldTris[i+2]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i+2]));

                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i]));
                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed2[0], oldTris[i+2]));

                        tris.Add(FindCorrectNewVertex(changed2[1], oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed2[0], oldTris[i+2]));
                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i+1]));

                        
                        
                        tris.Add(FindCorrectNewVertex(changed[1], oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed[0], oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed2[0], oldTris[i+2]));
                        
                        tris.Add(FindCorrectNewVertex(changed[1], oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed2[0], oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed2[1], oldTris[i+2]));
                    }
                }
                else if (changeCount == 1) // these will result in 4 verts, 2 tris
                {
                    if (changed[0] != null)
                    {
                        tris.Add(FindCorrectNewVertex(changed[0], oldTris[i+1]));
                        tris.Add(oldTris[i+1]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i+1]));
                        tris.Add(oldTris[i+2]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i+2]));

                        tris.Add(FindCorrectNewVertex(changed[0], oldTris[i+1]));
                        tris.Add(oldTris[i+2]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed[0], oldTris[i+2]));

                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed2[0], oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed2[0], oldTris[i+2]));

                        
                        
                        tris.Add(FindCorrectNewVertex(changed[0], oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed2[0], oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed2[0], oldTris[i+1]));

                        
                        
                        tris.Add(FindCorrectNewVertex(changed[0], oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed[0], oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed2[0], oldTris[i+2]));
                    } else if (changed[1] != null)
                    {
                        tris.Add(oldTris[i]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed[1], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed[1], oldTris[i+2]));
                        
                        tris.Add(oldTris[i]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed[1], oldTris[i+2]));
                        tris.Add(oldTris[i+2]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i+2]));

                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed2[1], oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed2[1], oldTris[i]));



                        
                        
                        tris.Add(FindCorrectNewVertex(changed[1], oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed2[1], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed2[1], oldTris[i+2]));

                        
                        
                        
                        
                        tris.Add(FindCorrectNewVertex(changed[1], oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed[1], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed2[1], oldTris[i]));

                    } else if (changed[2] != null)
                    {
                        tris.Add(oldTris[i]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i]));
                        tris.Add(oldTris[i+1]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed[2], oldTris[i+1]));

                        tris.Add(oldTris[i]+ GetAdjustment(adjustments.ToArray(), adjustmentValues.ToArray(), oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed[2], oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed[2], oldTris[i]));

                        tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i+2]));
                        tris.Add(FindCorrectNewVertex(changed2[2], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed2[2], oldTris[i+1]));



                        
                        
                        tris.Add(FindCorrectNewVertex(changed[2], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed2[2], oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed2[2], oldTris[i]));
                        
                        
                        
                        
                        
                        
                        tris.Add(FindCorrectNewVertex(changed[2], oldTris[i]));
                        tris.Add(FindCorrectNewVertex(changed[2], oldTris[i+1]));
                        tris.Add(FindCorrectNewVertex(changed2[2], oldTris[i+1]));
                    }
                }
            }
            else
            {
                tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i]));
                tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i+1]));
                tris.Add(FindNewVertex(movedVertices.ToArray(), oldTris[i+2]));
            }
        }

        // right okay so we now have a vertices list thats COMPLETE,
        // may as well quickly do uvs
        Vector2[] uvs = new Vector2[verts.Count];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = Vector2.one;
        }

        // last but of course not least, we need to set all the data
        result.SetVertices(verts);
        result.SetNormals(norms);
        result.SetUVs(0, uvs);
        result.SetTriangles(tris, 0);

        return result;
    }

    public static int FindNewVertex(ChangedVertex[] c, int old)
    {
        for (int i = 0; i < c.Length; i++)
        {
            if (c[i].oldVertIndex == old && c[i].newIndices.Count > 0)
            {
                return c[i].newIndices[0];
            }
        }

        return -1;
    }

    public static int FindCorrectNewVertex(ChangedVertex c, int connection)
    {
        for (int i = 0; i < c.connectingIndices.Count; i++)
        {
            if (c.connectingIndices[i] == connection)
            {
                return c.newIndices[i];
            }
        }

        return -1;
    }

    public static int GetAdjustment(int[] adjustments, int[] values, int raw)
    {
        int sum = 0;

        for (int i = 0; i < adjustments.Length; i++)
        {
            if (adjustments[i] < raw)
            {
                sum += values[i];
            }
        }

        return sum;
    }

    public static ChangedVertex FindChangedVertexMatch(ChangedVertex[] data, int target)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i].oldVertIndex == target)
            {
                return data[i];
            }
        }

        return null;
    }

    public static int[] GetConnectingIndices(int v, int[] tris)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < tris.Length; i+=3)
        {
            if (tris[i] == v)
            {
                result.Add(tris[i+1]);
                result.Add(tris[i+2]);
            } else if (tris[i+1] == v)
            {
                result.Add(tris[i]);
                result.Add(tris[i+2]);
            } else if (tris[i+2] == v)
            {
                result.Add(tris[i]);
                result.Add(tris[i+1]);
            }
        }

        return result.ToArray();
    }

    public static bool IsPointInRect(Vector3 point, Transform tool)
    {
        Vector3 min = Vector3.one * -0.5f;
        Vector3 max = Vector3.one * 0.5f;
        return IsPointInAlignedBounds(tool.InverseTransformPoint(point), min, max);
    }

    // involves direction
    public static Vector3 GetPointInRect(Vector3 point, Vector3 desiredPoint, Transform tool)
    {
        Vector3 min = Vector3.one * -0.5f;
        Vector3 max = Vector3.one * 0.5f;
        return tool.TransformPoint(GetDirectedPointInAlignedBounds(tool.InverseTransformPoint(point), tool.InverseTransformPoint(desiredPoint) - tool.InverseTransformPoint(point), min, max));
    }

    // no direction, just clamping
    public static Vector3 GetPointInRectSimple(Vector3 point, Transform tool)
    {
        Vector3 min = Vector3.one * -0.5f;
        Vector3 max = Vector3.one * 0.5f;
        return tool.TransformPoint(GetPointInAlignedBounds(tool.InverseTransformPoint(point), min, max));
    }

    public static bool IsPointInAlignedBounds(Vector3 point, Vector3 minBounds, Vector3 maxBounds)
    {
        return point.x > minBounds.x && point.x < maxBounds.x
        && point.y > minBounds.y && point.y < maxBounds.y
        && point.z > minBounds.z && point.z < maxBounds.z;
    }

    public static Vector3 GetPointInAlignedBounds(Vector3 point, Vector3 minBounds, Vector3 maxBounds)
    {
        return new Vector3(
            Mathf.Clamp(point.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(point.y, minBounds.y, maxBounds.y),
            Mathf.Clamp(point.z, minBounds.z, maxBounds.z)
        );
    }

    public static Vector3 GetDirectedPointInAlignedBounds(Vector3 point, Vector3 dir, Vector3 minBounds, Vector3 maxBounds)
    {
        Vector3 raw = new Vector3(
            Mathf.Clamp(point.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(point.y, minBounds.y, maxBounds.y),
            Mathf.Clamp(point.z, minBounds.z, maxBounds.z)
        );

        float dist = (raw - point).magnitude;

        return point + dir.normalized * dist / Mathf.Cos(Vector3.Angle(dir, raw - point) * Mathf.PI / 180);
    }
}
