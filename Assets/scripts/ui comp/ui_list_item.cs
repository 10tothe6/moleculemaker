using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ui_list_item : MonoBehaviour
{
    public string label;
    public Sprite icon;

    public ui_list parent;
    public int itemIndex;

    public Image i_icon;
    public TextMeshProUGUI tx_label;

    void Update()
    {
        if (parent != null && CanvasUtils.IsCursorInteract(gameObject, true) && Mouse.current.leftButton.wasPressedThisFrame)
        {
            parent.whenItemClicked.Invoke(itemIndex);
        }
    }

    public void Set()
    {
        if (icon != null)
        {
            i_icon.sprite = icon;
        }
        tx_label.text = label;
    }
}
