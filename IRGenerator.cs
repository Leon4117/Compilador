using System;
using System.Collections.Generic;

namespace Compilador
{
    public class IRGenerator : Expr.IVisitor<string>, Stmt.IVisitor<object?>
    {
        public ThreeAddressCode Code { get; } = new ThreeAddressCode();
        private int _tempCount = 0;
        private int _labelCount = 0;

        private Stack<string> _breakLabels = new Stack<string>();
        private Stack<string> _continueLabels = new Stack<string>();

        private string NewTemp()
        {
            return $"t{++_tempCount}";
        }

        private string NewLabel()
        {
            return $"L{++_labelCount}";
        }

        public void Generate(List<Stmt> statements)
        {
            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private string Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        public object? VisitBlockStmt(Stmt.Block stmt)
        {
            foreach (var statement in stmt.Statements)
            {
                Execute(statement);
            }
            return null;
        }

        public object? VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.Expr);
            return null;
        }

        public object? VisitIfStmt(Stmt.If stmt)
        {
            string elseLabel = NewLabel();
            string endLabel = NewLabel();

            string condition = Evaluate(stmt.Condition);
            Code.Emit(OpCode.IF_FALSE, condition, null, elseLabel);

            Execute(stmt.ThenBranch);
            Code.Emit(OpCode.GOTO, null, null, endLabel);

            Code.Emit(OpCode.LABEL, null, null, elseLabel);
            if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }

            Code.Emit(OpCode.LABEL, null, null, endLabel);
            return null;
        }

        public object? VisitPrintStmt(Stmt.Print stmt)
        {
            string value = Evaluate(stmt.Expr);
            Code.Emit(OpCode.PRINT, value);
            return null;
        }

        public object? VisitVarStmt(Stmt.Var stmt)
        {
            if (stmt.Initializer != null)
            {
                string value = Evaluate(stmt.Initializer);
                Code.Emit(OpCode.ASSIGN, value, null, stmt.Name.Lexeme);
            }
            else if (stmt.ArraySize != null)
            {
                Code.Emit(OpCode.NEW_ARRAY, stmt.ArraySize.Value.ToString(), null, stmt.Name.Lexeme);
            }
            return null;
        }

        public object? VisitWhileStmt(Stmt.While stmt)
        {
            string startLabel = NewLabel();
            string endLabel = NewLabel();

            _continueLabels.Push(startLabel);
            _breakLabels.Push(endLabel);

            Code.Emit(OpCode.LABEL, null, null, startLabel);
            
            string condition = Evaluate(stmt.Condition);
            Code.Emit(OpCode.IF_FALSE, condition, null, endLabel);

            Execute(stmt.Body);
            Code.Emit(OpCode.GOTO, null, null, startLabel);

            Code.Emit(OpCode.LABEL, null, null, endLabel);

            _breakLabels.Pop();
            _continueLabels.Pop();
            return null;
        }

        public object? VisitForStmt(Stmt.For stmt)
        {
            string startLabel = NewLabel();
            string incrementLabel = NewLabel();
            string endLabel = NewLabel();

            if (stmt.Initializer != null)
            {
                Execute(stmt.Initializer);
            }

            Code.Emit(OpCode.LABEL, null, null, startLabel);

            if (stmt.Condition != null)
            {
                string condition = Evaluate(stmt.Condition);
                Code.Emit(OpCode.IF_FALSE, condition, null, endLabel);
            }

            _continueLabels.Push(incrementLabel);
            _breakLabels.Push(endLabel);

            Execute(stmt.Body);

            Code.Emit(OpCode.LABEL, null, null, incrementLabel);
            if (stmt.Increment != null)
            {
                Evaluate(stmt.Increment);
            }
            Code.Emit(OpCode.GOTO, null, null, startLabel);

            Code.Emit(OpCode.LABEL, null, null, endLabel);

            _breakLabels.Pop();
            _continueLabels.Pop();
            return null;
        }

        public object? VisitBreakStmt(Stmt.Break stmt)
        {
            if (_breakLabels.Count > 0)
            {
                Code.Emit(OpCode.GOTO, null, null, _breakLabels.Peek());
            }
            return null;
        }

        public object? VisitContinueStmt(Stmt.Continue stmt)
        {
            if (_continueLabels.Count > 0)
            {
                Code.Emit(OpCode.GOTO, null, null, _continueLabels.Peek());
            }
            return null;
        }

        public object? VisitDoStmt(Stmt.Do stmt)
        {
            string startLabel = NewLabel();
            string continueLabel = NewLabel();
            string endLabel = NewLabel();

            _continueLabels.Push(continueLabel);
            _breakLabels.Push(endLabel);

            Code.Emit(OpCode.LABEL, null, null, startLabel);

            Execute(stmt.Body);

            Code.Emit(OpCode.LABEL, null, null, continueLabel);
            string condition = Evaluate(stmt.Condition);
            
            Code.Emit(OpCode.IF_FALSE, condition, null, endLabel);
            Code.Emit(OpCode.GOTO, null, null, startLabel);

            Code.Emit(OpCode.LABEL, null, null, endLabel);

            _breakLabels.Pop();
            _continueLabels.Pop();
            return null;
        }

        public string VisitArrayAccessExpr(Expr.ArrayAccess expr)
        {
            string index = Evaluate(expr.Index);
            string temp = NewTemp();
            Code.Emit(OpCode.ARRAY_LOAD, expr.Name.Lexeme, index, temp);
            return temp;
        }

        public string VisitArrayAssignExpr(Expr.ArrayAssign expr)
        {
            string index = Evaluate(expr.Index);
            string value = Evaluate(expr.Value);
            Code.Emit(OpCode.ARRAY_STORE, index, value, expr.Name.Lexeme);
            return value;
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            string value = Evaluate(expr.Value);
            Code.Emit(OpCode.ASSIGN, value, null, expr.Name.Lexeme);
            return expr.Name.Lexeme;
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            string left = Evaluate(expr.Left);
            string right = Evaluate(expr.Right);
            string temp = NewTemp();

            OpCode op = OpCode.ADD;
            switch (expr.Operator.Type)
            {
                case TokenType.PLUS: op = OpCode.ADD; break;
                case TokenType.MINUS: op = OpCode.SUB; break;
                case TokenType.STAR: op = OpCode.MUL; break;
                case TokenType.SLASH: op = OpCode.DIV; break;
                case TokenType.GREATER: op = OpCode.GT; break;
                case TokenType.GREATER_EQUAL: op = OpCode.GTE; break;
                case TokenType.LESS: op = OpCode.LT; break;
                case TokenType.LESS_EQUAL: op = OpCode.LTE; break;
                case TokenType.EQUAL_EQUAL: op = OpCode.EQ; break;
                case TokenType.BANG_EQUAL: op = OpCode.NEQ; break;
            }

            Code.Emit(op, left, right, temp);
            return temp;
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null) return "nil";
            if (expr.Value is string) return $"\"{expr.Value}\"";
            if (expr.Value is bool) return ((bool)expr.Value) ? "true" : "false";
            return expr.Value?.ToString() ?? "nil";
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            string temp = NewTemp();
            string left = Evaluate(expr.Left);
            
            string endLabel = NewLabel();
            string rightLabel = NewLabel();

            Code.Emit(OpCode.ASSIGN, left, null, temp);

            if (expr.Operator.Type == TokenType.OR)
            {
                Code.Emit(OpCode.IF_FALSE, left, null, rightLabel);
                Code.Emit(OpCode.GOTO, null, null, endLabel);
            }
            else
            {
                Code.Emit(OpCode.IF_FALSE, left, null, endLabel);
            }

            Code.Emit(OpCode.LABEL, null, null, rightLabel);
            string right = Evaluate(expr.Right);
            Code.Emit(OpCode.ASSIGN, right, null, temp);

            Code.Emit(OpCode.LABEL, null, null, endLabel);
            return temp;
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            string right = Evaluate(expr.Right);
            string temp = NewTemp();

            if (expr.Operator.Type == TokenType.MINUS)
            {
                Code.Emit(OpCode.SUB, "0", right, temp);
            }
            else if (expr.Operator.Type == TokenType.BANG)
            {
                Code.Emit(OpCode.NOT, right, null, temp);
            }

            return temp;
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            return expr.Name.Lexeme;
        }
    }
}
