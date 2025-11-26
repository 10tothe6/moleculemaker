using UnityEngine;

public class Test : MonoBehaviour
{
    public MeshFilter a;
    public MeshFilter b;

    public MeshFilter r;

    void Start()
    {
        r.sharedMesh = MeshUtils.CombineMeshes(new Mesh[] {a.sharedMesh, b.sharedMesh});
    }
}
