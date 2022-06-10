using System.Text.Json;

namespace wwm.swagger_api.Utils;

/// <summary>
/// json帮助类
/// </summary>
public static class JsonUtil
{
    /// <summary>
    /// 获取json对象的属性值
    /// </summary>
    /// <param name="jsonText"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static T GetJsonObjectProperty<T>(string jsonText, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(jsonText)) return default;
        using JsonDocument json = JsonDocument.Parse(jsonText);
        json.RootElement.TryGetProperty(propertyName, out JsonElement value);
        return GetData<T>(value);
    }
    /// <summary>
    /// 获取json数组的属性值
    /// </summary>
    /// <param name="jsonText"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static List<T> GetJsonArrayProperty<T>(string jsonText, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(jsonText)) return default;
        using (JsonDocument json = JsonDocument.Parse(jsonText))
        {
            var array = json.RootElement.EnumerateArray();
            var t = typeof(T);
            var data = array.Select(p => p.GetProperty(propertyName));
            return t.Name switch
            {
                "Int32" => data.Select(a => a.GetInt32()).Cast<T>().ToList(),
                "Double" => data.Select(a => a.GetDouble()).Cast<T>().ToList(),
                "String" => data.Select(a => a.GetString()).Cast<T>().ToList(),
                "Decimal" => data.Select(a => a.GetDecimal()).Cast<T>().ToList(),
                "DateTime" => data.Select(a => a.GetDateTime()).Cast<T>().ToList(),
                "Boolean" => data.Select(a => a.GetBoolean()).Cast<T>().ToList(),
                "Char" => data.Select(a => a.GetByte()).Cast<T>().ToList(),
                "Guid" => data.Select(a => a.GetGuid()).Cast<T>().ToList(),
                _ => null,
            };
        };
    }

    private static T GetData<T>(JsonElement json)
    {
        var t = typeof(T);
        return t.Name switch
        {
            "Int32" => (T)(object)json.GetInt32(),
            "Double" => (T)(object)json.GetDouble(),
            "String" => (T)(object)json.GetString(),
            "Decimal" => (T)(object)json.GetDecimal(),
            "DateTime" => (T)(object)json.GetDateTime(),
            "Boolean" => (T)(object)json.GetBoolean(),
            "Char" => (T)(object)json.GetByte(),
            "Guid" => (T)(object)json.GetGuid(),
            _ => default,
        };
    }
}
