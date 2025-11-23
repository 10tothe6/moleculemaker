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

    void Awake()
    {
        mode = 1;
        orbitDistance = 20;
    }

    void Update()
    {
        if (mode == (ushort)CameraMode.Freecam)
        {
            transform.position += 
            transform.right * translationSpeed * Time.deltaTime * hSpeed + 
            transform.up * translationSpeed * Time.deltaTime * vSpeed;

            hSpeed = Mathf.Lerp(hSpeed, 0, 0.1f);
            vSpeed = Mathf.Lerp(vSpeed, 0, 0.1f);

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
        } else if (mode == (ushort)CameraMode.Orbit)
        {
            if (Mouse.current.rightButton.isPressed)
            {
                transform.Rotate(Vector3.up * Time.deltaTime * Mouse.current.delta.ReadValue().x * -orbitSpeed, Space.World);
                transform.Rotate(Vector3.right * Time.deltaTime * Mouse.current.delta.ReadValue().y * orbitSpeed, Space.Self);
            }

            orbitDistance += Mouse.current.scroll.ReadValue().y * scrollSpeed;

            transform.position = Vector3.zero - transform.forward * orbitDistance;
        }
    }
}
