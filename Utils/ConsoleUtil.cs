namespace wwm.swagger_api;

/// <summary>
/// Console 帮助类
/// </summary>
public static class ConsoleUtil
{
    /// <summary>
    /// 写入两个值的console
    /// </summary>
    /// <param name="message1"></param>
    /// <param name="message2"></param>
    public static void Write(string message1, ConsoleColor color)
    {
        var currentConsoleColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(message1);
        Console.ForegroundColor = currentConsoleColor;
    }

    /// <summary>
    /// 写入两个值的console
    /// </summary>
    /// <param name="message1"></param>
    /// <param name="message2"></param>
    public static void WriteLine(string message1, ConsoleColor color)
    {
        var currentConsoleColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(message1);
        Console.ForegroundColor = currentConsoleColor;
        Console.WriteLine();
    }   


    /// <summary>
    /// 写入两个值的console
    /// </summary>
    /// <param name="message1"></param>
    /// <param name="message2"></param>
    public static void WriteLine(string message1, string? message2 = null)
    {
        var currentConsoleColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(message1);
        if (message2 != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(message2);
        }
        Console.ForegroundColor = currentConsoleColor;
        Console.WriteLine();
    }

    /// <summary>
    /// 写入两个值的console
    /// </summary>
    /// <param name="message1"></param>
    /// <param name="message2"></param>
    public static void WriteInfoLine(string message1, string? message2 = null)
    {
        var currentConsoleColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(message1);
        if (message2 != null)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(message2);
        }
        Console.ForegroundColor = currentConsoleColor;
        Console.WriteLine();
    }

    /// <summary>
    /// 写入两个值的console
    /// </summary>
    /// <param name="message1"></param>
    /// <param name="message2"></param>
    public static void WriteErrorLine(string message1, string? message2 = null)
    {
        var currentConsoleColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write(message1);
        if (message2 != null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(message2);
        }
        Console.ForegroundColor = currentConsoleColor;
        Console.WriteLine();
    }

}
