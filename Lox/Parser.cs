using static Lox.TokenType;

namespace Lox;

// program        → declaration* EOF ;
// declaration    → varDecl
//                | funcDecl
//                | statement ;
// statement      → exprStmt
//                | ifStmt
//                | printStmt
//                | whileStmt
//                | forStmt
//                | returnStmt
//                | block ;
// ifStmt         → "if" "(" expression ")" statement
//                  ( "else" statement )? ;
// forStmt        → "for" "(" ( varDecl | exprStmt | ";" )
//                  expression? ";"
//                  expression? ")" statement ;
// whileStmt      → "while" "(" expression ")" statement ;
// varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;
// funcDecl       → "fun" function ;
// returnStmt     → "return" expression? ";" ;
// function       → IDENTIFIER "(" parameters? ")" block ;
// parameters     → IDENTIFIER ( "," IDENTIFIER )* ;
// block          → "{" declaration* "}"
//
// expression     → assignment ;
// assignment     → IDENTIFIER "=" assignment
//                | ternary
//                | logic_or ;
// logic_or       → logic_and ( "or" logic_and)* ;
// logic_and      → ternary ( "and" ternary )* ;
// ternary        → equality ("?" equality ":" equality)* ;
// equality       → comparison ( ( "!=" | "==" ) comparison )* ;
// comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
// term           → factor ( ( "-" | "+" ) factor )* ;
// factor         → unary ( ( "/" | "*" ) unary )* ;
// unary          → ( "!" | "-" ) unary
//                | call ;
// call           → primary ( "(" arguments? ")" )* ;
// arguments      → expression ( "," expression )* ;
// primary        → "true" | "false" | "nil"
//                | NUMBER | STRING
//                | "(" expression ")"
//                | IDENTIFIER ;

class Parser
{
    private class ParseException : Exception;
    private readonly List<Token> Tokens;
    private int Current = 0;

    public Parser(List<Token> tokens)
    {
        Tokens = tokens;
    }

    public List<Stmt>? Parse()
    {
        List<Stmt> statements = [];
        while (!IsAtEnd())
        {
            var value = Declaration();
            // TODO: handle null value
            statements.Add(value);
        }
        return statements;
    }

    private Stmt? Declaration()
    {
        try
        {
            if (Match(FUN))
            {
                return Function("function");
            }
            if (Match(VAR))
            {
                return VarDeclaration();
            }

            return Statement();
        }
        catch (ParseException)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt Function(string kind)
    {
        Token name = Consume(IDENTIFIER, $"Expect {kind} name.");
        Consume(LEFT_PAREN, $"Expect '(' after {kind} name.");
        List<Token> parameters = [];
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
            } while (Match(COMMA));
        }
        Consume(RIGHT_PAREN, "Expect ')' after parameters.");

        Consume(LEFT_BRACE, $"Expect '{{' before {kind} body.");
        List<Stmt> body = Block();
        return new Stmt.Function(name, parameters, body);
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(IDENTIFIER, "Expect variable name.");

        Expr? initializer = null;
        if (Match(EQUAL))
        {
            initializer = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after variable declaration");
        return new Stmt.Var(name, initializer);
    }

    private Stmt Statement()
    {
        if (Match(PRINT))
        {
            return PrintStatement();
        }

        if (Match(RETURN))
        {
            return ReturnStatement();
        }

        if (Match(IF))
        {
            return IfStatement();
        }

        if (Match(WHILE))
        {
            return WhileStatement();
        }

        if (Match(FOR))
        {
            return ForStatement();
        }

        if (Match(LEFT_BRACE))
        {
            return new Stmt.Block(Block());
        }

        return ExpressionStatement();
    }

    private Stmt ReturnStatement()
    {
        Token keyword = Previous();
        Expr? value = null;
        if (!Check(SEMICOLON))
        {
            value = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after return value");
        return new Stmt.Return(keyword, value);
    }

    private Stmt ForStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'for'.");

        Stmt? initializer;
        if (Match(SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(VAR))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(SEMICOLON))
        {
            condition = Expression();
        }
        Consume(SEMICOLON, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!Check(RIGHT_PAREN))
        {
            increment = Expression();
        }
        Consume(RIGHT_PAREN, "Expect ')' after for clauses.");

        Stmt body = Statement();

        if (increment != null)
        {
            body = new Stmt.Block([body, new Stmt.Expression(increment)]);
        }

        condition ??= new Expr.Literal(true);
        body = new Stmt.While(condition, body);

        if (initializer != null)
        {
            body = new Stmt.Block([initializer, body]);
        }

        return body;
    }

    private Stmt WhileStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'while'.");
        Expr condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after condition.");
        Stmt body = Statement();

        return new Stmt.While(condition, body);
    }

    private Stmt IfStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'if'.");
        Expr condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after if condition.");

        Stmt thenBranch = Statement();
        Stmt? elseBranch = null;
        if (Match(ELSE))
        {
            elseBranch = Statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = [];

        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Stmt PrintStatement()
    {
        Expr value = Expression();
        Consume(SEMICOLON, "Expect ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();
        Consume(SEMICOLON, "Expect ';' after expression");
        return new Stmt.Expression(expr);
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        Expr expr = Or();

        if (Match(EQUAL))
        {
            Token equals = Previous();
            Expr value = Assignment();

            if (expr is Expr.Variable variable)
            {
                Token name = variable.Name;
                return new Expr.Assign(name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        Expr expr = And();

        while (Match(OR))
        {
            Token @operator = Previous();
            Expr right = And();
            expr = new Expr.Logical(expr, @operator, right);
        }

        return expr;
    }

    private Expr And()
    {
        Expr expr = Ternary();

        while (Match(AND))
        {
            Token @operator = Previous();
            Expr right = Ternary();
            expr = new Expr.Logical(expr, @operator, right);
        }

        return expr;
    }

    private Expr Ternary()
    {
        Expr expr = Equality();

        while (Match(QUESTION_MARK))
        {
            Expr right = Equality();
            Token oper = Previous();
            Consume(COLON, "Expect ':' in ternary.");
            Expr left = Equality();
            expr = new Expr.Ternary(expr, oper, right, left);
        }

        return expr;
    }

    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            Token oper = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            Token oper = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (Match(MINUS, PLUS))
        {
            Token oper = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (Match(SLASH, STAR))
        {
            Token oper = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(BANG, MINUS))
        {
            Token oper = Previous();
            Expr right = Unary();
            return new Expr.Unary(oper, right);
        }

        return Call();
    }

    private Expr Call()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = [];
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments");
                }
                arguments.Add(Expression());
            } while (Match(COMMA));
        }

        Token paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");

        return new Expr.Call(callee, paren, arguments);
    }

    // Deprecated
    private Expr Comma()
    {
        Expr expr = Primary();

        while (Match(COMMA))
        {
            Expr right = Comma();
            Token oper = Previous();
            expr = new Expr.Comma(expr, oper, right);
        }

        return expr;
    }

    private Expr Primary()
    {
        if (Match(FALSE)) return new Expr.Literal(false);
        if (Match(TRUE)) return new Expr.Literal(true);
        if (Match(NIL)) return new Expr.Literal(null);

        if (Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous().Literal);
        }

        if (Match(IDENTIFIER))
        {
            return new Expr.Variable(Previous());
        }

        if (Match(LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expect expression");
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            return Advance();
        }

        throw Error(Peek(), message);
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd())
        {
            return false;
        }
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            Current += 1;
        }
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == EOF;
    }

    private Token Peek()
    {
        return Tokens[Current];
    }

    private Token Previous()
    {
        return Tokens[Current - 1];
    }

    private static ParseException Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseException();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == SEMICOLON) return;

            switch (Peek().Type)
            {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            Advance();
        }
    }
}