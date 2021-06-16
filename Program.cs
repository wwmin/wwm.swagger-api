using System;
using System.Threading;

namespace swagger2js_cli
{
    internal class Program
    {
        private static void Main(string[] args)
        {
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

            if (args != null && args.Length == 0) args = new[] { "?" };
            ManualResetEvent wait = new ManualResetEvent(false);
            new Thread(() =>
            {
                Thread.CurrentThread.Join(TimeSpan.FromSeconds(1));
                ConsoleApp app = new ConsoleApp(args, wait);
            }).Start();
            wait.WaitOne();
            Console.ReadKey();
        }
    }
}