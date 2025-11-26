using UnityEngine;

// takes in the resulting molecule, and exports it
public class Exporter : MonoBehaviour
{
    private static Exporter _instance;

    public static Exporter Instance
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

    public float scalingFactor;

    private str_molecule toExport;
    public void RunExport(str_molecule input)
    {
        toExport = input;
    }
}
