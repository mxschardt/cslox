namespace Lox;

class Class : ICallable
{
    internal readonly string Name;
    internal readonly IDictionary<string, Function> Methods;

    internal Class(string name, IDictionary<string, Function> methods)
    {
        Name = name;
        Methods = methods;
    }

    internal Function? FindMethod(string name)
    {
        Methods.TryGetValue(name, out var method);
        return method;
    }

    public override string ToString()
    {
        return Name;
    }

    public int Arity() {
        Function? initializer = FindMethod("init");
        if (initializer == null)
        {
            return 0;
        }
        return initializer.Arity();
    }

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        Instance instance = new(this);
        Function? initializer = FindMethod("init");
        initializer?.Bind(instance).Call(interpreter, arguments);
        return instance;
    }
}