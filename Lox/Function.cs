namespace Lox;

class Function : ICallable
{
    private readonly Stmt.Function Declaration;
    private readonly Environment Closure;

    internal Function(Stmt.Function declaration, Environment closure)
    {
        Declaration = declaration;
        Closure = closure;
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
            return returnValue.Value;
        }

        return null;
    }

    public override string ToString()
    {
        return $"<fn {Declaration.Name.Lexeme} >";
    }
}