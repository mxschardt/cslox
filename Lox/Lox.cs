using System.Text;

namespace Lox;

public class Lox
{
    internal static readonly Interpreter Interpreter = new();
    internal static bool hadError = false;
    internal static bool hadRuntimeError = false;
    internal static bool Repl = false;

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: cslox [script]");
            System.Environment.Exit(0);
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
            System.Environment.Exit(65);
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        Run(text);

        if (hadError)
        {
            System.Environment.Exit(65);
        }
        if (hadRuntimeError)
        {
            System.Environment.Exit(70);
        }
    }

    private static void RunPrompt()
    {
        Repl = true;
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
        var parser = new Parser(tokens);
        var statements = parser.Parse();

        if (hadError || statements == null) return;

        Resolver resolver = new(Interpreter);
        resolver.Resolve(statements);

        if (hadError) return;

        if (Repl)
        {
            Interpreter.InterpretRepl(statements);
        }
        else
        {
            Interpreter.Interpret(statements);
        }
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    internal static void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, " at '" + token.Lexeme + "'", message);
        }
    }

    internal static void RuntimeException(RuntimeException error)
    {
        Console.WriteLine($"[line {error.Token.Line}] Error at '{error.Token.Lexeme}': {error.Message}");
        hadRuntimeError = true;
    }

    private static void Report(int line, string where, string message)
    {
        TextWriter errorWriter = Console.Error;
        errorWriter.WriteLine($"[line {line}] Error {where}: {message}");
        hadError = true;
    }
}