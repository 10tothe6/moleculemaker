using TMPro;
using UnityEngine;

// this bit of UI is meant to be modular (way more so compared to Drivetools V1), allowing you to dock the toolbar on any edge of the screen,
// OR keep it floating as a window
public enum DockingMode
{
    Floating,
    DockedUpper,
    DockedLeft,
    DockedLower,
    DockedRight,
}

// okay, so a lot of the menus need to be moved around, and docked on the edges of the screen

// basically the way we're gonna do this is by changing the effective width/height of the screen,
// that way docked nodes are just ignored

public class DockingManager : MonoBehaviour
{
    private static DockingManager _instance;

    public static DockingManager Instance
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

    // showing how many pixels for each edge of the screen are "occupied"
    public TextMeshProUGUI tx_bottomDebug;
    public TextMeshProUGUI tx_topDebug;
    public TextMeshProUGUI tx_leftDebug;
    public TextMeshProUGUI tx_rightDebug;

    public float occupiedUpperArea;
    public float occupiedLowerArea;
    public float occupiedLeftArea;
    public float occupiedRightArea;

    void Update()
    {
        // really only used for debug purposes
        tx_bottomDebug.text = occupiedLowerArea.ToString();
        tx_topDebug.text = occupiedUpperArea.ToString();
        tx_leftDebug.text = occupiedLeftArea.ToString();
        tx_rightDebug.text = occupiedRightArea.ToString();
    }

    public static float EffectiveUpper()
    {
        return Screen.height - Instance.occupiedUpperArea;
    }
    public static float EffectiveLower()
    {
        return Instance.occupiedLowerArea;
    }
    public static float EffectiveLeft()
    {
        return Instance.occupiedLeftArea;
    }
    public static float EffectiveRight()
    {
        return Screen.width - Instance.occupiedRightArea;
    }

    // *** docking functions ***
    // right so we don't *really* need to have horizontal and vertical as two separate things,
    // but I'm doing it that way in case changes need to be made in the future
    // good to have a little bit more data, right?

    // basically, increase whichever margin the menu is now taking up
    public void Dock(ushort dockingMode, float minorWidth)
    {
        if (dockingMode == (ushort)DockingMode.DockedUpper)
        {
            occupiedUpperArea += minorWidth;
        }
        else if (dockingMode == (ushort)DockingMode.DockedLower)
        {
            occupiedLowerArea += minorWidth;
        }
        else if (dockingMode == (ushort)DockingMode.DockedLeft)
        {
            occupiedLeftArea += minorWidth;
        }
        else if (dockingMode == (ushort)DockingMode.DockedRight)
        {
            occupiedRightArea += minorWidth;
        }
    }
    // this is called EVERY TIME that a users "grabs" a menu, 
    // but if the menu was floating before it just means that nothing happens
    public void Undock(ushort dockingMode, float minorWidth)
    {
        if (dockingMode == (ushort)DockingMode.DockedUpper)
        {
            occupiedUpperArea -= minorWidth;
        }
        else if (dockingMode == (ushort)DockingMode.DockedLower)
        {
            occupiedLowerArea -= minorWidth;
        }
        else if (dockingMode == (ushort)DockingMode.DockedLeft)
        {
            occupiedLeftArea -= minorWidth;
        }
        else if (dockingMode == (ushort)DockingMode.DockedRight)
        {
            occupiedRightArea -= minorWidth;
        }
    }
    // ***
}
