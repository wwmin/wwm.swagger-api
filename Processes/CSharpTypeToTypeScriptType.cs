using swagger2js_cli.Models;

using System.Text;

namespace swagger2js_cli.Processes;

public static class CSharpTypeToTypeScriptType
{
    #region Convert Type

    /// <summary>
    /// csharp type to typescript type
    /// </summary>
    /// <param name="csharpType"></param>
    /// <returns></returns>
    public static string Convert(string? refString, string csharpType)
    {
        Console.WriteLine(csharpType);
        if (csharpType == null)
        {
            return "any";
        }
        else if (csharpType == "string")
        {
            return "string";
        }
        else if (csharpType == "int")
        {
            return "number";
        }
        else if (csharpType == "integer")
        {
            return "number";
        }
        else if (csharpType == "long")
        {
            return "number";
        }
        else if (csharpType == "float")
        {
            return "number";
        }
        else if (csharpType == "double")
        {
            return "number";
        }
        else if (csharpType == "bool" || csharpType == "boolean")
        {
            return "boolean";
        }
        else if (csharpType == "DateTime")
        {
            return "Date";
        }
        else if (csharpType == "DateTimeOffset")
        {
            return "Date";
        }
        else if (csharpType == "byte[]")
        {
            return "string";
        }
        else if (csharpType == "object")
        {
            var t = ParseRefType(refString);
            return t ?? "any";
        }
        else if (csharpType == "array")
        {
            var t = ParseRefType(refString);
            return t == null ? "any[]" : t + "[]";
        }
        else
        {
            return csharpType;
        }
    }


    public static string Convert(string? refString, string csharpType, bool? isNullable)
    {
        if (isNullable != null && isNullable == true)
        {
            return Convert(refString, csharpType) + " | null";
        }
        else
        {
            return Convert(refString, csharpType);
        }
    }

    public static string? ParseRefType(string refString)
    {
        if (string.IsNullOrEmpty(refString))
        {
            return null;
        }
        //取ref string的最后一个
        string[] refs = refString.Split('/');
        return refs[refs.Length - 1];
    }
    #endregion



}
