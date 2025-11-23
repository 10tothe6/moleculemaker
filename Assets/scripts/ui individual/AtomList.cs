using UnityEngine;

public class AtomList : MonoBehaviour
{
    void Start()
    {
        GetComponent<ui_list>().whenItemClicked.AddListener( (x) => MoleculeConstructor.Instance.GrabAtom(x));

        GetComponent<ui_list>().Populate(MoleculeConstructor.Instance.atomIcons, MoleculeConstructor.Instance.atomNames);
    }
}
