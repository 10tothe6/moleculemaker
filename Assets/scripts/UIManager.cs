using TMPro;
using UnityEngine;

public enum UIMode
{
    Normal,
    NoUI,
    IndexAtoms,
}

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

    public GameObject g_newAtomMenu;
    public GameObject g_infoMenu;
    public GameObject g_simMenu;
    public GameObject g_bugMenu;
    public GameObject g_exportMenu;
    public GameObject g_settingsMenu;

    public GameObject hudObject;

    public Transform t_atomLabelContainer;
    public GameObject p_atomLabel;

    private ushort uiMode;

    void Start()
    {
        g_newAtomMenu.SetActive(true);
        g_infoMenu.SetActive(false);
    }

    public void AdvanceUIMode()
    {
        uiMode++;
        if (uiMode == 3)
        {
            uiMode = 0;
        }

        UpdateUIMode();
    }

    void UpdateUIMode()
    {
        hudObject.SetActive(uiMode != (ushort)UIMode.NoUI);

        if (uiMode == (ushort)UIMode.Normal)
        {
            
        } else if (uiMode == (ushort)UIMode.NoUI)
        {
            
        } else if (uiMode == (ushort)UIMode.IndexAtoms)
        {
            // looping through every atom and giving each a label
            GenerateAtomLabels();
        }
    }

    public void GenerateAtomLabels()
    {
        CanvasUtils.DestroyChildren(t_atomLabelContainer.gameObject);

        for (int i = 0; i < MoleculeConstructor.Instance.atomObjects.Count; i++)
        {
            GameObject g_newLabel = Instantiate(p_atomLabel, t_atomLabelContainer);
            g_newLabel.GetComponent<TextMeshProUGUI>().text = i.ToString();
        }
    }

    void RefreshAtomLabels()
    {
        // just updating the position is all
        int n = 0;
        for (int i = 0; i < MoleculeConstructor.Instance.atomObjects.Count; i++)
        {
            t_atomLabelContainer.GetChild(n).gameObject.SetActive(uiMode == (ushort)UIMode.IndexAtoms);
            t_atomLabelContainer.GetChild(n).position = Camera.main.WorldToScreenPoint(MoleculeConstructor.Instance.atomObjects[i].position);
        }
    }

    void Update()
    {
        RefreshAtomLabels();
    }

    public void ToggleInfoMenu()
    {
        g_infoMenu.SetActive(!g_infoMenu.activeSelf);
    }
    public void ToggleNewAtomMenu()
    {
        g_newAtomMenu.SetActive(!g_newAtomMenu.activeSelf);
    }
    public void ToggleSimMenu()
    {
        g_simMenu.SetActive(!g_simMenu.activeSelf);
    }
    public void ToggleBugMenu()
    {
        g_bugMenu.SetActive(!g_bugMenu.activeSelf);
    }
    public void ToggleSettingsMenu()
    {
        g_settingsMenu.SetActive(!g_settingsMenu.activeSelf);
    }
    public void ToggleExportMenu()
    {
        g_exportMenu.SetActive(!g_exportMenu.activeSelf);
    }
}
