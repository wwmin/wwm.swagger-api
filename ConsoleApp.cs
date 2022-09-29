using System.Diagnostics;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;

using wwm.swagger_api.Models;
using wwm.swagger_api.Processes;

namespace wwm.swagger_api;

/// <summary>
/// Console App
/// </summary>
public class ConsoleApp
{
    internal string ArgsOutput { get; private set; } = string.Empty;
    private bool ArgsReadKey { get; }
    /// <summary>
    /// 配置文件
    /// </summary>
    private Config _config { get; }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="args"></param>
    /// <param name="wait"></param>
    /// <exception cref="ArgumentException"></exception>
    public ConsoleApp(string[] args, ManualResetEvent wait)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var assembly = typeof(ConsoleApp).Assembly;
        var version = "v" + string.Join(".", assembly.GetName()?.Version?.ToString()?.Split(".", StringSplitOptions.RemoveEmptyEntries)?.Where((a, b) => b <= 2) ?? Array.Empty<string>());
        var logo = $@"
                                                                                           _ 
                                                                                          (_)
 __      ____      ___ __ ___    _____      ____ _  __ _  __ _  ___ _ __ ______ __ _ _ __  _ 
 \ \ /\ / /\ \ /\ / / '_ ` _ \  / __\ \ /\ / / _` |/ _` |/ _` |/ _ \ '__|______/ _` | '_ \| |
  \ V  V /  \ V  V /| | | | | |_\__ \\ V  V / (_| | (_| | (_| |  __/ |        | (_| | |_) | |
   \_/\_/    \_/\_/ |_| |_| |_(_)___/ \_/\_/ \__,_|\__, |\__, |\___|_|         \__,_| .__/|_|
                                                    __/ | __/ |                     | |      
                                                   |___/ |___/                      |_|      
";
        ConsoleUtil.WriteLine(logo, ConsoleColor.DarkGreen);
        ConsoleUtil.Write(@"# Github # ", ConsoleColor.White);
        ConsoleUtil.Write("https://github.com/wwmin/wwm.swagger-api.git", ConsoleColor.Green);
        ConsoleUtil.WriteLine($" {version}", ConsoleColor.DarkGreen);

        _config = Config.Build();
        ArgsReadKey = true;
        #region 对Text.Json统一配置
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            // 设置Json字符串支持的编码，默认情况下，序列化程序会转义所有非 ASCII 字符。 即，会将它们替换为 \uxxxx，其中 xxxx 为字符的 Unicode
            // 代码。 可以通过设置Encoder来让生成的josn字符串不转义指定的字符集而进行序列化 下面指定了基础拉丁字母和中日韩统一表意文字的基础Unicode 块
            // (U+4E00-U+9FCC)。 基本涵盖了除使用西里尔字母以外所有西方国家的文字和亚洲中日韩越的文字
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs),
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        #endregion
        //开始生成操作
        {
            string jsondata = string.Empty;
            #region 读取json文件内容
            if (StringUtil.IsUrl(_config.JsonUrl))
            {
                var client = new HttpClient();
                try
                {
                    var res = client.GetAsync(_config.JsonUrl).Result;
                    if (!res.IsSuccessStatusCode)
                    {
                        ConsoleUtil.WriteLine("获取json文件出错了,详细信息:" + JsonSerializer.Serialize(res.Content.ReadAsStringAsync().Result, jsonOptions), ConsoleColor.Red);
                        throw new ArgumentException(res.StatusCode.ToString());
                    }
                    jsondata = res.Content.ReadAsStringAsync().Result;
                }
                catch (Exception ex)
                {
                    ConsoleUtil.WriteLine($"获取json文件[ ${_config.JsonUrl} ]出错了,详细信息:" + JsonSerializer.Serialize(ex.Message, jsonOptions), ConsoleColor.Red);
                    wait.Set();
                    return;
                }
            }
            else if (File.Exists(_config.JsonUrl))
            {
                jsondata = File.ReadAllText(_config.JsonUrl);
            }
            else
            {
                throw new ArgumentException($"错误的参数设置：请检查 配置文件中 JsonUrl 参数的正确性");
            }

            ConsoleUtil.WriteLine($"\r\n[{DateTime.Now:MM-dd HH:mm:ss}] 读取swagger文件内容完毕\r\n", ConsoleColor.DarkGreen);
            #endregion
            #region 处理json

            //$ref=>@ref
            //jsondata = jsondata.Replace("$ref", "_ref");
            var swagger = JsonSerializer.Deserialize<SwaggerModel>(jsondata, jsonOptions);
            Task generateInterfaceTask = Task.FromResult(() => 0);
            if (_config.ScriptType == CONST.ScriptType.TypeScript)
            {
                // 生成Interface文件
                string filePath = _config.OutPath + $"{_config.ApiInterfaceFolderName}/index.ts";
                TypeScriptInterfaceProcess.GenerateTypeScriptTypesFromJsonModel(swagger?.components, filePath, _config);
                ConsoleUtil.WriteLine("接口文件路径: " + filePath, ConsoleColor.DarkRed);
                Console.WriteLine();
            }



            //生成api.{name}.ts文件
            {
                string baseFile = _config.OutPath + $"{_config.ApiFolderName}/";
                string interfacePre = "";
                string filePreText = "";
                if (_config.ScriptType == CONST.ScriptType.TypeScript)
                {
                    interfacePre = "IApi";
                    filePreText = $"import * as {interfacePre} from \"../{_config.ApiInterfaceFolderName}\";\n";
                }

                filePreText += $"{_config.ImportHttp}\n\n";
                TypeScriptApiProcess.GenerateTypeScriptApiFromJsonModel(swagger, baseFile, filePreText, interfacePre, _config);
                ConsoleUtil.WriteLine("接口Api文件夹: " + baseFile, ConsoleColor.DarkRed);
            }
            #endregion
            if (ArgsReadKey)
            {
                sw.Stop();
                Console.WriteLine("\r\n按任意键退出,共用时:" + sw.ElapsedMilliseconds / 1000.0 + "秒");
            }
            wait.Set();
        }
    }

    private static string GetDefaultRazorContent(Assembly assembly)
    {
        //此方式在发布之后工具位置会找不到资源
        //ArgsRazor = File.ReadAllText("Templates/SwaggerJsonRazor.cshtml");
        var res = "";
        // 读取文件流
        var names = assembly.GetManifestResourceNames();
        for (int i = 0; i < names.Length; i++)
        {
            var name = assembly.GetName().Name + ".Templates.SwaggerJsonRazor.cshtml";
            if (names[i] != name)
            {
                continue;
            }
            using var stream = assembly.GetManifestResourceStream(name);
            using var streamReader = new StreamReader(stream);
            var content = streamReader.ReadToEnd();
            res = content;
            break;
        }
        return res;
    }
}
