using System.Collections.Generic;

namespace Compilador
{
    public abstract class Expr
    {
        public interface IVisitor<R>
        {
            R VisitBinaryExpr(Binary expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            R VisitUnaryExpr(Unary expr);
            R VisitVariableExpr(Variable expr);
            R VisitAssignExpr(Assign expr);
            R VisitLogicalExpr(Logical expr);
            R VisitArrayAccessExpr(ArrayAccess expr);
            R VisitArrayAssignExpr(ArrayAssign expr);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

        public class ArrayAccess : Expr
        {
            public Token Name { get; }
            public Expr Index { get; }

            public ArrayAccess(Token name, Expr index)
            {
                Name = name;
                Index = index;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitArrayAccessExpr(this);
            }
        }

        public class ArrayAssign : Expr
        {
            public Token Name { get; }
            public Expr Index { get; }
            public Expr Value { get; }

            public ArrayAssign(Token name, Expr index, Expr value)
            {
                Name = name;
                Index = index;
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitArrayAssignExpr(this);
            }
        }

        public class Binary : Expr
        {
            public Expr Left { get; }
            public Token Operator { get; }
            public Expr Right { get; }

            public Binary(Expr left, Token op, Expr right)
            {
                Left = left;
                Operator = op;
                Right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }
        }

        public class Grouping : Expr
        {
            public Expr Expression { get; }

            public Grouping(Expr expression)
            {
                Expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }
        }

        public class Literal : Expr
        {
            public object? Value { get; }

            public Literal(object? value)
            {
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }
        }

        public class Unary : Expr
        {
            public Token Operator { get; }
            public Expr Right { get; }

            public Unary(Token op, Expr right)
            {
                Operator = op;
                Right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }
        }

        public class Variable : Expr
        {
            public Token Name { get; }

            public Variable(Token name)
            {
                Name = name;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }
        }

        public class Assign : Expr
        {
            public Token Name { get; }
            public Expr Value { get; }

            public Assign(Token name, Expr value)
            {
                Name = name;
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }
        }

        public class Logical : Expr
        {
            public Expr Left { get; }
            public Token Operator { get; }
            public Expr Right { get; }

            public Logical(Expr left, Token op, Expr right)
            {
                Left = left;
                Operator = op;
                Right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }
        }
    }
}
