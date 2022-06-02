namespace wwm.swaggerApi.Processes;

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
        if (refString.EndsWith("_String") || refString.EndsWith("_Byte[]"))
        {
            return ("string", true);
        }
        if (refString.EndsWith("_Int32") || refString.EndsWith("_Double") || refString.EndsWith("_Long") || refString.EndsWith("_Floa") || refString.EndsWith("_Decimal"))
        {
            return ("number", true);
        }
        if (refString.EndsWith("_Boolean"))
        {
            return ("boolean", true);
        }
        return (refString, false);
    }
    #endregion



}
