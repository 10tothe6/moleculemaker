using UnityEngine.UI;
using UnityEngine;

public class AtomList : MonoBehaviour
{
    void Start()
    {
        GetComponent<ui_list>().whenItemClicked.AddListener( (x) => MoleculeConstructor.Instance.GrabAtom(x));

        GetComponent<ui_list>().Populate(MoleculeConstructor.Instance.atomIcons, MoleculeConstructor.Instance.atomNames);

        for (int i = 0; i < GetComponent<ui_list>().t_itemContainer.childCount; i++)
        {
            GetComponent<ui_list>().t_itemContainer.GetChild(i).GetChild(0).GetComponent<Image>().color = MoleculeConstructor.Instance.m_atoms[i].color;
        }
    }
}
