using UnityEngine;

// i COULD have multiple classes for different categories of settings,
// but having it all in one class really does make my life easier

[System.Serializable]
public class rw_userprefs
{
    public static string moleculeDirectory = "C:\\Users\\maxim\\Desktop\\molecule export\\";
    public static rw_userprefs FactoryDefaults()
    {
        rw_userprefs result = new rw_userprefs();

        return result;
    }
}
