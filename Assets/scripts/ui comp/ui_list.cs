using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ui_list : MonoBehaviour
{
    public GameObject p_item;
    public Transform t_itemContainer;
    public float itemSpacing;
    
    public float scrollSpeedFactor;
    private Vector3 scrollingOffset;

    public UnityEvent<int> whenItemClicked;

    // no add/delete functionality, that'll be in some sort of ui_editablelist component

    void Update()
    {
        if (CanvasUtils.IsCursorInteract(gameObject, true))
        {
            scrollingOffset += Vector3.up * Mouse.current.scroll.ReadValue().y * scrollSpeedFactor;
        }
        Refresh();
    }

    // based off of the currentFieldMarkers array
    public void Populate(Sprite[] icons, string[] labels)
    {
        // first, just clear everything bc I am lazy
        CanvasUtils.DestroyChildren(t_itemContainer.gameObject);

        // NOW we populate
        for (int i = 0; i < icons.Length; i++)
        {
            GameObject newItem = Instantiate(p_item, t_itemContainer);

            ui_list_item comp = newItem.GetComponent<ui_list_item>();
            comp.itemIndex = i;
            comp.parent = this;
            comp.label = labels[i];
            comp.icon = icons[i];

            comp.Set();
            
        }
        Refresh();
    }

    public void TestItemEvent(int num)
    {
        Debug.Log(num);
    }

    // just updating the positions, not re-making the objects
    public void Refresh()
    {
        for (int i = 0; i < t_itemContainer.childCount; i++)
        {
            t_itemContainer.GetChild(i).localPosition = - Vector3.up * (i + 1) * itemSpacing + scrollingOffset;
        }
    }
}
