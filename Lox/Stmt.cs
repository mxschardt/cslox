namespace Lox;

abstract class Stmt
{
    internal interface IVisitor<T>
    {
        T VisitExpressionStmt(Expression stmt);
        T VisitPrintStmt(Print stmt);
        T VisitVarStmt(Var stmt);
        T VisitBlockStmt(Block stmt);
        T VisitIfStmt(If stmt);
        T VisitWhileStmt(While stmt);
        T VisitFunctionStmt(Function stmt);
        T VisitReturnStmt(Return stmt);
        T VisitClassStmt(Class stmt);
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

    internal class Function : Stmt
    {
        internal readonly Token Name;
        internal readonly List<Token> Params;
        internal readonly List<Stmt> Body;

        internal Function(Token name, List<Token> @params, List<Stmt> body)
        {
            Name = name;
            Params = @params;
            Body = body;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitFunctionStmt(this);
        }
    }

    internal class Return : Stmt
    {
        internal readonly Token Keyword;
        internal readonly Expr? Value;

        internal Return(Token keyword, Expr? value)
        {
            Keyword = keyword;
            Value = value;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitReturnStmt(this);
        }
    }

    internal class Class : Stmt
    {
        internal readonly Token Name;
        // internal readonly Expr.Variable Superclass;
        internal readonly List<Function> Methods;

        internal Class(Token name,
        // Expr.Variable superclass,
        List<Function> methods)
        {
            Name = name;
            // Superclass = superclass;
            Methods = methods;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitClassStmt(this);
        }
    }
}