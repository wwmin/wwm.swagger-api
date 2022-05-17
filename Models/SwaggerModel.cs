namespace swagger2js_cli.Models;

public class SwaggerModel
{
    public string openapi { get; set; }
    public Info info { get; set; }
    public Server[] servers { get; set; }
    public Dictionary<string, PathModel> paths { get; set; }
    public Components components { get; set; }
    public Security[] security { get; set; }
    public Tag[] tags { get; set; }
}

public class Info
{
    public string title { get; set; }
    public Contact contact { get; set; }
    public string version { get; set; }
    public string description { get; set; }
}

public class Contact
{
    public string name { get; set; }
    public string email { get; set; }
    public string url { get; set; }
}

public class PathModel
{
    public HttpRequestModel post { get; set; }
    public HttpRequestModel get { get; set; }

    public HttpRequestModel put { get; set; }
    public HttpRequestModel delete { get; set; }

    public HttpRequestModel? this[string index]
    {
        get
        {
            return index switch
            {
                "post" => post,
                "get" => get,
                "put" => put,
                "delete" => delete,
                _ => null
            };
        }
    }
}

public class HttpRequestModel
{
    public string[] tags { get; set; }
    public string summary { get; set; }
    public string operationId { get; set; }
    public Requestbody requestBody { get; set; }
    public Parameter[] parameters { get; set; }
    //public Responses responses { get; set; }
    public Dictionary<string, ResponseModel> responses { get; set; }
}

public class Requestbody
{
    public string description { get; set; }
    //public ApplicationJson content { get; set; }

    /// <summary>
    /// 此key包括 application/json, application/xml, text/xml, text/html, text/plain, application/octet-stream,multipart/form-data
    /// TODO: 需要对multipart/form-data 做类型特殊处理
    /// </summary>
    public Dictionary<string, JsonSchema> content { get; set; }
}

public class ResponseBody
{
    public string description { get; set; }
    public Dictionary<string, JsonSchema> content { get; set; }
}

public class ResponseContentModel
{

    //public Dictionary<string, JsonSchema> application_json { get; set; }
    //public Dictionary<string, JsonSchema> text_plain { get; set; }
    //public Dictionary<string, JsonSchema> text_json { get; set; }
}



public class JsonSchema
{
    /// <summary>
    /// key为"$ref", value为"/components/schemas/User"
    /// </summary>
    public ReferenceObject schema { get; set; }
}

/// <summary>
/// 引用类型类
/// 形式为:
/// {
///     "$ref":"/components/schemas/User"
/// }
/// </summary>
public class ReferenceObject
{
    public string _ref { get; set; }
}

public class FormDataSchema
{
    public FormDataSchemaModel schema { get; set; }
    public Dictionary<string, FormDataSchemaEncoding> encoding { get; set; }
}

public class FormDataSchemaModel
{
    public string type { get; set; }
    public Dictionary<string, FormDataSchemaPropertyModel> properties { get; set; }
}

public class FormDataSchemaPropertyModel
{
    public string type { get; set; }
    public string description { get; set; }
    public string format { get; set; }
    public bool nullable { get; set; }
}

public class FormDataSchemaEncoding
{
    public string style { get; set; }
}

//public class Responses
//{
//    public _200 _200 { get; set; }
//}

public class ResponseModel
{
    public string description { get; set; }
    public Dictionary<string, JsonSchema> content { get; set; }
}


public class Parameter
{
    public string name { get; set; }
    public string @in { get; set; }
    public bool required { get; set; }
    public PropertyModel schema { get; set; }
    public string description { get; set; }
}



public class Components
{
    public Dictionary<string, SchemasModel> schemas { get; set; }
    public Securityschemes securitySchemes { get; set; }
}

public class SchemasModel
{
    public string[] required { get; set; }
    public string type { get; set; }
    public Dictionary<string, PropertyModel> properties { get; set; }
    public bool additionalProperties { get; set; }
    public string description { get; set; }
    public object[] @enum { get; set; }
}

public class PropertyModel
{
    /// <summary>
    /// 引用类型, 原key为 "$ref" 因无法使用$开头的定义字段故在此初转义到_ref
    /// 如果存在此值,说明只有该引用类型字段
    /// </summary>
    public string _ref { get; set; }
    public int minLength { get; set; }
    public int maxLength { get; set; }
    public string type { get; set; }
    /// <summary>
    /// 如果 type 为 array, 则此处为其元素类型
    /// 形式为: 
    /// {
    ///     "$ref":"#/components/schemas/Pet"
    /// }
    /// </summary>
    public ReferenceObject items { get; set; }
    public string description { get; set; }
    public string format { get; set; }
    public bool nullable { get; set; }

    public Array @enum { get; set; }

    public object @default { get; set; }
}

public class Securityschemes
{
    public Bearer Bearer { get; set; }
}

public class Bearer
{
    public string type { get; set; }
    public string description { get; set; }
    public string scheme { get; set; }
    public string bearerFormat { get; set; }
    public string name { get; set; }
    public string @in { get; set; }
}

public class Server
{
    public string url { get; set; }
    public string description { get; set; }
}

public class Security
{
    public object[] Bearer { get; set; }
}

public class Tag
{
    public string name { get; set; }
    public string description { get; set; }
}
