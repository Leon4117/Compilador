using System.Collections.Generic;

namespace Compilador
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitBlockStmt(Block stmt);
            R VisitExpressionStmt(Expression stmt);
            R VisitIfStmt(If stmt);
            R VisitPrintStmt(Print stmt);
            R VisitVarStmt(Var stmt);
            R VisitWhileStmt(While stmt);
            R VisitForStmt(For stmt);
            R VisitBreakStmt(Break stmt);
            R VisitContinueStmt(Continue stmt);
            R VisitDoStmt(Do stmt);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

        public class Do : Stmt
        {
            public Stmt Body { get; }
            public Expr Condition { get; }

            public Do(Stmt body, Expr condition)
            {
                Body = body;
                Condition = condition;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitDoStmt(this);
            }
        }

        public class Block : Stmt
        {
            public List<Stmt> Statements { get; }

            public Block(List<Stmt> statements)
            {
                Statements = statements;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }
        }

        public class Expression : Stmt
        {
            public Expr Expr { get; }

            public Expression(Expr expr)
            {
                Expr = expr;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }

        public class If : Stmt
        {
            public Expr Condition { get; }
            public Stmt ThenBranch { get; }
            public Stmt? ElseBranch { get; }

            public If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
            {
                Condition = condition;
                ThenBranch = thenBranch;
                ElseBranch = elseBranch;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
            }
        }

        public class Print : Stmt
        {
            public Expr Expr { get; }

            public Print(Expr expr)
            {
                Expr = expr;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }

        public class Var : Stmt
        {
            public Token Name { get; }
            public Token Type { get; }
            public Expr? Initializer { get; }
            public int? ArraySize { get; }

            public Var(Token name, Token type, Expr? initializer, int? arraySize = null)
            {
                Name = name;
                Type = type;
                Initializer = initializer;
                ArraySize = arraySize;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }
        }

        public class While : Stmt
        {
            public Expr Condition { get; }
            public Stmt Body { get; }

            public While(Expr condition, Stmt body)
            {
                Condition = condition;
                Body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }
        }

        public class For : Stmt
        {
            public Stmt? Initializer { get; }
            public Expr? Condition { get; }
            public Expr? Increment { get; }
            public Stmt Body { get; }

            public For(Stmt? initializer, Expr? condition, Expr? increment, Stmt body)
            {
                Initializer = initializer;
                Condition = condition;
                Increment = increment;
                Body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitForStmt(this);
            }
        }

        public class Break : Stmt
        {
            public Token Keyword { get; }

            public Break(Token keyword)
            {
                Keyword = keyword;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBreakStmt(this);
            }
        }

        public class Continue : Stmt
        {
            public Token Keyword { get; }

            public Continue(Token keyword)
            {
                Keyword = keyword;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitContinueStmt(this);
            }
        }
    }
}
