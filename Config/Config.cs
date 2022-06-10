using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace wwm.swaggerApi;

/// <summary>
/// 配置项
/// </summary>
public class Config
{
    /// <summary>
    /// 构造Config
    /// </summary>
    /// <param name="configName"></param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public static Config Build(string configName = "wwm.swagger-api.json")
    {
        // 1. 此为命令行执行路径,可能执行目录不在当前目录
        //var currentDirectory = Directory.GetCurrentDirectory();
        // 2. 此为运行时所在的路径 (使用路径加载时仍无效) 
        //var currentDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
        // 3. 程序进程所在目录
        var currentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
        if (string.IsNullOrEmpty(currentDirectory))
        {
            throw new ApplicationException("系统未获取到当前执行路径");
        }
        string configPath = Path.Combine(currentDirectory, configName);
        var config = new Config();
        if (File.Exists(configPath))
        {
            StringBuilder sb = new StringBuilder();
            var allLines = File.ReadAllLines(configPath, encoding: Encoding.UTF8);
            foreach (var line in allLines)
            {
                string lineText = line?.Trim() ?? "";
                if (lineText.StartsWith("\"OutPath\""))
                {
                    lineText = lineText.Replace("\\", "/");
                    sb.AppendLine(lineText);
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            var configText = sb.ToString();
            config = JsonSerializer.Deserialize<Config>(configText, new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                PropertyNameCaseInsensitive = false,
                //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReadCommentHandling = JsonCommentHandling.Skip
            });
            if (config == null) throw new JsonException("格式化配置文件出现错误");
            if (!config.OutPath.EndsWith("/")) config.OutPath += "/";
        }
        if (string.IsNullOrEmpty(config.OutPath))
        {
            config.OutPath = Path.Combine(currentDirectory, "/");
        }
        config.OutPath = config.OutPath.Replace("\\", "/");
        if (config.OutPath.StartsWith("/"))
        {
            config.OutPath = Path.Join(currentDirectory, config.OutPath);
        }
        if (!Directory.Exists(config.OutPath)) Directory.CreateDirectory(config.OutPath);
        if (string.IsNullOrEmpty(config.JsonUrl))
        {
            throw new JsonException("json路径不能为空");
        }
        if (config.OutPath.StartsWith("."))
        {
            config.OutPath = Path.Combine(currentDirectory, config.OutPath);
        }
        if (string.IsNullOrWhiteSpace(config.ApiFolderName)) config.ApiFolderName = "api";
        if (string.IsNullOrWhiteSpace(config.ApiInterfaceFolderName)) config.ApiInterfaceFolderName = "apiInterface";
        if (string.IsNullOrWhiteSpace(config.ImportHttp)) config.ImportHttp = "import http from \"../index\"";
        return config;
    }


    /// <summary>
    /// 输出路径
    /// </summary>
    public string OutPath { get; set; } = string.Empty;
    /// <summary>
    /// Json url
    /// </summary>
    public string JsonUrl { get; set; } = string.Empty;
    /// <summary>
    /// 文件头文字
    /// </summary>
    public string FileHeadText { get; set; } = string.Empty;
    /// <summary>
    /// 函数自定尾参数
    /// </summary>
    public string FuncTailParameter { get; set; } = string.Empty;
    /// <summary>
    /// Api文件文件夹名称
    /// </summary>
    public string ApiFolderName { get; set; } = string.Empty;
    /// <summary>
    /// ApiInterface文件夹名称
    /// </summary>
    public string ApiInterfaceFolderName { get; set; } = string.Empty;
    /// <summary>
    /// ImportApi
    /// </summary>
    public string ImportHttp { get; set; } = string.Empty;
    /// <summary>
    /// 空格缩进个数
    /// </summary>
    public int IndentSpaceNum { get; set; } = 2;
}
