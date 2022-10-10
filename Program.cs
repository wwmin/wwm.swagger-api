namespace wwm.swagger_api;

internal class Program
{
    private static void Main(string[] args)
    {
        var name = "wwm.swagger-api";
        Console.Title = name;
        if (args == null || args.Length == 0) args = new[] { "?" };
        ManualResetEvent wait = new(false);
        new Thread(() =>
        {
            Thread.CurrentThread.Join(TimeSpan.FromSeconds(1));
            ConsoleApp app = new ConsoleApp(args, wait);
        }).Start();
        wait.WaitOne();
        Console.ReadKey();
    }
}
