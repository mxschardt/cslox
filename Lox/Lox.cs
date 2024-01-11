using System.Text;

namespace Lox;

public class Lox
{
    internal static bool hadError = false;

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: cslox [script]");
            Environment.Exit(0);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    private static void RunFile(string path)
    {
        var exists = File.Exists(path);
        if (!exists)
        {
            Console.WriteLine("No such file.");
            Environment.Exit(65);
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        Run(text);

        if (hadError)
        {
            Environment.Exit(65);
        }
    }

    private static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null)
            {
                Console.WriteLine("end of input");
                break;
            }
            Run(line);

            hadError = false;
        }
    }

    private static void Run(string input)
    {
        var scanner = new Scanner(input);
        List<Token> tokens = scanner.ScanTokens();

        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        TextWriter errorWriter = Console.Error;
        errorWriter.WriteLine($"[line {line}] Error {where}: {message}");
        hadError = true;
    }
}
