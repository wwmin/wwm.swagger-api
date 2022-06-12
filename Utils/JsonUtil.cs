using System.Dynamic;
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
    public static T? GetJsonObjectProperty<T>(string jsonText, string propertyName) where T : struct
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
    public static List<T>? GetJsonArrayProperty<T>(string jsonText, string propertyName)
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

    private static T GetData<T>(JsonElement json) where T : struct
    {

        var t = typeof(T);
        if (!t.IsValueType) throw new Exception("仅支持获取值类型");
        return t.Name switch
        {
            "Int32" => (T)(object)json.GetInt32(),
            "Double" => (T)(object)json.GetDouble(),
            "String" => (T)((object?)json.GetString() ?? ""),
            "Decimal" => (T)(object)json.GetDecimal(),
            "DateTime" => (T)(object)json.GetDateTime(),
            "Boolean" => (T)(object)json.GetBoolean(),
            "Char" => (T)(object)json.GetByte(),
            "Guid" => (T)(object)json.GetGuid(),
            _ => default,
        };
    }

    /// <summary>
    /// [扩展] json字符串反序列化为 dynamic 对象,
    /// ---- 
    /// string data = "....我是json字符串"
    /// dynamic 直接根据 key 取值即可
    /// dynamic jsonObj = data.DeserializeDynamicJsonObject();
    /// 直接用 jsonObj.myList[0].name 取值
    /// </summary>
    public static dynamic DeserializeDynamicJsonObject(this string data)
    {
        return new JsonTextAccessor(JsonSerializer.Deserialize<JsonElement>(data));
    }
    public class JsonTextAccessor : DynamicObject
    {
        private readonly JsonElement _content;
        public JsonTextAccessor(JsonElement content)
        {
            _content = content;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = null;

            if (_content.TryGetProperty(binder.Name, out JsonElement value))
            {
                result = Obtain(value);
            }
            else
            {
                return false;
            }

            return true;
        }
        private object? Obtain(in JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String: return element.GetString() ?? "";
                case JsonValueKind.Null: return null;
                case JsonValueKind.False: return false;
                case JsonValueKind.True: return true;
                case JsonValueKind.Number: return element.GetDouble();
            }
            if (element.ValueKind == JsonValueKind.Array)
            {
                return element.EnumerateArray().Select(item => Obtain(item)).ToList();
            }
            // Undefined / Object 
            return new JsonTextAccessor(element);
        }
    }
}
