using System.Diagnostics;
using static Lox.TokenType;

namespace Lox;

class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object?>
{
    internal readonly Environment Globals = new();
    private Environment Environment;
    private readonly Dictionary<Expr, int> Locals = [];

    internal Interpreter()
    {
        Environment = Globals;
        Globals.Define("clock", new Globals.Clock());
    }

    internal void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeException ex)
        {
            Lox.RuntimeException(ex);
        }
        catch (NotImplementedException)
        {
            // TODO
        }
        catch (UnreachableException)
        {
            throw;
        }
    }

    internal void InterpretRepl(List<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                if (statement is Stmt.Expression expr)
                {
                    object value = Evaluate(expr.Expr);
                    Console.WriteLine(Stringify(value));
                }
                else
                {
                    Execute(statement);
                }
            }
        }
        catch (RuntimeException ex)
        {
            Lox.RuntimeException(ex);
        }
        catch (NotImplementedException)
        {
            // TODO
        }
        catch (UnreachableException)
        {
            throw;
        }
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private object Evaluate(Expr expression)
    {
        return expression.Accept(this);
    }

    private static string Stringify(object literal)
    {
        if (literal == null)
        {
            return "nil";
        }
        if (literal is double obj)
        {
            string text = obj.ToString();
            if (text.EndsWith(".0"))
            {
                text = text[0..-2];
            }
            return text;
        }
        return literal.ToString() ?? throw new UnreachableException();
    }

    internal void Resolve(Expr expr, int depth)
    {
        Locals.Add(expr, depth);
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        object left = Evaluate(expr.Left);
        object right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case GREATER:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left > (double)right;
            case GREATER_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left >= (double)right;
            case LESS:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left < (double)right;
            case LESS_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left <= (double)right;

            case MINUS:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left - (double)right;
            case SLASH:
                CheckNumberOperands(expr.Operator, left, right);
                if ((double)right == 0)
                {
                    throw new RuntimeException(expr.Operator, "Division by zero");
                }
                return (double)left / (double)right;
            case STAR:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left * (double)right;
            case PLUS:
                return (left, right) switch
                {
                    (double l, double r) => l + r,
                    (string l, string r) => l + r,
                    (double l, string r) => l.ToString() + r,
                    (string l, double r) => l + r.ToString(),
                    _ => throw new RuntimeException(expr.Operator, "Operands must be numbers or strings.")
                };

            case BANG_EQUAL:
                return !IsEqual(left, right);
            case EQUAL_EQUAL:
                return IsEqual(left, right);
        };

        throw new UnreachableException();
    }

    public object? VisitGetExpr(Expr.Get expr)
    {
        object obj = Evaluate(expr.Object);
        if (obj is Instance instance)
        {
            return instance.Get(expr.Name);
        }

        throw new RuntimeException(expr.Name, "Only instances have properties");
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        object right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case BANG:
                return !IsTruthy(right);
            case MINUS:
                CheckNumberOperand(expr.Operator, right);
                return -(double)right;
        }
        throw new UnreachableException();
    }

    public object VisitTernaryExpr(Expr.Ternary expr)
    {
        object condition = Evaluate(expr.Condition);

        if (IsTruthy(condition))
        {
            return Evaluate(expr.Left);
        }
        else
        {
            return Evaluate(expr.Right);
        }
    }

    public object VisitCommaExpr(Expr.Comma expr)
    {
        Evaluate(expr.Left);
        return Evaluate(expr.Right);
    }

    object? Stmt.IVisitor<object?>.VisitClassStmt(Stmt.Class stmt)
    {
        Environment.Define(stmt.Name.Lexeme, null);
        Dictionary<string, Function> methods = [];
        foreach (var method in stmt.Methods)
        {
            Function function = new(method, Environment, method.Name.Lexeme.Equals("init"));
            methods.Add(method.Name.Lexeme, function);
        }
        Class @class = new Class(stmt.Name.Lexeme, methods);
        Environment.Assign(stmt.Name, @class);
        return null;
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        return LookUpVariable(expr.Name, expr);
    }

    private object? LookUpVariable(Token name, Expr expr)
    {
        if (Locals.TryGetValue(expr, out int distance))
        {
            return Environment.GetAt(distance, name.Lexeme);
        }
        return Globals.Get(name);
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        object value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        if (stmt.Initializer == null)
        {
            Environment.Announce(stmt.Name.Lexeme);
        }
        else
        {
            var value = Evaluate(stmt.Initializer);
            Environment.Define(stmt.Name.Lexeme, value);
        }

        return null;
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
        object value = Evaluate(expr.Value);

        if (Locals.TryGetValue(expr, out int distance))
        {
            Environment.AssignAt(distance, expr.Name, value);
        }
        else
        {
            Globals.Assign(expr.Name, value);
        }

        return value;
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(Environment));
        return null;
    }

    public object VisitLogicalExpr(Expr.Logical logical)
    {
        object left = Evaluate(logical.Left);

        if (logical.Operator.Type == OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(logical.Right);
    }

    public object VisitSetExpr(Expr.Set expr)
    {
        object obj = Evaluate(expr.Object);

        if (obj is not Instance ins)
        {
            throw new RuntimeException(expr.Name, "Only instances have fields");
        }

        object value = Evaluate(expr.Value);
        ins.Set(expr.Name, value);
        return value;
    }

    public object VisitThisExpr(Expr.This expr)
    {
        return LookUpVariable(expr.Keyword, expr);
    }


    internal void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        Environment previous = Environment;

        try
        {
            Environment = environment;

            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            Environment = previous;
        }

    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            Execute(stmt.ElseBranch);
        }

        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }
        return null;
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        object callee = Evaluate(expr.Callee);

        List<object> arguments = [];
        foreach (var argument in expr.Arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if (callee is not ICallable)
        {
            throw new RuntimeException(expr.Paren, "Can only call functions and classes");
        }

        ICallable function = (ICallable)callee;
        if (arguments.Count != function.Arity())
        {
            throw new RuntimeException(expr.Paren, $"Expected {function.Arity()} argument but got {arguments.Count}.");
        }
        return function.Call(this, arguments);
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        Function function = new(stmt, Environment, false);
        Environment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        object? value = null;
        if (stmt.Value != null)
        {
            value = Evaluate(stmt.Value);
        }
        throw new Return(value);
    }

    private static void CheckNumberOperand(Token @operator, object operand)
    {
        if (operand is double) return;
        throw new RuntimeException(@operator, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token @operator, object left, object right)
    {
        if (left is double && right is double) return;
        throw new RuntimeException(@operator, "Operands must be a numbers.");
    }

    private static bool IsTruthy(object? obj)
    {
        return obj switch
        {
            null => false,
            bool => (bool)obj,
            _ => true
        };
    }

    private static bool IsEqual(object a, object b)
    {
        if (a == null && b == null)
        {
            return true;
        }
        if (a == null)
        {
            return false;
        }

        return a.Equals(b);
    }
}