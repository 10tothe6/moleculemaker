using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class DoubleVector3
{
    public double x;
    public double y;
    public double z;

    public DoubleVector3(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public DoubleVector3(Vector3 v)
    {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
    }

    public DoubleVector3 Mul(double other)
    {
        return new DoubleVector3(x * other, y * other, z * other);
    }
    public DoubleVector3 Div(double other)
    {
        return new DoubleVector3(x / other, y / other, z / other);
    }
    public DoubleVector3 Add(DoubleVector3 other)
    {
        return new DoubleVector3(other.x + x, other.y + y, other.z + z);
    }
    public DoubleVector3 Add(Vector3 other)
    {
        return new DoubleVector3(other.x + x, other.y + y, other.z + z);
    }
    public DoubleVector3 Sub(DoubleVector3 other)
    {
        return new DoubleVector3(x - other.x, y - other.y, z - other.z);
    }
    public DoubleVector3 Sub(Vector3 other)
    {
        return new DoubleVector3(other.x - x, other.y - y, other.z - z);
    }
    public DoubleVector3 Norm()
    {
        double l = Math.Sqrt(x * x + y * y + z * z);
        return new DoubleVector3(x / l, y / l, z / l);
    }
    public double Mag()
    {
        return Math.Sqrt(x * x + y * y + z * z);
    }


    public Vector3 ToVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }

    public static Vector2[] ToVector2Array(DoubleVector3[] input)
    {
        Vector2[] result = new Vector2[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            result[i] = new Vector2((float)input[i].x, (float)input[i].y);
        }

        return result;
    }
    public static Vector3[] ToVector3Array(DoubleVector3[] input)
    {
        Vector3[] result = new Vector3[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            result[i] = new Vector3((float)input[i].x, (float)input[i].y, (float)input[i].z);
        }

        return result;
    }

    public static string EncodeToString(DoubleVector3 input)
    {
        string result = "";

        result += "vector3(";

        result += "{" + input.x.ToString() + "}" + "{" + input.y.ToString() + "}" + "{" + input.z.ToString() + "}";

        result += ")";

        return "$" + result;
    }

    // this script originally came from Drivetools, so it had some encoding/decoding shit
    // of course we don't need that here

    // public static DoubleVector3 DecodeFromString(string input)
    // {
    //     List<string> components = new List<string>();

    //     for (int i = 0; i < 3; i++)
    //     {
    //         int startIndex = Sys.FindOccurance(input, '{', i);
    //         int endIndex = Sys.FindClosingBracket(input, startIndex);

    //         components.Add(input.Substring(startIndex + 1, endIndex - startIndex - 1));
    //     }

    //     return new DoubleVector3(double.Parse(components[0]), double.Parse(components[1]), double.Parse(components[2]));
    // }

    // // can also encode from a ref, if its faster (which it usually is)
    // public string EncodeToString()
    // {
    //     string result = "";

    //     result += "vector3(";

    //     result += "{" + x.ToString() + "}" + "{" + y.ToString() + "}" + "{" + z.ToString() + "}";

    //     result += ")";

    //     return "$" + result;
    // }

    // Vector2 -> DoubleVector3
    public static DoubleVector3[] ToArray(Vector2[] input)
    {
        DoubleVector3[] result = new DoubleVector3[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            result[i] = new DoubleVector3(input[i].x, input[i].y, 0);
        }

        return result;
    }
    // Vector3 -> DoubleVector3
    public static DoubleVector3[] ToArray(Vector3[] input)
    {
        DoubleVector3[] result = new DoubleVector3[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            result[i] = new DoubleVector3(input[i].x, input[i].y, input[i].z);
        }

        return result;
    }
}