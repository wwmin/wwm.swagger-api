using System.Collections.Generic;

namespace swagger2js_cli.Models
{
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
    }

    public class Contact
    {
        public string name { get; set; }
        public string email { get; set; }
    }

    public class PathModel
    {
        public Post post { get; set; }
        public Get get { get; set; }

        public Post put { get; set; }
        public Post delete { get; set; }
    }

    public class Post
    {
        public string[] tags { get; set; }
        public string summary { get; set; }
        public string operationId { get; set; }
        public Requestbody requestBody { get; set; }
        public Parameter[] parameters { get; set; }
        public Responses responses { get; set; }
    }

    public class Requestbody
    {
        public string description { get; set; }
        public ApplicationJson content { get; set; }
    }

    public class ApplicationJson
    {
        public JsonSchema application_json { get; set; }

        public FormDataSchema multipart_form_data { get; set; }
    }

    public class JsonSchema
    {
        public SchemaModel schema { get; set; }
    }

    public class SchemaModel
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
    }

    public class FormDataSchemaEncoding
    {
        public string style { get; set; }
    }

    public class Responses
    {
        public _200 _200 { get; set; }
    }

    public class _200
    {
        public string description { get; set; }
    }

    public class Get
    {
        public string[] tags { get; set; }
        public string summary { get; set; }
        public string operationId { get; set; }
        public Parameter[] parameters { get; set; }
        public Responses responses { get; set; }
    }

    public class Parameter
    {
        public string name { get; set; }
        public string _in { get; set; }
        public Schema3 schema { get; set; }
        public string description { get; set; }
    }

    public class Schema3
    {
        public string type { get; set; }
        public string format { get; set; }
        public bool nullable { get; set; }
        public string description { get; set; }
        public object _default { get; set; }
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
    }

    public class PropertyModel
    {
        public int minLength { get; set; }
        public string type { get; set; }
        public string description { get; set; }
    }

    public class Securityschemes
    {
        public Bearer Bearer { get; set; }
    }

    public class Bearer
    {
        public string type { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string _in { get; set; }
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
}