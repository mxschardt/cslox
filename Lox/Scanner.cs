using static Lox.TokenType;

namespace Lox;

class Scanner
{
    private readonly string Source;
    private readonly List<Token> Tokens = [];
    private int Start = 0;
    private int Current = 0;
    private int Line = 1;
    private static readonly Dictionary<string, TokenType> Keywords = [];

    static Scanner()
    {
        Keywords.Add("and",    AND);
        Keywords.Add("class",  CLASS);
        Keywords.Add("else",   ELSE);
        Keywords.Add("false",  FALSE);
        Keywords.Add("for",    FOR);
        Keywords.Add("fun",    FUN);
        Keywords.Add("if",     IF);
        Keywords.Add("nil",    NIL);
        Keywords.Add("or",     OR);
        Keywords.Add("print",  PRINT);
        Keywords.Add("return", RETURN);
        Keywords.Add("super",  SUPER);
        Keywords.Add("this",   THIS);
        Keywords.Add("true",   TRUE);
        Keywords.Add("var",    VAR);
        Keywords.Add("while",  WHILE);
    }

    public Scanner(string source)
    {
        Source = source;
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            Start = Current;
            ScanToken();
        }

        Tokens.Add(new Token(EOF, "", null, Line));
        return Tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(': AddToken(LEFT_PAREN); break;
            case ')': AddToken(RIGHT_PAREN); break;
            case '{': AddToken(LEFT_BRACE); break;
            case '}': AddToken(RIGHT_BRACE); break;
            case ',': AddToken(COMMA); break;
            case '.': AddToken(DOT); break;
            case '-': AddToken(MINUS); break;
            case '+': AddToken(PLUS); break;
            case ';': AddToken(SEMICOLON); break;
            case '*': AddToken(STAR); break;

            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;
            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;
            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;

            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd())
                    {
                        Advance();
                    }
                }
                else
                {
                    AddToken(SLASH);
                }
                break;

            case '"':
                String();
                break;

            case ' ':
            case '\r':
            case '\t':
                break;

            case '\n':
                Line += 1;
                break;

            default:
                if (IsDigit(c))
                {
                    Number();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    Lox.Error(Line, "Unexpected character.");
                }
                break;
        }
    }

    // The `Advance` method consumes the next character in the source file and returns it.
    private char Advance()
    {
        return Source[Current++];
    }

    // The `Peek` method returns next character in the source file.
    private char Peek()
    {
        if (IsAtEnd())
        {
            return '\0';
        }
        return Source[Current];
    }

    private char PeekNext()
    {
        if (Current + 1 >= Source.Length)
        {
            return '\0';
        }
        return Source[Current + 1];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd())
        {
            return false;
        }
        if (Source[Current] != expected)
        {
            return false;
        }

        Current += 1;
        return true;
    }

    //  `AddToken` grabs the text of the current lexeme and creates a new token for it.
    private void AddToken(TokenType type, object? literal)
    {
        var text = Source[Start..Current];
        Tokens.Add(new Token(type, text, literal, Line));
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private bool IsAtEnd()
    {
        return Current >= Source.Length;
    }

    private static bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
                c == '_';
    }

    private static bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    private static bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
            {
                Line += 1;
            }
            Advance();
        }

        if (IsAtEnd())
        {
            Lox.Error(Line, "Unterminated string.");
        }

        // Consume closing ".
        Advance();

        string value = Source[(Start + 1)..(Current - 1)];
        AddToken(STRING, value);
    }

    private void Number()
    {
        while (IsDigit(Peek()))
        {
            Advance();
        }

        // Look for a fractions part.
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            // Consume the ".".
            Advance();

            while (IsDigit(Peek()))
            {
                Advance();
            }
        }

        AddToken(NUMBER, Double.Parse(Source[Start..Current]));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek()))
        {
            Advance();
        }

        var text = Source[Start..Current];
        if (Keywords.TryGetValue(text, out TokenType type))
        {
            AddToken(type);
            return;
        }
        AddToken(IDENTIFIER);
    }
}
