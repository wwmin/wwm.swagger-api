using System.Text.RegularExpressions;

namespace wwm.swagger_api;

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

    /// <summary>
    /// 判断是否是URL连接
    /// </summary>
    /// <param name="urlStr"></param>
    /// <returns></returns>
    public static bool IsUrl(string urlStr) => Regex.IsMatch(urlStr, @"^((https?)(:))?(\/\/)(\w*|\d*)");
}
