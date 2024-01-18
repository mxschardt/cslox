namespace Lox;

abstract class Expr
{
    internal interface IVisitor<T>
    {
        T VisitBinaryExpr(Binary expr);
        T VisitGroupingExpr(Grouping expr);
        T? VisitLiteralExpr(Literal expr);
        T VisitUnaryExpr(Unary expr);
        T VisitCommaExpr(Comma expr);
        T VisitTernaryExpr(Ternary ternary);
    }

    internal abstract T Accept<T>(IVisitor<T> visitor);

    internal class Binary : Expr
    {
        internal readonly Expr Left;
        internal readonly Token Operator;
        internal readonly Expr Right;

        internal Binary(Expr left, Token @operator, Expr right)
        {
            Left = left;
            Operator = @operator;
            Right = right;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpr(this);
        }
    }

    internal class Grouping : Expr
    {
        internal readonly Expr Expression;

        internal Grouping(Expr expression)
        {
            Expression = expression;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpr(this);
        }
    }

    internal class Literal : Expr
    {
        internal readonly object? Value;

        internal Literal(object? value)
        {
            Value = value;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return visitor.VisitLiteralExpr(this);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }

    internal class Unary : Expr
    {
        internal readonly Token Operator;
        internal readonly Expr Right;

        internal Unary(Token @operator, Expr right)
        {
            Operator = @operator;
            Right = right;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
    }

    internal class Comma : Expr
    {
        internal readonly Expr Left;
        internal readonly Token Operator;
        internal readonly Expr Right;

        internal Comma(Expr left, Token @operator, Expr right)
        {
            Left = left;
            Operator = @operator;
            Right = right;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitCommaExpr(this);
        }
    }

    internal class Ternary : Expr
    {
        internal readonly Expr Condition;
        internal readonly Token Operator;
        internal readonly Expr Left;
        internal readonly Expr Right;

        internal Ternary(Expr condition, Token @operator, Expr left, Expr right)
        {
            Condition = condition;
            Operator = @operator;
            Left = left;
            Right = right;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitTernaryExpr(this);
        }
    }
}
