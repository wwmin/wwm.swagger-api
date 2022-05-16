using swagger2js_cli.Models;

using System.Text;
using System.Text.Json;

namespace swagger2js_cli.Processes;
/// <summary>
/// 处理json文档
/// </summary>
public static class TypeScriptInterfaceProcess
{
    public static void ParseSwaggerJson(string swaagerJson)
    {
        var swagger = JsonSerializer.Deserialize<SwaggerModel>(swaagerJson);

        {
            //string filePath = "D:/api/interface/index.ts";
            //var allTsModelsString = GenerateTypeScriptTypesFromJsonModel(swagger?.components);
            //SaveToFile(filePath, allTsModelsString);
        }

        TypeScriptApiProcess.GenerateTypeScriptApiFromJsonModel(swagger);
    }


    #region GenerateTypeString
    public static string GenerateTypeScriptTypesFromJsonModel(Components? jsonComponents)
    {
        StringBuilder sb = new StringBuilder();
        if (jsonComponents == null)
        {
            return "";
        }
        //var schemas = GenerateSchemasTypeScriptType(jsonComponents.schemas);
        foreach (var key in jsonComponents.schemas.Keys)
        {
            var value = jsonComponents.schemas[key];
            var typeScriptType = GenerateSchemasTypeScriptInterface(key, value);
            sb.Append(typeScriptType);
            //Console.WriteLine(typeScriptType);
        }

        var securitySchemes = jsonComponents.securitySchemes;
        return sb.ToString();
    }

    /// <summary>
    /// 生成实体类
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private static string GenerateSchemasTypeScriptInterface(string tsTypeName, SchemasModel model)
    {
        if (model == null) return "";
        string prefix_space_num = "  ";//默认两个空格
        var typeScriptInterface = string.IsNullOrEmpty(model.description) ? "" : $"/** {model.description} */\n";
        typeScriptInterface += $"export interface {tsTypeName} {{\n";
        if (model.type == "object")
        {
            var properties = model.properties;
            if (properties != null)
            {
                var allTypes = new HashSet<string>();
                bool isNullable = false;
                foreach (var key in properties.Keys)
                {
                    var value = properties[key];
                    //Description
                    if (!string.IsNullOrEmpty(value.description))
                    {
                        //typeScriptInterface += string.IsNullOrEmpty(value.description) ? value.description : $"/** {value.description} */\n";
                        if (value.@default == null)
                        {
                            typeScriptInterface += $"{prefix_space_num}/** {value.description} */\n";
                        }
                        else
                        {
                            typeScriptInterface += $"{prefix_space_num}/** {value.description}\n" +
                                $"{prefix_space_num}* {value.@default }\n" +
                                $"{prefix_space_num}*/\n";
                        }
                    }
                    // key : type
                    if (value.type != null)
                    {
                        var t = CSharpTypeToTypeScriptType.Convert(value.items?._ref, value.type);
                        allTypes.Add(t);
                        if (value.nullable)
                        {
                            isNullable = value.nullable;
                            typeScriptInterface += $"{prefix_space_num}{key}: {t} | null,\n";
                        }
                        else
                        {
                            typeScriptInterface += $"{prefix_space_num}{key}: {t},\n";
                        }
                    }
                    //                    else if (value.@enum != null)
                    //                    {
                    //                        typeScriptInterface += $@"{key}: {Convert(value.@enum)},
                    //";
                    //                    }
                    else if (value._ref != null)
                    {
                        var t = CSharpTypeToTypeScriptType.ParseRefType(value._ref);
                        if (t != null)
                        {
                            allTypes.Add(t);
                            typeScriptInterface += $"{prefix_space_num}{key}: {t},\n";
                        }
                    }
                }
                //增加索引签名 例如:  [index:string]:string|number
                var allTypeString = allTypes.Aggregate((x, y) => x + " | " + y);
                if (isNullable)
                {
                    allTypeString += " | null";
                }
                typeScriptInterface += $"{prefix_space_num}/** 索引 */\n";
                typeScriptInterface += $"{prefix_space_num}[index: string]: {allTypeString}\n";
            }
        }
        typeScriptInterface += $"}}\n\n";
        return typeScriptInterface;
    }
    #endregion

    private static void SaveToFile(string filePath, string str)
    {
        var dirPath = Path.GetDirectoryName(filePath)!;
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllText(filePath, str);
    }
}
