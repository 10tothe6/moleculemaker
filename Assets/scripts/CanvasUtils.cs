using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// A support class with functions for UI
// This class can be referenced by any other script in the project
public class CanvasUtils : MonoBehaviour
{
    public static List<GameObject> GetImageObjectsInChildren(GameObject parent, bool inclueParent)
    {
        List<GameObject> toReturn = new List<GameObject>();
        if (inclueParent) { toReturn.Add(parent); }

        Image[] imagesComponents = parent.GetComponentsInChildren<Image>(true);

        for (int i = 0; i < imagesComponents.Length; i++)
        {
            toReturn.Add(imagesComponents[i].gameObject);
        }

        return toReturn;
    }

    // -- CHILD FUNCTIONS -- //
    // Changing data of all children

    public static void SetTransparencyOfChildren(GameObject inputObject, float a, bool force)
    {
        Image[] images = inputObject.GetComponentsInChildren<Image>(true);

        for (int i = 0; i < images.Length; i++)
        {
            if (!force && images[i].color.a == 0)
            {
                images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, images[i].color.a);
            }
            else
            {
                images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, a);
            }
        }
    }

    public static void DestroyChildren(GameObject inputObject)
    {
        Transform[] toDestroy = inputObject.GetComponentsInChildren<Transform>(true);
        for (int i = toDestroy.Length - 1; i >= 0; i--)
        {
            if (toDestroy[i].gameObject != inputObject)
            {
                Destroy(toDestroy[i].gameObject);
            }
            toDestroy[i] = null;
        }
    }

    public static void SetChildrenActive(GameObject inputObject, bool active)
    {
        Transform[] toDestroy = inputObject.GetComponentsInChildren<Transform>(true);
        for (int i = toDestroy.Length - 1; i >= 0; i--)
        {
            if (toDestroy[i].gameObject != inputObject)
            {
                toDestroy[i].gameObject.SetActive(active);
            }
            toDestroy[i] = null;
        }
    }

    public static Transform SearchChildrenForName(GameObject inputObject, string name)
    {
        Transform[] children = inputObject.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].gameObject.name == name)
            {
                // found the name, so return
                return children[i];
            }
        }

        // tried to find the object and did not find it
        Debug.Log("ERROR: Attempt to find an object of name " + name + " failed, object does not exist");
        return null;
    }

    // -- INTERACTION FUNCTIONS -- //
    // These use the order of children in the heirarchy to check interaction with the cursor

    // Detect whether the cursor is interacting with a supplied object
    public static bool IsCursorInteract(GameObject inputObject, bool ignoreChildren)
    {
        RectTransform[] canvasObjects = UIManager.Instance.t_canvas.GetComponentsInChildren<RectTransform>();

        // loop backwards through the canvas objects
        // only works if they are organized in order
        for (int i = canvasObjects.Length - 1; i > 0; i--)
        {
            if (canvasObjects[i].gameObject == inputObject && IsCursorInBounds(inputObject, false))
            {
                return true; // if we reach the input return true
            }
            else if (canvasObjects[i].gameObject.activeSelf && IsCursorInBounds(canvasObjects[i].gameObject, false) && canvasObjects[i].gameObject.GetComponent<Image>() != null)
            {
                if (ignoreChildren && canvasObjects[i].transform.IsChildOf(inputObject.transform)) { continue; }
                return false;
            }
        }

        if (inputObject == UIManager.Instance.t_canvas.gameObject) { return true; }

        return false; // this should happen if the cursor isn't in the bounding box of the object at all
    }

    // Detect whether the cursor is interacting with a supplied object,
    // while ALSO IGNORING CERtAIN GAMEOBJECTS
    public static bool IsCursorInteract(GameObject inputObject, bool ignoreChildren, List<GameObject> ignoreObjects)
    {
        RectTransform[] canvasObjects = UIManager.Instance.t_canvas.GetComponentsInChildren<RectTransform>();

        // loop backwards through the canvas objects
        // only works if they are organized in order
        for (int i = canvasObjects.Length - 1; i > 0; i--)
        {
            if (canvasObjects[i].gameObject == inputObject && IsCursorInBounds(inputObject, false))
            {
                return true; // if we reach the input return true
            }
            else if (canvasObjects[i].gameObject.activeSelf && IsCursorInBounds(canvasObjects[i].gameObject, false) && canvasObjects[i].gameObject.GetComponent<Image>() != null)
            {
                if (ignoreChildren && canvasObjects[i].transform.IsChildOf(inputObject.transform)) { continue; }
                if (ignoreObjects.Contains(canvasObjects[i].gameObject)) { continue; }
                Debug.Log(canvasObjects[i].gameObject.name);
                return false;
            }
        }

        if (inputObject == UIManager.Instance.t_canvas.gameObject) { return true; }

        return false; // this should happen if the cursor isn't in the bounding box of the object at all
    }

    // Detect whether the cursor is interacting with a supplied object,
    // THIS ONE WORKS WITH MANY OBJECTS, AND REQUIRES ONE OF THEM
    public static bool IsCursorInteractOR(List<GameObject> inputObjects, bool ignoreChildren)
    {
        bool isInteracting = false;

        for (int i = 0; i < inputObjects.Count; i++)
        {
            if (IsCursorInteract(inputObjects[i], ignoreChildren))
            {
                isInteracting = true;
                break;
            }
        }

        return isInteracting;
    }
    // Detect whether the cursor is interacting with a supplied object,
    // THIS ONE WORKS WITH MANY OBJECTS, AND REQUIRES THEM ALL
    public static bool IsCursorInteractAND(List<GameObject> inputObjects, bool ignoreChildren)
    {
        bool isInteracting = true;

        for (int i = 0; i < inputObjects.Count; i++)
        {
            if (!IsCursorInteract(inputObjects[i], ignoreChildren))
            {
                isInteracting = false;
                break;
            }
        }

        return isInteracting;
    }

    // Detect whether the cursor is within the bounds of a supplied object
    public static bool IsCursorInBounds(GameObject inputObject, bool mustBeActive)
    {
        Vector3[] v = new Vector3[4];
        inputObject.GetComponent<RectTransform>().GetWorldCorners(v);

        Vector2 min = new Vector2(v[0].x, v[0].y);
        Vector2 max = new Vector2(v[2].x, v[2].y);

        if (!inputObject.activeSelf && mustBeActive) { return false; }
        // check if the cursor is inside the object's 'bounding box'
        if (Mouse.current.position.ReadValue().x > min.x
        && Mouse.current.position.ReadValue().x < max.x
        && Mouse.current.position.ReadValue().y > min.y
        && Mouse.current.position.ReadValue().y < max.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // -- ARRAY MANIPULATION FUNCTIONS -- // 

    public static Vector3[] RemoveDuplicatePoints(Vector3[] points)
    {
        List<Vector3> toReturn = new List<Vector3>();
        for (int i = 0; i < points.Length; i++)
        {
            if (!toReturn.Contains(points[i]))
            {
                toReturn.Add(points[i]);
            }
        }
        return toReturn.ToArray();
    }

    public static Vector3[] RemovePoint(Vector3[] points, Vector3 toRemove)
    {
        List<Vector3> toReturn = new List<Vector3>();
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != toRemove)
            {
                toReturn.Add(points[i]);
            }
        }
        return toReturn.ToArray();
    }

    public static int GetIndexOfMinimum(float[] values)
    {
        int minIndex = -1;
        float min = -1;
        for (int i = 0; i < values.Length; i++)
        {
            if (min == -1 || values[i] < min)
            {
                minIndex = i;
                min = values[i];
            }
        }
        return minIndex;
    }


    // public static void ApplyColorPalette() {
    //     Image[] images = UIManager.Instance.canvasTransform.gameObject.GetComponentsInChildren<Image>();
    //     for (int i = 0; i < images.Length; i++) {
    //         if (images[i].color == AppData.Instance.editorColor1) {
    //             images[i].color = AppData.Instance.color1;
    //         } else if (images[i].color == AppData.Instance.editorColor2) {
    //             images[i].color = AppData.Instance.color2;
    //         } else if (images[i].color == AppData.Instance.editorColor3) {
    //             images[i].color = AppData.Instance.color3;
    //         } else if (images[i].color == AppData.Instance.editorColor4) {
    //             images[i].color = AppData.Instance.color4;
    //         }
    //     }
    // }

    // public static void ApplyColorPaletteToObject(GameObject inputObject) {
    //     Image image = inputObject.GetComponent<Image>();
    //     if (image.color == AppData.Instance.editorColor1) {
    //         image.color = AppData.Instance.color1;
    //     } else if (image.color == AppData.Instance.editorColor2) {
    //         image.color = AppData.Instance.color2;
    //     } else if (image.color == AppData.Instance.editorColor3) {
    //         image.color = AppData.Instance.color3;
    //     } else if (image.color == AppData.Instance.editorColor4) {
    //         image.color = AppData.Instance.color4;
    //     }
    // }

    public static Color MakeOpaque(Color inputColor)
    {
        return new Color(inputColor.r, inputColor.g, inputColor.b, 1);
    }
}