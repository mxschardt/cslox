namespace Lox;

class Instance
{
    private readonly Class @Class;
    private readonly Dictionary<string, object?> Fields = [];

    internal Instance(Class @class)
    {
        @Class = @class;
    }

    internal object? Get(Token name)
    {
        if (Fields.TryGetValue(name.Lexeme, out object? value))
        {
            return value;
        }

        Function? method = @Class.FindMethod(name.Lexeme);
        if (method != null)
        {
            return method.Bind(this);;
        }

        throw new RuntimeException(name, $"Undefined property '{name.Lexeme}.");
    }

    internal void Set(Token name, object? value)
    {
        Fields.Add(name.Lexeme, value);
    }

    public override string ToString()
    {
        return @Class.Name + " instance";
    }
}