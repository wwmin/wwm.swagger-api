using System.Diagnostics;
using System.Text;

using wwm.swagger_api.Models;

namespace wwm.swagger_api.Processes;
/// <summary>
/// 处理json文档
/// </summary>
public static class TypeScriptInterfaceProcess
{

    #region GenerateTypeString
    /// <summary>
    /// 从Json文档中生成类型
    /// </summary>
    /// <param name="jsonComponents"></param>
    /// <param name="filePath"></param>
    /// <param name="_config"></param>
    public static void GenerateTypeScriptTypesFromJsonModel(Components? jsonComponents, string filePath, Config _config)
    {
        StringBuilder sb = new StringBuilder();
        if (jsonComponents == null)
        {
            return;
        }
        string prefix_space_num = _config.IndentSpaceNum > 0 ? Enumerable.Range(0, _config.IndentSpaceNum).Select(a => " ").Aggregate((x, y) => x + y) : "";//默认两个空格
        sb.AppendLine(_config.FileHeadText);
        //var schemas = GenerateSchemasTypeScriptType(jsonComponents.schemas);
        foreach (var key in jsonComponents.schemas.Keys)
        {
            var value = jsonComponents.schemas[key];
            // 将key处理掉特殊字符
            string keyDeal = StringUtil.ReplceSpecialStr(CONST.SpecialSymbols, key);
            if (value.@enum != null)
            {
                var typeScriptEnum = GenerateScheasTypeScriptEnum(keyDeal, value, prefix_space_num);
                sb.Append(typeScriptEnum);
            }
            else
            {
                var typeScriptType = GenerateSchemasTypeScriptInterface(keyDeal, value, prefix_space_num, jsonComponents);
                sb.Append(typeScriptType);
            }
            ConsoleUtil.WriteLine("生成接口: " + keyDeal, ConsoleColor.DarkCyan);
        }
        //TODO: 
        //var securitySchemes = jsonComponents.securitySchemes;
        //return sb.ToString();
        SaveToFile(filePath, sb.ToString());
    }

    /// <summary>
    /// 生成Interface
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private static StringBuilder? GenerateSchemasTypeScriptInterface(string tsTypeName, SchemasModel model, string prefix_space_num, Components jsonComponents)
    {
        if (model == null) return null;
        //后缀是 _String _Int32 _Byte[] 等等的对象实体,内部是值类型,不需要定义接口 
        var refValue = ProcessUtil.ParseValueTypeFromRef(tsTypeName);
        if (refValue.isValue)
        {
            return null;
        }
        // string prefix_space_num = "  ";//默认两个空格
        var typeScriptInterface = new StringBuilder(string.IsNullOrEmpty(model.description) ? "" : $"/** {model.description} */\n");
        typeScriptInterface.AppendLine($"export interface {tsTypeName} {{");
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
                            typeScriptInterface.AppendLine($"{prefix_space_num}/** {value.description} */");
                        }
                        else
                        {
                            typeScriptInterface.AppendLine($"{prefix_space_num}/** {value.description}");
                            typeScriptInterface.AppendLine($"{prefix_space_num}* {value.@default}");
                            typeScriptInterface.AppendLine($"{prefix_space_num}*/");
                        }
                    }
                    else if (!string.IsNullOrEmpty(value._ref))
                    {
                        // 存在引用,使用引用注释
                        // 类似于
                        // // 用户信息
                        // public UserInfo User{get;set;}
                        // 属性是引用类型时,swagger.json 只保留了$ref值, 将description忽略了(swagger标准bug?)
                        var refName = ProcessUtil.ParseRefType(value._ref);
                        if (refName != null)
                        {
                            var schema = jsonComponents.schemas[refName];
                            if (schema != null && string.IsNullOrWhiteSpace(schema.description) == false)
                            {
                                if (schema.description.Contains("<br />"))
                                {
                                    var schemaDescriptionList = schema.description.Split(new string[] { "<br />&nbsp;", "<br />" }, StringSplitOptions.RemoveEmptyEntries);
                                    var schemaDescription = string.Join("\r\n" + prefix_space_num + prefix_space_num, schemaDescriptionList);
                                    typeScriptInterface.AppendLine($"{prefix_space_num}/** {schemaDescription} */");
                                }
                                else
                                {
                                    typeScriptInterface.AppendLine($"{prefix_space_num}/** {schema.description} */");
                                }
                            }
                        }
                    }
                    // key : type
                    if (value.type != null)
                    {
                        var t = ProcessUtil.Convert(value.items?._ref, value.type);
                        allTypes.Add(t);
                        if (value.nullable)
                        {
                            isNullable = value.nullable;
                            typeScriptInterface.AppendLine($"{prefix_space_num}{key}: {t} | null,");
                        }
                        else
                        {
                            typeScriptInterface.AppendLine($"{prefix_space_num}{key}: {t},");
                        }
                    }
                    else if (value._ref != null)
                    {
                        var t = ProcessUtil.ParseRefType(value._ref);
                        if (t != null)
                        {
                            allTypes.Add(t);
                            typeScriptInterface.AppendLine($"{prefix_space_num}{key}: {t},");
                        }
                    }
                    else
                    {
                        allTypes.Add("any");
                        typeScriptInterface.AppendLine($"{prefix_space_num}{key}: any,");
                    }
                }
                if (allTypes.Count == 0) return typeScriptInterface.Clear();
                //增加索引签名 例如:  [index:string]:string|number
                var allTypeString = allTypes.Aggregate((x, y) => x + " | " + y);
                if (isNullable)
                {
                    allTypeString += " | null";
                }
                typeScriptInterface.AppendLine($"{prefix_space_num}/** 索引 */");
                typeScriptInterface.AppendLine($"{prefix_space_num}[index: string]: {allTypeString}");
            }
        }
        typeScriptInterface.AppendLine($"}}\n");
        return typeScriptInterface;
    }

    /// <summary>
    /// 生成枚举  enum类型及注释依靠description生成
    /// </summary>
    /// <param name="tsTypeName"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    private static string GenerateScheasTypeScriptEnum(string tsTypeName, SchemasModel model, string prefix_space_num)
    {
        if (model == null || string.IsNullOrEmpty(model.description)) return "";
        // string prefix_space_num = "  ";//默认两个空格
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
            typeScriptEnum += $"{prefix_space_num}{key} = {value}{(i == enumDescriptionList.Length - 1 ? "" : ",")}\n";
        }


        typeScriptEnum += $"}}\n\n";
        return typeScriptEnum;
    }

    #endregion
    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="str"></param>
    public static void SaveToFile(string filePath, string str)
    {
        var dirPath = Path.GetDirectoryName(filePath)!;
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllText(filePath, str);
    }
}
