namespace Lox;

internal class Environment
{
    private readonly Environment? Enclosing = null;
    private readonly Dictionary<string, object?> Values = [];
    private readonly HashSet<string> Undefined = [];

    public Environment() { }

    public Environment(Environment enclosing)
    {
        Enclosing = enclosing;
    }

    internal void Define(string name, object? value)
    {
        Values.Add(name, value);
    }

    internal object? GetAt(int distance, string name)
    {
        return Ancestor(distance).Values[name];
    }

    internal void AssignAt(int distance, Token name, object value)
    {
        Ancestor(distance).Values.Add(name.Lexeme, value);
    }

    Environment Ancestor(int distance)
    {
        // Variable must exist because Resolver already found it.
#pragma warning disable CS8602, CS8603 // Possible null reference.
        var environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.Enclosing;
        }
        return environment;
#pragma warning restore CS8602, CS8603
    }

    internal void Announce(string name)
    {
        Undefined.Add(name);
    }

    internal void Assign(Token name, object value)
    {
        if (Values.ContainsKey(name.Lexeme))
        {
            Values[name.Lexeme] = value;
            return;
        }

        if (Undefined.Contains(name.Lexeme))
        {
            Define(name.Lexeme, value);
            Undefined.Remove(name.Lexeme);
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

        if (Undefined.Contains(name.Lexeme))
        {
            throw new RuntimeException(name, $"Variable '{name.Lexeme}' isn't initialized.");
        }

        if (Enclosing != null)
        {
            return Enclosing.Get(name);
        }

        throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }
}