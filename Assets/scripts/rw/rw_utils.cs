using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

// utility class, contains functions for modifying files/file paths
public class rw_utils : MonoBehaviour
{
    // this ends up being the appdata/locallow/fakevoxel/etc. directory for windows
    public static string saveDirectory;

    // the current version of the software, this is organized by year.update.hotfix
    // so, the first build for 2025 is 2025.0.1
    // (same as most frc-related software)
    public static string buildVersion = "2025.0.1";

    public static rw_userprefs prefs;

    public Sprite[] ins_fileIcons;
    public static Sprite[] fileIcons;

    void Awake()
    {
        rw_utils.fileIcons = ins_fileIcons;
        rw_utils.saveDirectory = UnityEngine.Application.persistentDataPath;
    }

    public static string[] GetFoldersInPath(string path)
    {
        return Directory.GetDirectories(path);
    }
    public static string[] GetFilesInPath(string path)
    {
        return Directory.GetFiles(path);
    }

    // *****************************************************************************
    public static void SavePreferences(rw_userprefs _prefs)
    {
        string savePath = saveDirectory + "/" + "v" + buildVersion + "/";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "prefs.prf", FileMode.Create);

        formatter.Serialize(stream, _prefs);
        stream.Close();
    }

    public static rw_userprefs LoadPreferences()
    {
        string loadPath = saveDirectory + "/" + "v" + buildVersion + "/";

        if (File.Exists(loadPath + "prefs.prf"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "prefs.prf", FileMode.Open);

            rw_userprefs prefs = formatter.Deserialize(stream) as rw_userprefs;
            // TODO: add try/catch block here
            stream.Close();
            return prefs;
        }
        else
        {
            return null;
        }
    }

    public static void SaveMolecule(str_molecule _mol)
    {
        string savePath = rw_userprefs.moleculeDirectory;

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "molecule.mol", FileMode.Create);

        formatter.Serialize(stream, _mol);
        stream.Close();
    }

    public static str_molecule LoadMolecule()
    {
        string loadPath = rw_userprefs.moleculeDirectory;

        if (File.Exists(loadPath + "molecule.mol"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "molecule.mol", FileMode.Open);

            str_molecule mol = formatter.Deserialize(stream) as str_molecule;
            // TODO: add try/catch block here
            stream.Close();
            return mol;
        }
        else
        {
            return null;
        }
    }

    // *****************************************************************************

    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    public static void SavePNG(Texture2D image) {

        string savePath = saveDirectory + "/" + "v" + buildVersion + "/default assets/";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        BinaryFormatter formatter = new BinaryFormatter();

        File.WriteAllBytes(savePath + image.name, image.EncodeToPNG());
    }

    public static string[] GetAllDefaultAssetDirectories() {
        List<string> toReturn = new List<string>();

        string[] filePaths = Directory.GetFiles(saveDirectory + "/" + "v" + buildVersion + "/default assets/");

        for (int i = 0; i < filePaths.Length; i++) {
            toReturn.Add(filePaths[i]);
        }

        return toReturn.ToArray();
    }

    // replaces one line of a text file with a string
    //returns null if goes right, return error message if goes wrong
    public static string ModifyTxtWithNoSurprises(string file, int lineIndex, string newText)
    {
        try
        {
            var lines = File.ReadAllLines(file);
            lines[lineIndex] = newText;
            File.WriteAllLines(file, lines);
        }
        catch (Exception e)
        {
            return e.Message;
        }
        return null;
    }


    // takes out the last directory of a file path, like going out of a folder
    public static string RemoveLastDirectory(string directory)
    {
        for (int i = directory.Length - 1; i >= 0; i--)
        {
            if (directory[i] == '/' || directory[i] == '\\')
            {
                return directory.Substring(0, i);
            }
        }
        return "";
    }
    
    public static string GetLastDirectory(string directory) {
        for (int i = directory.Length - 1; i >= 0; i--) {
            if (directory[i] == '/' || directory[i] == '\\') {
                return directory.Substring(i, directory.Length - (i));
            }
        }
        return "";
    }

    // gets the file name, given the file path (removes the extension)
    // c:/users/me/desktop/map.png --> map
    // c:/users/me/documents/file --> file
    public static string GetFileName(string path)
    {
        int startIndex = 0;
        for (int i = path.Length - 1; i >= 0; i--)
        {
            if (path[i] == '/' || path[i] == '\\')
            {
                startIndex = i + 1;
                break;
            }
        }

        int endIndex = path.Length - 1;
        if (path.LastIndexOf(".") > startIndex)
        {
            UnityEngine.Debug.Log(path);

            endIndex = 0;
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '.')
                {
                    endIndex = i;
                    break;
                }
            }
        }

        return path.Substring(startIndex, endIndex - startIndex);
    }

    // sets this application as the default driverstation, by modifying the FRC Driverstation settings file
    public void SetAsDefaultDriverstation() {
        ModifyTxtWithNoSurprises(
            "C:/Users/Public/Documents/FRC/FRC DS Data Storage.ini", 
            3, 
            RemoveLastDirectory("DashboardCmdLine = " + UnityEngine.Application.dataPath) + "Drivetools.exe");
    }
}