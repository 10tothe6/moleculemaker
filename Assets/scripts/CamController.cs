using UnityEngine;
using UnityEngine.InputSystem;

public enum CameraMode
{
    Freecam,
    Orbit
}

public class CamController : MonoBehaviour
{
    public ushort mode;

    public float translationSpeed;

    private float hSpeed;
    private float vSpeed;

    public float orbitDistance;
    public float orbitSpeed;
    public float scrollSpeed;

    private Vector3 orbitPoint;

    void Awake()
    {
        mode = 1;
        orbitDistance = 20;
        
        orbitPoint = Vector3.zero;
    }

    void Update()
    {
        if (mode == (ushort)CameraMode.Freecam)
        {
            transform.position += 
            transform.right * translationSpeed * Time.deltaTime * hSpeed + 
            transform.up * translationSpeed * Time.deltaTime * vSpeed;
        } else if (mode == (ushort)CameraMode.Orbit)
        {
            if (Mouse.current.rightButton.isPressed)
            {
                transform.Rotate(Vector3.up * Time.deltaTime * Mouse.current.delta.ReadValue().x * -orbitSpeed, Space.World);
                transform.Rotate(Vector3.right * Time.deltaTime * Mouse.current.delta.ReadValue().y * orbitSpeed, Space.Self);
            }

            if (!CanvasUtils.IsInteractingWithUI())
            {
                orbitDistance += Mouse.current.scroll.ReadValue().y * scrollSpeed;
            }

            transform.position = orbitPoint - transform.forward * orbitDistance;
            
            orbitPoint += 
            Vector3.right * translationSpeed * Time.deltaTime * hSpeed + 
            Vector3.forward * translationSpeed * Time.deltaTime * vSpeed;
        }

        if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed)
        {
            hSpeed = -1;
        } else if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed)
        {
            hSpeed = 1;
        }

        if (UnityEngine.InputSystem.Keyboard.current.qKey.isPressed)
        {
            vSpeed = -1;
        } else if (UnityEngine.InputSystem.Keyboard.current.eKey.isPressed)
        {
            vSpeed = 1;
        }

        hSpeed = Mathf.Lerp(hSpeed, 0, 0.1f);
        vSpeed = Mathf.Lerp(vSpeed, 0, 0.1f);
    }
}
