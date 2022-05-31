using swagger2js_cli.Models;

using System.Text;

namespace swagger2js_cli.Processes;
/// <summary>
/// Api 处理
/// </summary>
public static class TypeScriptApiProcess
{
    public static void GenerateTypeScriptApiFromJsonModel(SwaggerModel? swaggerModel, string basePath, string filePreText, string interfacePre, Config _config)
    {
        if (swaggerModel == null) return;
        Dictionary<string, PathModel>? PathDic = swaggerModel.paths;
        if (PathDic == null) return;
        string prefix_space_num = "  ";//默认两个空格
        //string basePath = "D:/api/api/";
        RemoveFileIfExist(basePath);
        string filePrefix = "api.";
        string filePost = ".ts";
        //string interfacePre = "IApi";
        //string filePreText = $"import * as {interfacePre} from \"../interface\";\n" +
        //    "import http from \"../index\"\n\n";
        var keys = PathDic.Keys;
        Dictionary<string, bool> pathStatistic = new Dictionary<string, bool>();
        foreach (var key in keys)
        {

            var value = PathDic[key];

            var tag = (value.get ?? value.post ?? value.put ?? value.delete).tags?.FirstOrDefault() ?? "common";
            var fileName = basePath + filePrefix + tag + filePost;
            bool hasTagFile = pathStatistic.ContainsKey(tag);
            if (hasTagFile == false)
            {
                pathStatistic.Add(tag, true);
            }
            var parseeKey = ParseRoutePathToString(key);
            var requestUrlPathName = ParsePathToCamelName(parseeKey, "api", tag);
            //Console.WriteLine(tag + " : " + requestUrlPathName);
            //一个路径有多个请求, 则使用对象形式 account.code:{ get: ()=>{},post:()=>{} }
            HttpRequestModel reqModel = null;
            List<string> methods = new List<string>();

            if (value.get != null)
            {
                reqModel = value.get;
                methods.Add(nameof(value.get));
            }
            if (value.post != null)
            {
                reqModel = value.post;
                methods.Add(nameof(value.post));
            }
            if (value.put != null)
            {
                reqModel = value.put;
                methods.Add(nameof(value.put));
            }
            if (value.delete != null)
            {
                reqModel = value.delete;
                methods.Add(nameof(value.delete));
            }
            if (methods.Count == 0)
            {
                continue;
            }
            if (!hasTagFile)
            {

                //将tag注释添加到api的文件头上面
                var tagger = swaggerModel.tags.FirstOrDefault(p => p.name == tag);
                var fileTagDesc = "";
                if (!string.IsNullOrEmpty(_config.FileHeadText)) fileTagDesc += _config.FileHeadText + "\n";

                if (tagger != null && !string.IsNullOrEmpty(tagger.description))
                {
                    fileTagDesc += $"/**\n";
                    fileTagDesc += $" * {tagger.description}\n";
                    fileTagDesc += $" */\n\n";
                }
                SaveFile(fileName, fileTagDesc + filePreText, false);
            }
            if (methods.Count == 1 && reqModel != null)
            {
                var apiValue = ConvertReqModelToApi(swaggerModel, reqModel, key, requestUrlPathName, methods.FirstOrDefault()!, interfacePre, prefix_space_num);
                if (!string.IsNullOrEmpty(apiValue.interfaceText)) SaveFile(fileName, apiValue.interfaceText, true);
                SaveFile(fileName, apiValue.content, true);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                //使用单个对象形式输出
                if (!string.IsNullOrEmpty(reqModel.summary)) sb.AppendLine($"/** {reqModel.summary} */");
                //有多个请求类型
                foreach (var method in methods)
                {
                    reqModel = value[method]!;
                    if (reqModel == null) continue;
                    var apiValue = ConvertReqModelToApi(swaggerModel, reqModel, key, requestUrlPathName, method, interfacePre, prefix_space_num, "_" + method.ToUpperFirst());
                    if (!string.IsNullOrEmpty(apiValue.interfaceText)) SaveFile(fileName, apiValue.interfaceText, true);
                    SaveFile(fileName, apiValue.content, true);
                }
            }
            ConsoleUtil.WriteLine("生成Api: " + key, ConsoleColor.DarkGreen);
        }
    }

    private static string ParsePathToCamelName(string path, string removePreApi, string removeTag)
    {
        var pathList = path.Split("/", StringSplitOptions.RemoveEmptyEntries).ToList();
        pathList = pathList.Where(p => p.Length > 0 && p != "-").ToList();
        if (pathList.Count < 1) return "";
        if (pathList[0] == removePreApi) pathList.RemoveAt(0);
        if (pathList.Count < 1) return "";
        if (pathList[0] == removeTag) pathList.RemoveAt(0);
        if (pathList.Count < 1) return "_" + ParseStringReplaceStrigulaToUp(removeTag);//ParseStringReplaceStrigulaToUp(removeTag);//如果删除tag后没有值,则使用 _tag值
        return pathList.Count == 1 ? ParseStringReplaceStrigulaToUp(pathList[0]) : pathList.Aggregate((x, y) => ParseStringReplaceStrigulaToUp(x) + ParseStringReplaceStrigulaToUp(y));
    }

    private static string ParseStringReplaceStrigulaToUp(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var sl = s.Split("-", StringSplitOptions.RemoveEmptyEntries).Where(p => p.Length > 0).ToList();

        if (sl.Count == 1)
        {
            return sl[0].ToUpperFirst();
        }
        return sl.Aggregate((x, y) => x.ToUpperFirst() + y.ToUpperFirst());
    }


    #region 入参
    /// <summary>
    /// 请求输入参数 提取
    /// </summary>
    /// <param name="ps"></param>
    /// <returns></returns>
    private static (string content, string paramInterfaceName, List<string>? inPathKeys) ParseParameters(string requestName, Parameter[] ps, string prefix_space_num = "  ", string summary = "")
    {
        if (ps == null) return ("", "", null);
        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(summary)) sb.AppendLine($"/** {summary} - 请求参数 */");
        var name = requestName + "Params";
        sb.AppendLine($"export interface {name} {{");
        List<string>? inPathList = null;
        for (int i = 0; i < ps.Length; i++)
        {
            var p = ps[i];
            if (p.@in == "path")
            {
                if (inPathList == null) inPathList = new List<string>();
                inPathList.Add(p.name);
            }
            if (!string.IsNullOrEmpty(p.description))
            {
                sb.AppendLine($"{prefix_space_num}/** {p.description} */");
            }
            sb.AppendLine($"{prefix_space_num}{p.name}: {CSharpTypeToTypeScriptType.Convert(null, p.schema.type)}{(p.required ? "" : " | null")}{(i == ps.Length - 1 ? "" : ",")}");
        }

        sb.AppendLine($"}}\n");
        return (sb.ToString(), name, inPathList);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="swaggerModel"></param>
    /// <param name="reqModel"></param>
    /// <param name="path"></param>
    /// <param name="requestUrlPathName"></param>
    /// <param name="methodName"></param>
    /// <param name="interfacePre"></param>
    /// <param name="prefix_space_num"></param>
    /// <param name="methodPost">可为空,当不为空时,将在导出函数后缀添加 该变量值</param>
    /// <returns></returns>
    private static (string content, string interfaceText) ConvertReqModelToApi(SwaggerModel swaggerModel, HttpRequestModel reqModel, string path, string requestUrlPathName, string methodName, string interfacePre, string prefix_space_num, string? methodPost = "")
    {
        StringBuilder sb = new StringBuilder();
        //使用单个对象形式输出
        if (!string.IsNullOrEmpty(reqModel.summary)) sb.AppendLine($"/** {reqModel.summary} */");
        // 处理入参, get/delete 默认只有queryParams , post/put默认首先有requestBody和parameters
        // 处理parameters参数
        var parameters = ParseParameters(requestUrlPathName, reqModel.parameters, prefix_space_num, reqModel.summary);

        //if (!string.IsNullOrEmpty(parameters.content))
        //{
        //    SaveFile(fileName, parameters.content, true);
        //}
        string paramType = parameters.paramInterfaceName;
        // 处理requestBody
        var requestBody = CSharpTypeToTypeScriptType.ParseRefType(reqModel.requestBody?.content?["application/json"]?.schema?._ref);
        var hasRequestBody = false;
        var hasParamType = !string.IsNullOrEmpty(paramType);
        if (!string.IsNullOrEmpty(requestBody))
        {
            hasRequestBody = true;
            //有限将bodyParams放到请求参数前面
            var refValue = CSharpTypeToTypeScriptType.ParseValueTypeFromRef(requestBody);
            if (refValue.isValue)
            {
                requestBody = refValue.content;
            }
            else
            {
                requestBody = interfacePre + "." + refValue.content;
            }
            if (hasParamType == false)
            {
                sb.AppendLine($"export const {requestUrlPathName + methodPost} = (params: {requestBody}, loading: boolean = true) => {{");
            }
            else
            {
                sb.AppendLine($"export const {requestUrlPathName + methodPost} = (params: {requestBody} , requestParams: {paramType}, loading: boolean = true) => {{");
            }
        }
        else if (hasParamType)
        {

            sb.AppendLine($"export const {requestUrlPathName + methodPost} = (params: {paramType}, loading: boolean = true) => {{");
        }
        else
        {
            sb.AppendLine($"export const {requestUrlPathName + methodPost} = (loading: boolean = true) => {{");
        }
        if (parameters.inPathKeys != null && parameters.inPathKeys.Count > 0)
        {
            var pathKeys = string.Join(" , ", parameters.inPathKeys);
            if (hasRequestBody)
            {
                sb.AppendLine($"{prefix_space_num}let {{ {pathKeys} }} = requestParams;");
            }
            else
            {
                sb.AppendLine($"{prefix_space_num}let {{ {pathKeys} }} = params;");
            }
        }
        // 处理出参
        var responseType = ParseResponseType(swaggerModel, reqModel.responses, true);
        if (!responseType.content.StartsWith("any"))
        {
            if (!responseType.isValueType)
            {
                responseType.content = interfacePre + "." + responseType.content;
            }
        }
        var realUrlPath = UrlPathToES6ParamsPath(path);
        if (hasRequestBody && hasParamType)
        {
            sb.AppendLine($"{prefix_space_num}return http.{methodName}<{responseType.content}>(`{realUrlPath}`, params, requestParams, loading);");
        }
        else if (hasRequestBody || hasParamType)
        {
            sb.AppendLine($"{prefix_space_num}return http.{methodName}<{responseType.content}>(`{realUrlPath}`, params, {{}}, loading);");
        }
        else
        {
            sb.AppendLine($"{prefix_space_num}return http.{methodName}<{responseType.content}>(`{realUrlPath}`, {{}}, {{}}, loading);");
        }
        //sb.AppendLine($"{prefix_space_num}return http.{methodName}<{responseType.content}>(`{realUrlPath}`{(string.IsNullOrEmpty(paramType) ? "" : ", params")}{(hasRequestBody ? " , requestParams" : "")});");
        sb.AppendLine($"}}\n\n");
        return (sb.ToString(), parameters.content);
    }
    #endregion
    #region 出参
    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestName"></param>
    /// <param name="rs"></param>
    /// <param name="prefix_space_num"></param>
    /// <param name="summary"></param>
    /// <param name="isRemoveWrapType">是否去掉统一包装类型, 默认去掉, 只去 返回类型中的data的类型</param>
    /// <returns></returns>
    private static (string content, bool isValueType) ParseResponseType(SwaggerModel swaggerModel, Dictionary<string, ResponseModel> rs, bool isRemoveWrapType = true)
    {
        if (rs == null) return ("any", false);
        var keys = rs.Keys;

        //StringBuilder sb = new StringBuilder();
        //if (!string.IsNullOrEmpty(summary)) sb.AppendLine($"/** {summary} - 响应类型 */");
        //var name = requestName + "Response";
        // 查看是否是引用类型, 如果不是则需要自己新建一个接口对象
        // 取成功类型, 及key是200 的值, TODO: 其他类型待处理
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys.ElementAt(i);
            var value = rs[key];
            if (key == "200")
            {
                // 此处取json的返回值
                if (value.content != null && value.content["application/json"] != null)
                {
                    var refType = CSharpTypeToTypeScriptType.ParseRefType(value.content["application/json"].schema._ref);
                    if (refType != null)
                    {
                        if (isRemoveWrapType)
                        {
                            // 去components.schemas 引用类型中去查找
                            var schemas = swaggerModel?.components?.schemas;
                            if (schemas == null) return ("any", false);
                            var dataRef = schemas.Keys.Where(p => p == refType).FirstOrDefault();
                            if (dataRef == null) return ("any", false);
                            if (schemas[dataRef].properties.TryGetValue("data", out var data))
                            {
                                if (data._ref == null && data.items == null)
                                {
                                    return (data.type, true);
                                }
                                // 因 值 类型被系统包装成了对象类型, 实际返回值还是值类型, swagger或框架的bug?
                                var subType = CSharpTypeToTypeScriptType.ParseRefType(data._ref);
                                if (subType != null)
                                {
                                    var refValue = CSharpTypeToTypeScriptType.ParseValueTypeFromRef(subType);
                                    if (refValue.isValue == false && refValue.content.StartsWith("ActionResult_"))
                                    {
                                        if (schemas[refValue.content].properties.TryGetValue("value", out var subData))
                                        {
                                            //系统过度包装了ActionResult类型
                                            var subSubType = CSharpTypeToTypeScriptType.ParseRefType(subData._ref);
                                            if (subSubType != null)
                                            {
                                                refValue = CSharpTypeToTypeScriptType.ParseValueTypeFromRef(subSubType);
                                            }
                                        }
                                    }
                                    return (refValue.content, refValue.isValue);
                                }
                                else if (data.@type == "array")
                                {
                                    var arrayType = CSharpTypeToTypeScriptType.ParseRefType(data.items._ref);
                                    if (arrayType == null)
                                    {
                                        if (data.items._ref != null)
                                        {
                                            return (data.items._ref + "[]", false);
                                        }
                                    }
                                    return (arrayType != null ? arrayType + "[]" : "any", false);
                                }

                                return (data.type ?? "any", false);
                            };
                        }
                        else
                        {
                            return (refType, false);
                        }
                    }
                    else
                    {
                        return ("any", false);
                    }
                }
            }

        }
        return ("any", false);
    }
    #endregion
    /// <summary>
    /// 将形如: /api/account/{id}/{name} 路径修改为: /api/account/$id/$name
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static string ParseRoutePathToString(string s)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c == '{')
            {
                sb.Append("$"); //使用$符号作为动态路径的标记
                continue;
            }
            if (c == '}')
            {
                continue;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// 将形如: /api/account/{id}/{name} 路径修改为: /api/account/${id}/${name}
    /// </summary>
    /// <param name="urlPath"></param>
    /// <returns></returns>
    private static string UrlPathToES6ParamsPath(string urlPath)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < urlPath.Length; i++)
        {
            var c = urlPath[i];
            if (c == '{')
            {
                sb.Append("${"); //使用$符号作为动态路径的标记
                continue;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    #region File
    private static void SaveFile(string filePath, string content, bool isAppend = false)
    {
        var dirPath = Path.GetDirectoryName(filePath)!;
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        if (isAppend)
        {
            File.AppendAllText(filePath, content);
        }
        else
        {
            File.WriteAllText(filePath, content);
        }
    }

    private static void RemoveFileIfExist(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
    #endregion

}
