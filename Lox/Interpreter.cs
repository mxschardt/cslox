using System.Diagnostics;
using static Lox.TokenType;

namespace Lox;

class Interpreter : Expr.IVisitor<object>
{
    internal void Interpret(Expr expression)
    {
        try
        {
            object value = Evaluate(expression);
            Console.WriteLine(Stringify(value));
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
            // TODO
        }
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
                return (double)left / (double)right;
            case STAR:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left * (double)right;
            case PLUS:
                if (left is double && right is double)
                {
                    return (double)left + (double)right;
                }

                if (left is string && right is string)
                {
                    return (string)left + (string)right;
                }

                throw new RuntimeException(expr.Operator, "Operands must be two numbers or two strings.");

            case BANG_EQUAL:
                return !IsEqual(left, right);
            case EQUAL_EQUAL:
                return IsEqual(left, right);
        };

        throw new UnreachableException();
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

    public object VisitTernaryExpr(Expr.Ternary ternary)
    {
        throw new NotImplementedException();
    }

    public object VisitCommaExpr(Expr.Comma expr)
    {
        throw new NotImplementedException();
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
