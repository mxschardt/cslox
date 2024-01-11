namespace Lox;

class Scanner
{
    private readonly string Source;
    private readonly List<Token> Tokens = [];
    private int Start = 0;
    private int Current = 0;
    private int Line = 1;

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

        Tokens.Add(new Token(TokenType.EOF, "", null, Line));
        return Tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '.': AddToken(TokenType.DOT); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '+': AddToken(TokenType.PLUS); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '*': AddToken(TokenType.STAR); break;

            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
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
                    AddToken(TokenType.SLASH);
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
                Lox.Error(Line, "Unexpected character.");
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
        AddToken(TokenType.STRING, value);
    }
}
