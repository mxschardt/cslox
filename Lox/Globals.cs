namespace Lox.Globals;

class Clock : ICallable
{
    public int Arity() => 0;
    public object Call(Interpreter interpreter, List<object> arguments)
    {
        return DateTime.Now.Millisecond / 1000.0;
    }
    public override string ToString() => "<native fn>";
}