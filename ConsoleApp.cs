﻿using swagger2js_cli.Models;
using swagger2js_cli.Processes;

using System.Drawing;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace swagger2js_cli;

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
        _config = Config.Build();

        var assembly = typeof(ConsoleApp).Assembly;
        var version = "v" + string.Join(".", assembly.GetName()?.Version?.ToString()?.Split(".", StringSplitOptions.RemoveEmptyEntries)?.Where((a, b) => b <= 2) ?? Array.Empty<string>());
        var logo = $@"
                                        ___  _     
                                       |__ \(_)    
  _____      ____ _  __ _  __ _  ___ _ __ ) |_ ___ 
 / __\ \ /\ / / _` |/ _` |/ _` |/ _ \ '__/ /| / __|
 \__ \\ V  V / (_| | (_| | (_| |  __/ | / /_| \__ \
 |___/ \_/\_/ \__,_|\__, |\__, |\___|_||____| |___/
                     __/ | __/ |           _/ |    
                    |___/ |___/           |__/     
";
        ConsoleUtil.WriteLine(logo, ConsoleColor.White);
        ConsoleUtil.Write(@"# Github # ", ConsoleColor.White);
        ConsoleUtil.Write("https://github.com/wwmin/swagger2js_cli", ConsoleColor.Green);
        ConsoleUtil.WriteLine($" {version}", ConsoleColor.DarkGreen);

        ArgsReadKey = true;
        #region 对Text.Json统一配置
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        #endregion
        //开始生成操作
        {
            string jsondata = string.Empty;
            #region 读取json文件内容
            if (IsUrl(_config.JsonUrl))
            {
                var client = new HttpClient();
                var res = client.GetAsync(_config.JsonUrl).Result;
                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine("获取网络文件出错了,详细信息:" + JsonSerializer.Serialize(res.Content.ReadAsStringAsync().Result, jsonOptions), ConsoleColor.Red);
                    throw new ArgumentException(res.StatusCode.ToString());
                }
                jsondata = res.Content.ReadAsStringAsync().Result;
            }
            else if (File.Exists(_config.JsonUrl))
            {
                jsondata = File.ReadAllText(_config.JsonUrl);
            }
            else
            {
                throw new ArgumentException($"错误的参数设置：请检查 配置文件中 JsonUrl 参数的正确性");
            }

            ConsoleUtil.WriteLine($"\r\n[{DateTime.Now:MM-dd HH:mm:ss}] 读取文件内容完毕\r\n", ConsoleColor.DarkGreen);
            #endregion
            #region 处理json
            //$ref=>@ref
            jsondata = jsondata.Replace("$ref", "_ref");
            var swagger = JsonSerializer.Deserialize<SwaggerModel>(jsondata, jsonOptions);
            // 生成Interface文件
            {
                string filePath = _config.OutPath + "apiInterface/index.ts";
                TypeScriptInterfaceProcess.GenerateTypeScriptTypesFromJsonModel(swagger?.components, filePath);
                ConsoleUtil.WriteLine("接口文件路径: " + filePath, ConsoleColor.DarkRed);
                Console.WriteLine();
            }
            //生成api.{name}.ts文件
            {
                string baseFile = _config.OutPath + "api/";
                string interfacePre = "IApi";
                string filePreText = $"import * as {interfacePre} from \"../apiInterface\";\n" +
                    "import http from \"../index\"\n\n";
                TypeScriptApiProcess.GenerateTypeScriptApiFromJsonModel(swagger, baseFile, filePreText, interfacePre);
                ConsoleUtil.WriteLine("接口文件夹: " + baseFile, ConsoleColor.DarkRed);

            }

            #endregion
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

    private static bool IsUrl(string urlStr) => Regex.IsMatch(urlStr, @"^((https?)(:))?(\/\/)(\w*|\d*)");
}
