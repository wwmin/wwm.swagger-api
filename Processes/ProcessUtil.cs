﻿using System.Text;

namespace wwm.swagger_api.Processes;

public static class ProcessUtil
{
    #region Convert Type

    /// <summary>
    /// csharp type to typescript type
    /// </summary>
    /// <param name="csharpType"></param>
    /// <returns></returns>
    public static string Convert(string? refString, string? csharpType)
    {
        switch (csharpType)
        {
            case null:
                return ParseRefType(refString) ?? "any";
            case "string":
                return "string";
            case "int":
            case "integer":
            case "long":
            case "float":
            case "double":
                return "number";
            case "bool":
            case "boolean":
                return "boolean";
            case "DateTime":
                return "Date";
            case "DateTimeOffset":
                return "Date";
            case "byte[]":
                return "string";
            case "object":
                {
                    var t = Convert(null, ParseRefType(refString));
                    return t ?? "any";
                }
            case "array":
                {
                    var t = Convert(null, ParseRefType(refString));
                    return t == null ? "any[]" : t + "[]";
                }
            default:
                return csharpType;
        }
    }

    /// <summary>
    /// 引用类型取最后一个值
    /// </summary>
    /// <param name="refString"></param>
    /// <returns></returns>
    public static string? ParseRefType(string? refString)
    {
        if (string.IsNullOrEmpty(refString))
        {
            return null;
        }
        //取ref string的最后一个
        string[] refs = refString.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return refs[refs.Length - 1];
    }

    /// <summary>
    /// 值类型变成引用类型后的类型错误转换
    /// </summary>
    /// <param name="refString"></param>
    /// <returns></returns>
    public static (string content, bool isValue) ParseValueTypeFromRef(string refString)
    {
        if (refString == "FormData")
        {
            return ("FormData", true);
        }
        if (refString == "DataTable")
        {
            return ("any[][]", true);
        }
        if (refString == "array")
        {
            return ("any[]", true);
        }
        if (refString.EndsWith("_String") || refString.EndsWith("_Byte[]") || refString.Equals("string"))
        {
            return ("string", true);
        }
        if (refString.EndsWith("_Boolean") || refString.Equals("boolean"))
        {
            return ("boolean", true);
        }
        if (refString.EndsWith("_Int32") || refString.EndsWith("_Double") || refString.EndsWith("_Long") || refString.EndsWith("_Float") || refString.EndsWith("_Decimal"))
        {
            return ("number", true);
        }
        if (refString.EndsWith("_Int32[]") || refString.EndsWith("_Double[]") || refString.EndsWith("_Long[]") || refString.EndsWith("_Float[]") || refString.EndsWith("_Decimal[]"))
        {
            return ("number[]", true);
        }
        return (refString, false);
    }
    #endregion

    #region Extract Parameter Name
    /// <summary>
    /// 将字符串化的参数字符串,提取参数
    /// <code>loading: boolean = true, callbackFn: (param: any) => any</code>
    /// <code>loading,callbackFn</code>
    /// </summary>
    /// <param name="parameterString"></param>
    /// <returns></returns>
    public static List<string> ExtractParameterName(string parameterString)
    {
        if (string.IsNullOrWhiteSpace(parameterString))
        {
            return new List<string>();
        }
        var ps = parameterString.Split(",");
        List<string> paramNameList = new List<string>(ps.Length);
        foreach (var item in ps)
        {
            var sb = new StringBuilder();
            foreach (var c in item)
            {
                // 跳过前面的空格
                if (sb.Length == 0)
                {
                    if (Char.IsWhiteSpace(c)) continue;
                    sb.Append(c);
                }
                else
                {
                    if (Char.IsLetterOrDigit(c)) sb.Append(c);
                    else break;
                }
            }
            paramNameList.Add(sb.ToString());
        }
        return paramNameList;
    }

    /// <summary>
    /// 将import xx as xxx from xx 字符串,提取参数
    /// </summary>
    /// <param name="importString"></param>
    /// <returns></returns>
    public static string ExtractImportName(string importString, string defaultName = "http")
    {
        var importIndex = importString.IndexOf("import");
        var fromIndex = importString.IndexOf("from");
        if (importIndex + 6 >= fromIndex) return defaultName;

        var paramList = importString[(importIndex + 6)..fromIndex].Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (paramList.Length == 0) return defaultName;
        return paramList[paramList.Length - 1];
    }
    #endregion

}
