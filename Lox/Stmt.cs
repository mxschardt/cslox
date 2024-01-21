namespace Lox;

abstract class Stmt
{
    internal interface IVisitor<T>
    {
        T VisitExpressionStmt(Expression stmt);
        T VisitPrintStmt(Print stmt);
        T VisitVarStmt(Var stmt);
        T VisitBlockStmt(Block block);
    }

    internal abstract T Accept<T>(IVisitor<T> visitor);

    internal class Expression : Stmt
    {
        internal readonly Expr Expr;

        internal Expression(Expr expression)
        {
          Expr = expression;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExpressionStmt(this);
        }
    }

    internal class Print : Stmt
    {
        internal readonly Expr Expr;

        internal Print(Expr expression)
        {
          Expr = expression;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitPrintStmt(this);
        }
    }

    internal class Var : Stmt
    {
        internal readonly Token Name;
        internal readonly Expr? Initializer;

        internal Var(Token name, Expr? initializer)
        {
          Name = name;
          Initializer = initializer;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVarStmt(this);
        }
    }

    internal class Block : Stmt
    {
        internal readonly List<Stmt> Statements;

        internal Block(List<Stmt> statements)
        {
            Statements = statements;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBlockStmt(this);
        }
    }
}