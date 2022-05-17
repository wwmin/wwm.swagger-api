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
            string filePath = "D:/api/interface/index.ts";
            var allTsModelsString = GenerateTypeScriptTypesFromJsonModel(swagger?.components);
            SaveToFile(filePath, allTsModelsString);
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
            if (value.@enum != null)
            {
                var typeScriptEnum = GenerateScheasTypeScriptEnum(key, value);
                sb.Append(typeScriptEnum);
            }
            else
            {
                var typeScriptType = GenerateSchemasTypeScriptInterface(key, value);
                sb.Append(typeScriptType);
            }
            //Console.WriteLine(typeScriptType);
        }

        var securitySchemes = jsonComponents.securitySchemes;
        return sb.ToString();
    }

    /// <summary>
    /// 生成接口
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private static string GenerateSchemasTypeScriptInterface(string tsTypeName, SchemasModel model)
    {
        if (model == null) return "";
        //后缀是 _String _Int32 _Byte[] 等等的对象实体,内部是值类型,不需要定义接口
        var refValue = CSharpTypeToTypeScriptType.ParseValueTypeFromRef(tsTypeName);
        if (refValue.isValue)
        {
            return "";
        }
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

    /// <summary>
    /// 生成枚举  enum类型及注释依靠description生成
    /// </summary>
    /// <param name="tsTypeName"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    private static string GenerateScheasTypeScriptEnum(string tsTypeName, SchemasModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.description)) return "";
        string prefix_space_num = "  ";//默认两个空格
        var typeScriptEnum = "";

        var enumDescriptionList = model.description.Split(new string[] { "<br />&nbsp;", "<br />" }, StringSplitOptions.RemoveEmptyEntries);
        int descLength = enumDescriptionList.Length - model.@enum.Length;
        if (descLength > 0)
        {
            //有注释
            var desc = string.Join(" ", enumDescriptionList[..descLength]);
            typeScriptEnum += $"/** {desc} */\n";
        }
        typeScriptEnum += $"export enum {tsTypeName} {{\n";
        for (int i = descLength; i < enumDescriptionList.Length; i++)
        {
            var item = enumDescriptionList[i];
            // item 形如: "用户 = 1"
            // item 形如: "user 普通用户 = 2"
            // 规则: 取最后一个 = 两侧的值, 防止用户注释干扰
            var index = item.LastIndexOf("=");
            if (index == -1 || index == 0) continue;
            var keyIndex = item.Substring(0, index).Trim().LastIndexOf(" ");
            if (keyIndex == -1)
            {
                keyIndex = 0;
            };
            var key = item.Substring(keyIndex, index - keyIndex).Trim();
            var value = item.Substring(index + 1).Trim();
            var description = item.Substring(0, keyIndex).Trim();

            //var enumItem = item.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (!string.IsNullOrEmpty(description))
            {
                typeScriptEnum += $"{prefix_space_num}/** {description} */\n";
            }
            typeScriptEnum += $"{prefix_space_num}{key} = {value},\n";
        }


        typeScriptEnum += $"}}\n\n";
        return typeScriptEnum;
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
