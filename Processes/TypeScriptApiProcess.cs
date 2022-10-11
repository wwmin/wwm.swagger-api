using System.Diagnostics;
using System.Text;

using wwm.swagger_api.Models;

namespace wwm.swagger_api.Processes;
/// <summary>
/// Api 处理
/// </summary>
public static class TypeScriptApiProcess
{
    /// <summary>
    /// 生成Api文件
    /// </summary>
    /// <param name="swaggerModel"></param>
    /// <param name="basePath"></param>
    /// <param name="filePreText"></param>
    /// <param name="interfacePre"></param>
    /// <param name="_config"></param>
    public static void GenerateTypeScriptApiFromJsonModel(SwaggerModel? swaggerModel, string basePath, string filePreText, string interfacePre, Config _config)
    {
        if (swaggerModel == null) return;
        bool isTs = _config.ScriptType == CONST.ScriptType.TypeScript;
        Dictionary<string, PathModel>? PathDic = swaggerModel.paths;
        if (PathDic == null) return;
        string prefix_space_num = _config.IndentSpaceNum > 0 ? Enumerable.Range(0, _config.IndentSpaceNum).Select(a => " ").Aggregate((x, y) => x + y) : "";//默认两个空格
        //RemoveFileIfExist(basePath);
        string filePrefix = "api.";
        string filePost = isTs ? ".ts" : ".js";
        var keys = PathDic.Keys;
        Dictionary<string, bool> pathStatistic = new Dictionary<string, bool>();
        foreach (var key in keys)
        {
            Dictionary<string, bool> existParamDic = new Dictionary<string, bool>();
            var value = PathDic[key];
            var tag = (value.get ?? value.post ?? value.put ?? value.delete ?? value.head).tags?.FirstOrDefault() ?? "common";
            var fileName = basePath + filePrefix + tag + filePost;
            bool hasTagFile = pathStatistic.ContainsKey(tag);
            if (hasTagFile == false)
            {
                pathStatistic.Add(tag, true);
            }
            var parseeKey = ParseRoutePathToString(key);
            var requestUrlPathName = ParsePathToCamelName(parseeKey, "api", tag);
            //一个路径有多个请求, 则使用对象形式 account.code:{ get: ()=>{},post:()=>{} }
            HttpRequestModel? reqModel = null;
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
            if (value.head != null)
            {
                reqModel = value.head;
                methods.Add(nameof(value.head));
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
                var apiValue = ConvertReqModelToApi(_config, swaggerModel, reqModel, key, requestUrlPathName, methods.FirstOrDefault()!, interfacePre, prefix_space_num);
                if (isTs)
                {
                    var notExistParam = existParamDic.TryAdd(tag + ":" + apiValue.interfaceName, true);
                    if (notExistParam)
                    {
                        if (!string.IsNullOrEmpty(apiValue.interfaceText)) SaveFile(fileName, apiValue.interfaceText, true);
                    }
                }
                SaveFile(fileName, apiValue.content, true);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                //使用单个对象形式输出
                if (!string.IsNullOrEmpty(reqModel?.summary)) sb.AppendLine($"/** {reqModel.summary} */");
                //有多个请求类型
                foreach (var method in methods)
                {
                    reqModel = value[method]!;
                    if (reqModel == null) continue;
                    var apiValue = ConvertReqModelToApi(_config, swaggerModel, reqModel, key, requestUrlPathName, method, interfacePre, prefix_space_num, "_" + method.ToUpperFirst());
                    if (isTs)
                    {
                        var notExistParam = existParamDic.TryAdd(tag + ":" + apiValue.interfaceName, true);
                        if (notExistParam)
                        {
                            if (!string.IsNullOrEmpty(apiValue.interfaceText)) SaveFile(fileName, apiValue.interfaceText, true);
                        }
                    }
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
    private static (string content, string paramInterfaceName, List<string>? inPathKeys) ParseParameters(string requestName, Parameter[] ps, string prefix_space_num = "  ", string summary = "", string interfacePre = "")
    {
        if (ps == null) return ("", "", null);
        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(summary)) sb.AppendLine($"/** {summary} - 请求参数 */");
        var name = requestName + "Params";
        sb.AppendLine($"export interface {name} {{");
        List<string>? inPathList = null;
        var allTypes = new HashSet<string>();
        bool isNullable = false;
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
            string interfaceNameType = "";
            if (p.schema._ref != null)
            {
                interfaceNameType = interfacePre + "." + ProcessUtil.Convert(p.schema._ref, p.schema.type);
            }
            else
            {
                interfaceNameType = ProcessUtil.Convert(p.schema.items?.type, p.schema.type);
            }
            allTypes.Add(interfaceNameType);
            if (isNullable == false && p.required == false) isNullable = true;
            sb.AppendLine($"{prefix_space_num}{p.name}: {interfaceNameType}{(p.required ? "" : " | null")},");
        }

        // 添加索引
        var allTypeString = allTypes.Count <= 1 ? allTypes.FirstOrDefault() : allTypes.Aggregate((x, y) => x + " | " + y);
        if (isNullable)
        {
            allTypeString += " | null";
        }
        sb.AppendLine($"{prefix_space_num}[index: string]: {allTypeString}");
        sb.AppendLine($"}}\n");
        return (sb.ToString(), name, inPathList);
    }

    /// <summary>
    /// 将请求参数转换成Api
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
    private static (string content, string interfaceText, string interfaceName) ConvertReqModelToApi(Config _config, SwaggerModel swaggerModel, HttpRequestModel reqModel, string path, string requestUrlPathName, string methodName, string interfacePre, string prefix_space_num, string? methodPost = "")
    {
        StringBuilder sb = new StringBuilder();
        //使用单个对象形式输出
        if (!string.IsNullOrEmpty(reqModel.summary)) sb.AppendLine($"/** {reqModel.summary} */");
        // 处理入参, get/delete 默认只有queryParams , post/put默认首先有requestBody和parameters
        // 处理parameters参数
        var parameters = ParseParameters(requestUrlPathName, reqModel.parameters, prefix_space_num, reqModel.summary, interfacePre);
        //if (!string.IsNullOrEmpty(parameters.content))
        //{
        //    SaveFile(fileName, parameters.content, true);
        //}
        string paramType = parameters.paramInterfaceName;
        // 处理requestBody
        var reqBodyContent = reqModel.requestBody?.content;

        var requestBody = "";
        // TODO: 全类别请求类型
        if (reqBodyContent != null && reqBodyContent.Count > 0)
        {
            var schema = reqBodyContent.Where(p => p.Key == "application/json").FirstOrDefault().Value?.schema;
            //Debug.Assert(schema.type != "array");
            if (schema == null)
            {
                // 尝试读取form-data
                schema = reqBodyContent.Where(p => p.Key == "multipart/form-data").FirstOrDefault().Value.schema;
                if (schema != null)
                {
                    //是form-data类型
                    requestBody = "FormData";
                }
            }
            else
            {
                requestBody = ProcessUtil.Convert(schema._ref ?? schema.items._ref, schema.type);
            }

        }
        // requestBody = CSharpTypeToTypeScriptType.ParseRefType(reqModel.requestBody?.content?["application/json"]?.schema?._ref);
        requestBody = ProcessUtil.ParseRefType(requestBody);
        var hasRequestBody = !string.IsNullOrEmpty(requestBody);
        var hasParamType = !string.IsNullOrEmpty(paramType);
        var funcTailParameter = string.IsNullOrWhiteSpace(_config.FuncTailParameter) ? "" : _config.FuncTailParameter;
        var funcTailParameterNameList = ProcessUtil.ExtractParameterName(funcTailParameter);
        var funcTailParameterNameListString = funcTailParameterNameList.Count > 0 ? $", {string.Join(", ", funcTailParameterNameList)}" : "";
        bool isTs = _config.ScriptType == CONST.ScriptType.TypeScript;
        string? funcTailString = null;
        Func<bool, string> hasParameterString = (bool hasPreString) =>
        {
            if (funcTailString != null) return funcTailString;
            funcTailString = hasPreString ? (string.IsNullOrEmpty(funcTailParameter) ? "" : ", " + funcTailParameter) : funcTailParameter;
            return funcTailString;
        };
        if (hasRequestBody)
        {

            var refValue = ProcessUtil.ParseValueTypeFromRef(requestBody!);
            if (refValue.isValue)
            {
                requestBody = refValue.content;
            }
            else
            {
                requestBody = interfacePre + "." + refValue.content;
            }
            if (hasParamType)
            {
                if (isTs)
                {
                    sb.AppendLine($"export const {requestUrlPathName + methodPost} = (params: {paramType} , body: {requestBody}{hasParameterString(true)}) => {{");
                }
                else
                {
                    sb.AppendLine($"export const {requestUrlPathName + methodPost} = (params, body{hasParameterString(true)}) => {{");
                }
            }
            else
            {
                if (isTs)
                {
                    sb.AppendLine($"export const {requestUrlPathName + methodPost} = (body: {requestBody}{hasParameterString(true)}) => {{");
                }
                else
                {
                    sb.AppendLine($"export const {requestUrlPathName + methodPost} = (body{hasParameterString(true)}) => {{");

                }
            }
        }
        else if (hasParamType)
        {
            if (isTs)
            {
                sb.AppendLine($"export const {requestUrlPathName + methodPost} = (params: {paramType}{hasParameterString(true)}) => {{");
            }
            else
            {
                sb.AppendLine($"export const {requestUrlPathName + methodPost} = (params{hasParameterString(true)}) => {{");
            }
        }
        else
        {
            sb.AppendLine($"export const {requestUrlPathName + methodPost} = ({hasParameterString(false)}) => {{");
        }
        if (parameters.inPathKeys != null && parameters.inPathKeys.Count > 0)
        {
            var pathKeys = string.Join(" , ", parameters.inPathKeys);
            sb.AppendLine($"{prefix_space_num}let {{ {pathKeys} }} = params;");
        }

        var realUrlPath = UrlPathToES6ParamsPath(path);
        var httpFuncName = ProcessUtil.ExtractImportName(_config.ImportHttp, "http");
        if (isTs)
        {
            // 处理出参
            var responseType = ParseResponseType(swaggerModel, reqModel.responses, _config.RemoveUnifyWrapObjectName);

            if (responseType.content != null && !responseType.content.StartsWith("any"))
            {
                if (!responseType.isValueType)
                {
                    responseType.content = StringUtil.ReplceSpecialStr(CONST.SpecialSymbols, responseType.content);
                    responseType.content = interfacePre + "." + responseType.content;
                }
            }
            if (hasRequestBody && hasParamType)
            {
                sb.AppendLine($"{prefix_space_num}return {httpFuncName}.{methodName}<{responseType.content}>(`{realUrlPath}`, params, body{funcTailParameterNameListString});");
            }
            else if (hasParamType)
            {
                sb.AppendLine($"{prefix_space_num}return {httpFuncName}.{methodName}<{responseType.content}>(`{realUrlPath}`, params, {{}}{funcTailParameterNameListString});");
            }
            else if (hasRequestBody)
            {
                sb.AppendLine($"{prefix_space_num}return {httpFuncName}.{methodName}<{responseType.content}>(`{realUrlPath}`, {{}}, body{funcTailParameterNameListString});");
            }
            else
            {
                sb.AppendLine($"{prefix_space_num}return {httpFuncName}.{methodName}<{responseType.content}>(`{realUrlPath}`, {{}}, {{}}{funcTailParameterNameListString});");
            }
        }
        else
        {
            if (hasRequestBody && hasParamType)
            {
                sb.AppendLine($"{prefix_space_num}return {httpFuncName}.{methodName}(`{realUrlPath}`, params, body{funcTailParameterNameListString});");
            }
            else if (hasParamType)
            {
                sb.AppendLine($"{prefix_space_num}return {httpFuncName}.{methodName}(`{realUrlPath}`, params, {{}}{funcTailParameterNameListString});");
            }
            else if (hasRequestBody)
            {
                sb.AppendLine($"{prefix_space_num}return {httpFuncName}.{methodName}(`{realUrlPath}`, {{}}, body{funcTailParameterNameListString});");
            }
            else
            {
                sb.AppendLine($"{prefix_space_num}return {httpFuncName}.{methodName}(`{realUrlPath}`, {{}}, {{}}{funcTailParameterNameListString});");
            }
        }
        sb.AppendLine($"}}\n\n");
        return (sb.ToString(), parameters.content, parameters.paramInterfaceName);
    }
    #endregion
    #region 出参
    /// <summary>
    /// 解析出餐类型
    /// </summary>
    /// <param name="requestName"></param>
    /// <param name="rs"></param>
    /// <param name="prefix_space_num"></param>
    /// <param name="summary"></param>
    /// <param name="removeUnifyWrapObjectName">是否去掉统一包装类型, 默认去掉, 返回类型中的data的类型</param>
    /// <returns></returns>
    private static (string content, bool isValueType) ParseResponseType(SwaggerModel swaggerModel, Dictionary<string, ResponseModel> rs, string removeUnifyWrapObjectName = "")
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
            if (key == "200" && value != null && value.content != null)
            {
                // 此处取json的返回值
                // 如果没有json则尝试使用*/*获取

                JsonSchema jsonSchema;
                var json1 = value.content.TryGetValue("application/json", out jsonSchema!);
                if (json1 == false || jsonSchema == null)
                {
                    json1 = value.content.TryGetValue("*/*", out jsonSchema!);
                }
                if (json1 && jsonSchema != null)
                {
                    var refType = ProcessUtil.Convert(jsonSchema.schema._ref ?? jsonSchema.schema.items?._ref, jsonSchema.schema.type);
                    if (refType != null)
                    {
                        if (string.IsNullOrEmpty(removeUnifyWrapObjectName) == false)
                        {
                            // 去components.schemas 引用类型中去查找
                            var schemas = swaggerModel?.components?.schemas;
                            if (schemas == null) return ("any", false);
                            var dataRef = schemas.Keys.Where(p => p == refType).FirstOrDefault();
                            if (dataRef == null) return ("any", false);
                            if (dataRef.EndsWith("_Object")) return ("object", true);
                            if (schemas[dataRef].properties.TryGetValue(removeUnifyWrapObjectName, out var data))
                            {
                                if (data._ref == null && data.items == null)
                                {
                                    var t = ProcessUtil.Convert(null, data.type);
                                    return (t, true);
                                }
                                // 因 值 类型被系统包装成了对象类型, 实际返回值还是值类型, swagger或框架的bug? , 调用Value或value的值
                                var subType = ProcessUtil.ParseRefType(data._ref);
                                if (subType != null)
                                {
                                    var refValue = ProcessUtil.ParseValueTypeFromRef(subType);
                                    if (refValue.isValue == false && refValue.content.StartsWith("ActionResult_"))
                                    {
                                        PropertyModel? subData = null;
                                        bool isGeted = schemas[refValue.content].properties.TryGetValue("value", out subData);
                                        if (!isGeted) isGeted = schemas[refValue.content].properties.TryGetValue("Value", out subData);

                                        if (isGeted && subData != null)
                                        {
                                            //系统过度包装了ActionResult类型
                                            var subSubType = ProcessUtil.ParseRefType(subData._ref);
                                            if (subSubType != null)
                                            {
                                                refValue = ProcessUtil.ParseValueTypeFromRef(subSubType);
                                            }
                                        }
                                    }
                                    return (refValue.content, refValue.isValue);
                                }
                                else if (data.@type == "array")
                                {
                                    var arrayType = ProcessUtil.ParseRefType(data.items._ref);
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
                            var refValue = ProcessUtil.ParseValueTypeFromRef(refType);
                            return (refValue.content, refValue.isValue);
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
        if (s.Length < 3 || !s.Contains("{")) return s;
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
