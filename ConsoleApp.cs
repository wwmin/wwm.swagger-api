using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using RazorEngine.Templating;
using swagger2js_cli.Models;

namespace swagger2js_cli
{
    /// <summary>
    ///
    /// </summary>
    public class ConsoleApp
    {
        private string ArgsSwaggerJsonFileUrl { get; }
        private string ArgsRazor { get; }
        internal string ArgsOutput { get; private set; }
        private bool ArgsReadKey { get; }

        public ConsoleApp(string[] args, ManualResetEvent wait)
        {
            var assembly = typeof(ConsoleApp).Assembly;
            var version = "v" + string.Join(".", assembly.GetName().Version.ToString().Split(".").Where((a, b) => b <= 2));
            Colorful.Console.WriteAscii("swagger2js", Color.Violet);
            Colorful.Console.WriteFormatted(@"
# Github # {0} {1}
", Color.SlateGray,
new Colorful.Formatter("https://github.com/wwmin/swagger2js_cli", Color.DeepSkyBlue),
new Colorful.Formatter(version, Color.SlateGray));



            //ArgsSwaggerJsonFileUrl = "swagger.json";
            ArgsReadKey = true;

            #region SetDirection

            Action<string> setArgsOutput = value =>
            {
                ArgsOutput = value;
                ArgsOutput = ArgsOutput.Trim().TrimEnd('/', '\\');
                ArgsOutput += ArgsOutput.Contains("\\") ? "\\" : "/";
                if (!Directory.Exists(ArgsOutput))
                    Directory.CreateDirectory(ArgsOutput);
            };
            setArgsOutput(Directory.GetCurrentDirectory());

            #endregion SetDirection

            #region showInitConsole

            Action showInitConsole = () =>
            {
                Colorful.Console.WriteFormatted(@"
     {0}
     更新工具：dotnet tool update -g swagger2js_cli
     在nuget上查看 https://www.nuget.org/packages/swagger2js_cli/
 # 快速开始 #
 > {1}
    -FileNameUrl   [必填]swagger.json URL(或本地文件路径) 如:-FileNameUrl http://localhost:5000/swagger/v1/swagger.json
    -Razor         自定义模板 如:-Razor ""d:\diy.cshtml""
    -Output        保存路径，默认为当前 shell 所在目录 如:-Output apiFiles
    -DownLoadDefaultRazor   获取默认的razor模板到本地 如: -DownLoadDefaultRazor 后面不跟参数
", Color.SlateGray,
new Colorful.Formatter("swagger2js 将swagger.json文件生成api.{name}.js", Color.SlateGray),
new Colorful.Formatter("swagger2js", Color.White)
);
            };

            #endregion showInitConsole

            #region GetArguments

            string args0 = args[0].Trim().ToLower();
            if (args[0] == "?" || args0 == "--help" || args0 == "-help")
            {
                showInitConsole();
                wait.Set();
                return;
            }
            for (int a = 0; a < args.Length; a++)
            {
                switch (args[a].Trim().ToLower())
                {
                    case "-razor":
                        ArgsRazor = File.ReadAllText(args[a + 1]);
                        a++;
                        break;

                    case "-filenameurl":
                        ArgsSwaggerJsonFileUrl = args[a + 1];
                        a++;
                        break;

                    case "-readkey":
                        ArgsReadKey = args[a + 1].Trim() == "1";
                        a++;
                        break;

                    case "-output":
                        setArgsOutput(args[a + 1]);
                        a++;
                        break;
                    case "-downloaddefaultrazor":
                        ArgsRazor = GetDefaultRazorContent(assembly);
                        //Colorful.Console.WriteFormatted(ArgsRazor, Color.AliceBlue);

                        var swaggerJsonRazorCshtml = "SwaggerJsonRazor.cshtml";
                        if (File.Exists(swaggerJsonRazorCshtml))
                        {
                            File.Delete(swaggerJsonRazorCshtml);
                        }
                        File.WriteAllText(swaggerJsonRazorCshtml, ArgsRazor);
                        //a++;
                        break;
                    default:
                        showInitConsole();
                        throw new ArgumentException($"错误的参数设置：{args[a]}");
                }
            }
            if (string.IsNullOrWhiteSpace(ArgsSwaggerJsonFileUrl))
            {
                throw new ArgumentException($"错误的参数设置：-FileNameUrl 参数不能为空");
            }
            #endregion GetArguments
            #region 读取内嵌的模板资源
            if (ArgsRazor == null)
            {
                ArgsRazor = GetDefaultRazorContent(assembly);
            }
            #endregion 读取内嵌的模板资源
            //开始生成操作
            {
                string jsondata = string.Empty;
                #region 读取json文件内容
                if (IsUrl(ArgsSwaggerJsonFileUrl))
                {
                    var client = new HttpClient();
                    var res = client.GetAsync(ArgsSwaggerJsonFileUrl).Result;
                    if (!res.IsSuccessStatusCode)
                    {
                        Colorful.Console.WriteFormatted("获取网络文件出错了,详细信息:" + JsonSerializer.Serialize(res.Content.ReadAsStringAsync().Result), Color.Red);
                        throw new ArgumentException(res.StatusCode.ToString());
                    }
                    jsondata = res.Content.ReadAsStringAsync().Result;
                }
                else if (File.Exists(ArgsSwaggerJsonFileUrl))
                {
                    jsondata = File.ReadAllText(ArgsSwaggerJsonFileUrl);
                }
                else
                {
                    throw new ArgumentException($"错误的参数设置：请检查 -FileNameUrl 参数的正确性");
                }

                Colorful.Console.WriteFormatted($"\r\n[{DateTime.Now:MM-dd HH:mm:ss}] 读取文件内容完毕\r\n", Color.DarkGreen);
                #endregion


                //$ref=>_ref , application/json=>application_json , multipart/form-data=>multipart_form_data
                var cleanData = jsondata.Replace("$ref", "_ref").Replace("application/json", "application_json").Replace("multipart/form-data", "multipart_form_data");
                SwaggerModel rawData = JsonSerializer.Deserialize<SwaggerModel>(cleanData);
                SwaggerModel data = JsonSerializer.Deserialize<SwaggerModel>(cleanData);
                var paths = data.paths.Keys;
                var keyList = paths.Select(p => p.Split("/")).ToList();
                var k0 = keyList[0];
                var minLength = keyList.Min(p => p.Length);
                var sameRoute = new List<string>();
                for (int i = 0; i < minLength; i++)
                {
                    if (keyList.All(l => l[i] == k0[i]))
                    {
                        sameRoute.Add(k0[i]);
                    }
                    else
                    {
                        break;
                    }
                }
                var groupDic = new Dictionary<string, List<string>>();
                keyList.ForEach(k =>
                {
                    var diffName = k[sameRoute.Count];
                    if (groupDic.ContainsKey(diffName))
                    {
                        var v = groupDic[diffName];

                        groupDic[diffName].Add(string.Join("/", k));
                    }
                    else
                    {
                        groupDic.Add(diffName, new List<string>() { string.Join("/", k) });
                    }
                });
                int outputCounter = 0;
                var razorId = Guid.NewGuid().ToString("N");
                foreach (var key in groupDic.Keys)
                {
                    data.paths = new Dictionary<string, PathModel>();
                    foreach (var path in groupDic[key])
                    {
                        data.paths.Add(path, rawData.paths[path]);
                    }
                    string razorResult = RazorEngine.Engine.Razor.RunCompile(ArgsRazor, razorId, null, data);
                    razorResult = razorResult.Replace("&quot;", "\"");
                    var fileName = $"api.{key.First().ToString().ToLower() + key[1..]}.js";
                    var fileFullPath = $"{ArgsOutput}/{fileName}";
                    if (File.Exists(fileFullPath))
                    {
                        File.Delete(fileFullPath);
                    }
                    File.WriteAllText(fileFullPath, razorResult);
                    outputCounter++;
                    Colorful.Console.WriteFormatted($"\r\n[{outputCounter}]:{fileName}", Color.BurlyWood);
                }

                Colorful.Console.WriteFormatted($"\r\n\r\n[{DateTime.Now:MM-dd HH:mm:ss}] 生成完毕，总共生成了 {outputCounter} 个文件，目录：\"{ArgsOutput}\"\r\n", Color.DarkGreen);

                if (ArgsReadKey)
                {
                    Console.WriteLine("\r\n按任意键退出");
                    //Console.ReadKey();
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

        private static bool IsUrl(string urlStr) => Regex.IsMatch(urlStr, @"((http|ftp|https)://)(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,4})*(/[a-zA-Z0-9\&%_\./-~-]*)?");
    }
}