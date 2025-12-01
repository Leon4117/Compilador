using System;
using System.Collections.Generic;

namespace Compilador
{
    public class Parser
    {
        private class ParseError : Exception { }

        private readonly List<Token> _tokens;
        private int _current = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                Stmt? decl = Declaration();
                if (decl != null) statements.Add(decl);
            }
            return statements;
        }

        private Stmt? Declaration()
        {
            try
            {
                if (Match(TokenType.VAR, TokenType.INT, TokenType.BOOL, TokenType.STRING_TYPE, TokenType.FLOAT)) 
                {
                    Token typeToken = Previous();
                    return VarDeclaration(typeToken);
                }

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt VarDeclaration(Token typeToken)
        {
            int? arraySize = null;
            if (Match(TokenType.LEFT_BRACKET))
            {
                Token sizeToken = Consume(TokenType.NUMBER, "Expect array size.");
                arraySize = Convert.ToInt32(sizeToken.Literal);
                Consume(TokenType.RIGHT_BRACKET, "Expect ']' after array size.");
            }

            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr? initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, typeToken, initializer, arraySize);
        }

        private Stmt Statement()
        {
            if (Match(TokenType.FOR)) return ForStatement();
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.DO)) return DoStatement();
            if (Match(TokenType.BREAK)) return BreakStatement();
            if (Match(TokenType.CONTINUE)) return ContinueStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt? elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after while condition.");
            Stmt body = Statement();

            return new Stmt.While(condition, body);
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                Stmt? decl = Declaration();
                if (decl != null) statements.Add(decl);
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt? initializer;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR, TokenType.INT, TokenType.BOOL, TokenType.STRING_TYPE))
            {
                Token typeToken = Previous();
                initializer = VarDeclaration(typeToken);
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr? condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr? increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            Stmt body = Statement();

            return new Stmt.For(initializer, condition, increment, body);
        }

        private Stmt DoStatement()
        {
            Stmt body = Statement();
            Consume(TokenType.WHILE, "Expect 'while' after do-loop body.");
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            Consume(TokenType.SEMICOLON, "Expect ';' after do-while condition.");

            return new Stmt.Do(body, condition);
        }

        private Stmt BreakStatement()
        {
            Token keyword = Previous();
            Consume(TokenType.SEMICOLON, "Expect ';' after 'break'.");
            return new Stmt.Break(keyword);
        }

        private Stmt ContinueStatement()
        {
            Token keyword = Previous();
            Consume(TokenType.SEMICOLON, "Expect ';' after 'continue'.");
            return new Stmt.Continue(keyword);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Expr Assignment()
        {
            Expr expr = Or();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Expr.Variable v)
                {
                    Token name = v.Name;
                    return new Expr.Assign(name, value);
                }
                else if (expr is Expr.ArrayAccess a)
                {
                    return new Expr.ArrayAssign(a.Name, a.Index, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(TokenType.OR))
            {
                Token op = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(TokenType.AND))
            {
                Token op = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token op = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }

            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_BRACKET))
                {
                    Expr index = Expression();
                    Consume(TokenType.RIGHT_BRACKET, "Expect ']' after index.");
                    
                    if (expr is Expr.Variable v)
                    {
                        expr = new Expr.ArrayAccess(v.Name, index);
                    }
                    else if (expr is Expr.ArrayAccess)
                    {
                        throw Error(Previous(), "Multidimensional arrays not supported.");
                    }
                    else
                    {
                        throw Error(Previous(), "Expect variable before array index.");
                    }
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if (Match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(Previous());
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
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
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private ParseError Error(Token token, string message)
        {
            Console.Error.WriteLine($"[line {token.Line}] Error at '{token.Lexeme}': {message}");
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.SEMICOLON) return;

                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }
    }
}
