namespace wwm.swaggerApi;

/// <summary>
/// 
/// </summary>
public static class StringUtil
{
    /// <summary>
    /// 将首个字母大写
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string ToUpperFirst(this string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s[..1].ToUpper() + s[1..];
    }
}
