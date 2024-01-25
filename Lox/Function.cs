namespace Lox;

class Function : ICallable
{
    private readonly Stmt.Function Declaration;
    private readonly Environment Closure;
    private readonly bool IsInitializer;

    internal Function(Stmt.Function declaration, Environment closure, bool isInitializer)
    {
        Declaration = declaration;
        Closure = closure;
        IsInitializer = isInitializer;
    }

    internal Function Bind(Instance instance)
    {
        Environment environment = new(Closure);
        environment.Define("this", instance);
        return new Function(Declaration, environment, IsInitializer);
    }

    public int Arity()
    {
        return Declaration.Params.Count;
    }

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        Environment environment = new(Closure);
        for (var i = 0; i < Declaration.Params.Count; i++)
        {
            environment.Define(Declaration.Params[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(Declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            if (IsInitializer)
            {
                return Closure.GetAt(0, "this");
            }
            return returnValue.Value;
        }

        if (IsInitializer)
        {
            return Closure.GetAt(0, "this");
        }

        return null;
    }

    public override string ToString()
    {
        return $"<fn {Declaration.Name.Lexeme} >";
    }
}