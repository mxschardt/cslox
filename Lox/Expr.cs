namespace Lox;

abstract class Expr
{
    internal interface IVisitor<T>
    {
        T VisitBinaryExpr(Binary expr);
        T VisitGroupingExpr(Grouping expr);
        T VisitLiteralExpr(Literal expr);
        T VisitUnaryExpr(Unary expr);
    }

    internal abstract T Accept<T>(IVisitor<T> visitor);

    internal class Binary : Expr
    {
        internal readonly Expr Left;
        internal readonly Token Oper;
        internal readonly Expr Right;

        internal Binary(Expr left, Token oper, Expr right)
        {
          Left = left;
          Oper = oper;
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
        internal readonly object Value;

        internal Literal(object value)
        {
          Value = value;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }
    }

    internal class Unary : Expr
    {
        internal readonly Token Oper;
        internal readonly Expr Right;

        internal Unary(Token oper, Expr right)
        {
          Oper = oper;
          Right = right;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
    }

}
