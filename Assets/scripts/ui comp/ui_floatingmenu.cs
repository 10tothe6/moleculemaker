using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// works along with ToolbarMenu.cs
public class ui_floatingmenu : MonoBehaviour
{
    public GameObject g_menuParent;
    public GameObject g_interactPoint;

    public bool isGrabbed;
    public ushort state;

    private Vector3 cursorOffset;

    public UnityAction handlePositioning;
    public UnityAction onGrab;

    void Awake()
    {
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && CanvasUtils.IsCursorInteract(g_interactPoint, false))
        {
            isGrabbed = true;
            if (onGrab != null) onGrab.Invoke();
            state = (ushort)DockingMode.Floating;

            cursorOffset = transform.position - (Vector3)Mouse.current.position.ReadValue();
        }
        if (!Mouse.current.leftButton.isPressed)
        {
            isGrabbed = false;
        }

        if (handlePositioning != null) handlePositioning.Invoke();

        if (isGrabbed && state == (ushort)DockingMode.Floating)
        {
            Vector2 mPos = Mouse.current.position.ReadValue();
            transform.position = new Vector3(mPos.x, mPos.y, 0) + cursorOffset;
        }
    }
}
