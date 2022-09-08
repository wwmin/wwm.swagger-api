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

    /// <summary>
    /// 去除特殊字符
    /// </summary>
    /// <param name="specialSymbols"></param>
    /// <param name="words"></param>
    /// <returns></returns>
    public static string ReplceSpecialStr(List<string> specialSymbols, string words)
    {
        if (specialSymbols == null || specialSymbols.Count == 0) return words;
        var res = words;
        specialSymbols.ForEach(s =>
        {
            res = res.Replace(s, "");
        });
        return res;
    }
}
