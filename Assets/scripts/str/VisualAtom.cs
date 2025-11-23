using UnityEngine;

public class VisualAtom : MonoBehaviour
{
    public int type;

    public void SetType()
    {
        GetComponent<MeshRenderer>().sharedMaterial = MoleculeConstructor.Instance.m_atoms[type];
        transform.localScale = Vector3.one * MoleculeConstructor.Instance.atomSizes[type];
    }
}
