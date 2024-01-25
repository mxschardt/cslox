namespace Lox;

class Resolver : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
    private readonly Interpreter Interpreter;
    private readonly Stack<IDictionary<string, bool>> Scopes = new();
    private FunctionType CurrentFunction = FunctionType.NONE;
    private enum ClassType {
        NONE,
        CLASS
    }
    private ClassType CurrentClass = ClassType.NONE;

    internal Resolver(Interpreter interpreter)
    {
        Interpreter = interpreter;
    }

    internal void Resolve(List<Stmt> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void BeginScope()
    {
        Scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        Scopes.Pop();
    }

    private void Declare(Token name)
    {
        if (Scopes.Count == 0)
        {
            return;
        }

        var scope = Scopes.Peek();
        if (scope.ContainsKey(name.Lexeme))
        {
            Lox.Error(name, "Already a variable with this name in this scope.");
        }

        scope.Add(name.Lexeme, false);
    }

    private void Define(Token name)
    {
        if (Scopes.Count == 0)
        {
            return;
        }

        Scopes.Peek()[name.Lexeme] = true;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        for (var i = 0; i < Scopes.Count; i++)
        {
            if (Scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                Interpreter.Resolve(expr, i);
                return;
            }
        }
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        FunctionType enclosingFunction = CurrentFunction;
        CurrentFunction = type;
        BeginScope();
        foreach (Token param in function.Params)
        {
            Declare(param);
            Define(param);
        }
        Resolve(function.Body);
        EndScope();
        CurrentFunction = enclosingFunction;
    }

    object? Expr.IVisitor<object?>.VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    object? Expr.IVisitor<object?>.VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitBlockStmt(Stmt.Block expr)
    {
        BeginScope();
        Resolve(expr.Statements);
        EndScope();
        return null;
    }

    object? Expr.IVisitor<object?>.VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);

        foreach (Expr argument in expr.Arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    object? Expr.IVisitor<object?>.VisitCommaExpr(Expr.Comma expr)
    {
        throw new NotImplementedException();
    }

    object? Stmt.IVisitor<object?>.VisitClassStmt(Stmt.Class stmt)
    {
        ClassType enclosingClass = CurrentClass;
        CurrentClass = ClassType.CLASS;

        Declare(stmt.Name);
        Define(stmt.Name);

        BeginScope();
        Scopes.Peek().Add("this", true);

        foreach (var method in stmt.Methods)
        {
            FunctionType declaration = FunctionType.METHOD;
            if (method.Name.Lexeme.Equals("init"))
            {
                declaration = FunctionType.INITIALIZER;
            }
            ResolveFunction(method, declaration);
        }

        EndScope();

        CurrentClass = enclosingClass;
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    object? Expr.IVisitor<object?>.VisitGetExpr(Expr.Get expr)
    {
        Resolve(expr.Object);
        return null;
    }

    object? Expr.IVisitor<object?>.VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if (stmt.ElseBranch != null)
        {
            Resolve(stmt.ElseBranch);
        }
        return null;
    }

    object? Expr.IVisitor<object?>.VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    object? Expr.IVisitor<object?>.VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    object? Expr.IVisitor<object?>.VisitSetExpr(Expr.Set expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Object);
        return null;
    }

    object? Expr.IVisitor<object?>.VisitThisExpr(Expr.This expr)
    {
        if (CurrentClass == ClassType.NONE)
        {
            Lox.Error(expr.Keyword, "Can't use 'this' outside of a class");
        }

        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitReturnStmt(Stmt.Return stmt)
    {
        if (stmt.Value != null)
        {
            if (CurrentFunction == FunctionType.INITIALIZER)
            {
                Lox.Error(stmt.Keyword, "Can't return from an initializer");
            }
            Resolve(stmt.Value);
        }
        return null;
    }

    object? Expr.IVisitor<object?>.VisitTernaryExpr(Expr.Ternary expr)
    {
        throw new NotImplementedException();
    }

    object? Expr.IVisitor<object?>.VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    object? Expr.IVisitor<object?>.VisitVariableExpr(Expr.Variable expr)
    {
        if (Scopes.Count != 0 && Scopes.Peek().TryGetValue(expr.Name.Lexeme, out var v) && v == false)
        {
            Lox.Error(expr.Name, "Can't read local variable in its own initializer.");
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer != null)
        {
            Resolve(stmt.Initializer);
        }
        Define(stmt.Name);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return null;
    }
}

enum FunctionType
{
    NONE,
    FUNCTION,
    INITIALIZER,
    METHOD
}