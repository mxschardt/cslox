namespace Lox;

abstract class Stmt
{
    internal interface IVisitor<T>
    {
        T VisitExpressionStmt(Expression stmt);
        T VisitPrintStmt(Print stmt);
        T VisitVarStmt(Var stmt);
        T VisitBlockStmt(Block block);
        T VisitIfStmt(If stmt);
        T VisitWhileStmt(While stmt);
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

    internal class If : Stmt
    {
        internal readonly Expr Condition;
        internal readonly Stmt ThenBranch;
        internal readonly Stmt? ElseBranch;

        internal If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitIfStmt(this);
        }
    }
    internal class While : Stmt
    {
        internal readonly Expr Condition;
        internal readonly Stmt Body;

        internal While(Expr condition, Stmt body)
        {
            Condition = condition;
            Body = body;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitWhileStmt(this);
        }
    }
}