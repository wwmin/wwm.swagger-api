using System.Text;
using System.Text.Json;

namespace swagger2js_cli;

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
    public static Config Build(string configName = "appsettings.json")
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        string configPath = Path.Combine(currentDirectory, configName);
        var config = new Config();
        if (File.Exists(configPath))
        {
            var configText = File.ReadAllText(configPath, encoding: Encoding.UTF8);
            config = JsonSerializer.Deserialize<Config>(configText, new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReadCommentHandling = JsonCommentHandling.Skip
            });
            if (config == null) throw new JsonException("格式化配置文件出现错误");
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
}
