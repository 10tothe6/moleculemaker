using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    public static UIManager Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null)
            {
                _instance = value;
            }
            else if (_instance != value)
            {
                Debug.Log("Duplicate NetworkManager instance in scene!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public Transform t_canvas;

    public GameObject g_controlMenu;
    public GameObject g_newAtomMenu;
    public GameObject g_infoMenu;

    void Start()
    {
        g_controlMenu.SetActive(true);
        g_newAtomMenu.SetActive(true);
        g_infoMenu.SetActive(false);
    }

    public void ToggleInfoMenu()
    {
        g_infoMenu.SetActive(!g_infoMenu.activeSelf);
    }
    public void ToggleNewAtomMenu()
    {
        g_infoMenu.SetActive(!g_newAtomMenu.activeSelf);
    }
    public void ToggleControlMenu()
    {
        g_infoMenu.SetActive(!g_controlMenu.activeSelf);
    }
}
