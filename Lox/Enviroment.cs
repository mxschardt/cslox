namespace Lox;

internal class Environment
{
    private readonly Environment? Enclosing = null;
    private readonly Dictionary<string, object?> Values = [];

    public Environment() { }

    public Environment(Environment enclosing)
    {
        Enclosing = enclosing;
    }

    internal void Define(string name, object? value)
    {
        Values.Add(name, value);
    }

    internal void Assign(Token name, object value)
    {
        if (Values.ContainsKey(name.Lexeme))
        {
            Values[name.Lexeme] =  value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");

    }

    internal object? Get(Token name)
    {
        if (Values.TryGetValue(name.Lexeme, out object? value))
        {
            return value;
        }

        if (Enclosing != null)
        {
            return Enclosing.Get(name);
        }

        throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }
}