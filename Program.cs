using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;
using swagger2js_cli.Models;
using System.Linq;
using Console = Colorful.Console;
using System.Collections.Generic;

namespace swagger2js_cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string outputFolder = "output";
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            //string template = "Hello @Model.Name,welcome to RazorEngine!";
            //var result = Engine.Razor.RunCompile(template, "template", null, new { Name = "World" });
            //Console.WriteLine(result);
            //var r2 = Engine.Razor.RunCompile(template, "template", null, new { Name = "Wmnin" });
            //Console.WriteLine(r2);
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://39.100.85.166:8001/");
                var res = await client.GetAsync("swagger/v1/swagger.json");
                var jsondata = await res.Content.ReadAsStringAsync();
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
                var razorId = Guid.NewGuid().ToString("N");
                foreach (var key in groupDic.Keys)
                {
                    data.paths = new Dictionary<string, PathModel>();
                    //var sameRouteList = sameRoute.Select(p => p).ToList();
                    //sameRouteList.Add(key);
                    //var sameRouteString = string.Join("/", sameRouteList);
                    foreach (var path in groupDic[key])
                    {
                        data.paths.Add(path, rawData.paths[path]);
                    }
                    //await File.WriteAllTextAsync("swaggerJson.json", JsonSerializer.Serialize(data));

                    //var razor = RazorContentManager.swagger_test_cshtml;
                    var razor = File.ReadAllText("Templates/SwaggerJsonRazor.cshtml");
                    string razorResult = RazorEngine.Engine.Razor.RunCompile(razor, razorId, null, data);
                    razorResult = razorResult.Replace("&quot;", "\"");

                    var apiJsText = $"{outputFolder}/api.{key.First().ToString().ToLower() + key[1..]}.js";
                    if (File.Exists(apiJsText))
                    {
                        File.Delete(apiJsText);
                    }
                    File.WriteAllText(apiJsText, razorResult);
                    System.Console.WriteLine(razorResult);
                }
            }
            //{
            //    string templateFilePath = "HelloWorld.cshtml";
            //    var templateFile = File.ReadAllText(templateFilePath);
            //    string templateFileResult = Engine.Razor.RunCompile(templateFile, Guid.NewGuid().ToString("N"), null, new CopyRightUserInfo
            //    {
            //        CreateTime = DateTime.Now,
            //        EmailAddress = "wwei.min@163.com",
            //        UserName = "wwmin"
            //    });

            //    var rebuildBat = "_重新生成.bat";
            //    if (File.Exists(rebuildBat) == false)
            //    {
            //        var razorCshtml = "_razor.cshtml.txt";
            //        if (File.Exists(razorCshtml) == false)
            //        {
            //            File.WriteAllText(razorCshtml, templateFileResult);
            //            Console.WriteFormatted("生成成功", Color.Magenta);
            //        }
            //    }

            //    Console.WriteFormatted(templateFileResult, Color.Magenta);
            //}
            Console.ReadKey();
        }
    }
}
