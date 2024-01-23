namespace Lox;

class Return : Exception
{
    internal readonly object? Value;

    internal Return(object? value)
    {
        Value = value;
    }
}