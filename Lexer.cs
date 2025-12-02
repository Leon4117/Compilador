using System;
using System.Collections.Generic;

namespace Compilador
{
    public class Lexer
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;
        private int _column = 1;
        private int _startColumn = 1;

        private static readonly Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
        {
            { "and",    TokenType.AND },
            { "class",  TokenType.CLASS },
            { "else",   TokenType.ELSE },
            { "false",  TokenType.FALSE },
            { "for",    TokenType.FOR },
            { "if",     TokenType.IF },
            { "nil",    TokenType.NIL },
            { "or",     TokenType.OR },
            { "print",  TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super",  TokenType.SUPER },
            { "this",   TokenType.THIS },
            { "true",   TokenType.TRUE },
            { "var",    TokenType.VAR },
            { "while",  TokenType.WHILE },
            { "do",     TokenType.DO },
            { "int",    TokenType.INT },
            { "bool",   TokenType.BOOL },
            { "string", TokenType.STRING_TYPE },
            { "float",  TokenType.FLOAT },
            { "break",  TokenType.BREAK },
            { "continue", TokenType.CONTINUE },
            { "switch", TokenType.SWITCH },
            { "case",   TokenType.CASE },
            { "default", TokenType.DEFAULT }
        };

        public Lexer(string source)
        {
            _source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                _startColumn = _column;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line, _column));
            return _tokens;
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
                case '[': AddToken(TokenType.LEFT_BRACKET); break;
                case ']': AddToken(TokenType.RIGHT_BRACKET); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case ':': AddToken(TokenType.COLON); break;
                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    break;

                case '\n':
                    break;

                case '"': String(); break;

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
                        Console.Error.WriteLine($"[line {_line}] Error: Unexpected character: {c}");
                    }
                    break;
            }
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = _source.Substring(_start, _current - _start);
            if (!_keywords.TryGetValue(text, out TokenType type))
            {
                type = TokenType.IDENTIFIER;
            }
            AddToken(type);
        }

        private void Number()
        {
            if (Peek() == '0' && (PeekNext() == 'x' || PeekNext() == 'X'))
            {
                Advance();
                Advance();
                
                while (IsHexDigit(Peek())) Advance();
                
                string hexStr = _source.Substring(_start + 2, _current - _start - 2);
                double value = Convert.ToInt32(hexStr, 16);
                AddToken(TokenType.NUMBER, value);
                return;
            }

            while (IsDigit(Peek())) Advance();

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            if (Peek() == 'e' || Peek() == 'E')
            {
                Advance();
                
                if (Peek() == '+' || Peek() == '-')
                {
                    Advance();
                }
                
                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.NUMBER, double.Parse(_source.Substring(_start, _current - _start)));
        }

        private bool IsHexDigit(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'f') ||
                   (c >= 'A' && c <= 'F');
        }

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                Advance();
            }

            if (IsAtEnd())
            {
                Console.Error.WriteLine($"[line {_line}] Error: Unterminated string.");
                return;
            }

            Advance();

            string value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenType.STRING, value);
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            Advance();
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        private char Advance()
        {
            char c = _source[_current++];
            if (c == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }
            return c;
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object? literal)
        {
            string text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line, _startColumn));
        }
    }
}
