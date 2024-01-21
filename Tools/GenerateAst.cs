namespace Tools;

public class GenerateAst
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: generate_ast <output dir>");
            Environment.Exit(64);
        }
        var outputDir = args[0];

        List<string> list = [
            "Binary   : Expr left, Token oper, Expr right",
            "Grouping : Expr expression",
            "Literal  : object value",
            "Unary    : Token oper, Expr right",
            "Variable : Token name"
        ];

        // DefineAst(outputDir, "Expr", list);
        DefineAst(outputDir, "Stmt", [
            "Expression : Expr expression",
            "Print      : Expr expression",
            "Var        : Token name, Expr initializer"
        ]);
    }

    private static void DefineAst(string outputDir, string baseName, List<string> types)
    {
        var path = outputDir + "/" + baseName + ".cs";
        using var writer = new StreamWriter(path);

        writer.WriteLine("namespace Lox;");
        writer.WriteLine();
        writer.WriteLine("abstract class " + baseName);
        writer.WriteLine("{");

        DefineVisitor(writer, baseName, types);
        writer.WriteLine("    internal abstract T Accept<T>(IVisitor<T> visitor);");
        writer.WriteLine();

        foreach (var type in types)
        {
            string className = type.Split(":")[0].Trim();
            string fields = type.Split(":")[1].Trim();
            DefineType(writer, baseName, className, fields);
            writer.WriteLine();
        }

        writer.WriteLine("}");
    }

    private static void DefineType(
        StreamWriter writer, string baseName,
        string className, string fieldList)
    {
        writer.WriteLine("    internal class " + className + " : " + baseName);
        writer.WriteLine("    {");

        // Store parameters in fields.
        string[] fields = fieldList.Split(", ");

        // Fields.
        foreach (var field in fields)
        {
            var f = field.Split(" ");
            writer.WriteLine("        " + "internal readonly " + f[0] + " " + Capitalize(f[1]) + ";");
        }

        // Constructor.
        writer.WriteLine();
        writer.WriteLine("        internal " + className + "(" + fieldList + ")");
        writer.WriteLine("        {");

        foreach (var field in fields)
        {
            var name = field.Split(" ")[1];
            writer.WriteLine("          " + Capitalize(name) + " = " + name + ";");
        }
        writer.WriteLine("        }");
        writer.WriteLine();

        writer.WriteLine("        internal override T Accept<T>(IVisitor<T> visitor)");
        writer.WriteLine("        {");
        writer.WriteLine("            return visitor.Visit" + className + baseName + "(this);");
        writer.WriteLine("        }");

        writer.WriteLine("    }");
    }

    private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
    {
        writer.WriteLine("    internal interface IVisitor<T>");
        writer.WriteLine("    {");
        foreach (var type in types)
        {
            var typeName = type.Split(":")[0].Trim();
            writer.WriteLine("        T Visit" + typeName + baseName + "(" + typeName + " " + baseName.ToLower() + ");");
        }
        writer.WriteLine("    }");
    }

    private static string Capitalize(string input)
    {
        ArgumentException.ThrowIfNullOrEmpty(input);
        return string.Concat(input[0].ToString().ToUpper(), input[1..].ToString().AsSpan());
    }
}
