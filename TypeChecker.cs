using System;
using System.Collections.Generic;

namespace Compilador
{
    public class TypeChecker : Expr.IVisitor<Type>, Stmt.IVisitor<Type>
    {
        private readonly SymbolTable _symbolTable = new SymbolTable();
        private bool _hadError = false;
        private int _loopDepth = 0;

        public bool HadError => _hadError;

        public void Check(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    CheckStmt(statement);
                }
            }
            catch (SemanticError error)
            {
                ReportError(error.Line, error.Column, error.Message);
            }
        }

        private void CheckStmt(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private Type CheckExpr(Expr expr)
        {
            return expr.Accept(this);
        }

        public Type VisitBlockStmt(Stmt.Block stmt)
        {
            _symbolTable.EnterScope();
            foreach (Stmt statement in stmt.Statements)
            {
                CheckStmt(statement);
            }
            _symbolTable.ExitScope();
            return VoidType.Instance;
        }

        public Type VisitExpressionStmt(Stmt.Expression stmt)
        {
            CheckExpr(stmt.Expr);
            return VoidType.Instance;
        }

        public Type VisitIfStmt(Stmt.If stmt)
        {
            Type conditionType = CheckExpr(stmt.Condition);
            if (!TypeHelper.IsBoolean(conditionType) && !(conditionType is ErrorType))
            {
                ReportError(0, 0, $"If condition must be boolean, got {conditionType}");
            }

            CheckStmt(stmt.ThenBranch);
            if (stmt.ElseBranch != null)
            {
                CheckStmt(stmt.ElseBranch);
            }

            return VoidType.Instance;
        }

        public Type VisitPrintStmt(Stmt.Print stmt)
        {
            CheckExpr(stmt.Expr);
            return VoidType.Instance;
        }

        public Type VisitVarStmt(Stmt.Var stmt)
        {
            Type varType = VoidType.Instance;

            if (stmt.Initializer != null)
            {
                varType = CheckExpr(stmt.Initializer);
            }
            else if (stmt.ArraySize != null)
            {
                Type elementType = TypeHelper.FromTokenType(stmt.Type.Type);
                varType = new ArrayType(elementType, stmt.ArraySize.Value);
            }
            else
            {
                varType = TypeHelper.FromTokenType(stmt.Type.Type);
            }

            try
            {
                _symbolTable.Declare(stmt.Name.Lexeme, varType, stmt.Name.Line, stmt.Name.Column);
            }
            catch (SemanticError error)
            {
                ReportError(error.Line, error.Column, error.Message);
            }

            return VoidType.Instance;
        }

        public Type VisitWhileStmt(Stmt.While stmt)
        {
            Type conditionType = CheckExpr(stmt.Condition);
            if (!TypeHelper.IsBoolean(conditionType) && !(conditionType is ErrorType))
            {
                ReportError(0, 0, $"While condition must be boolean, got {conditionType}");
            }

            _loopDepth++;
            CheckStmt(stmt.Body);
            _loopDepth--;
            return VoidType.Instance;
        }

        public Type VisitForStmt(Stmt.For stmt)
        {
            _symbolTable.EnterScope();

            if (stmt.Initializer != null)
            {
                CheckStmt(stmt.Initializer);
            }

            if (stmt.Condition != null)
            {
                Type conditionType = CheckExpr(stmt.Condition);
                if (!TypeHelper.IsBoolean(conditionType) && !(conditionType is ErrorType))
                {
                    ReportError(0, 0, $"For condition must be boolean, got {conditionType}");
                }
            }

            if (stmt.Increment != null)
            {
                CheckExpr(stmt.Increment);
            }

            _loopDepth++;
            CheckStmt(stmt.Body);
            _loopDepth--;

            _symbolTable.ExitScope();
            return VoidType.Instance;
        }

        public Type VisitBreakStmt(Stmt.Break stmt)
        {
            if (_loopDepth == 0)
            {
                ReportError(stmt.Keyword.Line, stmt.Keyword.Column, "Must be inside a loop to use 'break'.");
            }
            return VoidType.Instance;
        }

        public Type VisitContinueStmt(Stmt.Continue stmt)
        {
            if (_loopDepth == 0)
            {
                ReportError(stmt.Keyword.Line, stmt.Keyword.Column, "Must be inside a loop to use 'continue'.");
            }
            return VoidType.Instance;
        }

        public Type VisitAssignExpr(Expr.Assign expr)
        {
            Type valueType = CheckExpr(expr.Value);
            Symbol? symbol = _symbolTable.Lookup(expr.Name.Lexeme);

            if (symbol == null)
            {
                ReportError(expr.Name.Line, expr.Name.Column, 
                    $"Undefined variable '{expr.Name.Lexeme}'");
                return ErrorType.Instance;
            }

            if (!symbol.Type.IsCompatibleWith(valueType) && !(valueType is ErrorType))
            {
                ReportError(expr.Name.Line, expr.Name.Column,
                    $"Cannot assign {valueType} to variable of type {symbol.Type}");
                return ErrorType.Instance;
            }

            symbol.IsInitialized = true;
            return valueType;
        }

        public Type VisitBinaryExpr(Expr.Binary expr)
        {
            Type leftType = CheckExpr(expr.Left);
            Type rightType = CheckExpr(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.PLUS:
                    if (leftType is StringType || rightType is StringType)
                    {
                        return StringType.Instance;
                    }
                    if (TypeHelper.IsNumeric(leftType) && TypeHelper.IsNumeric(rightType))
                    {
                        return IntType.Instance;
                    }
                    if (!(leftType is ErrorType) && !(rightType is ErrorType))
                    {
                        ReportError(expr.Operator.Line, expr.Operator.Column,
                            $"Cannot add {leftType} and {rightType}");
                    }
                    return ErrorType.Instance;

                case TokenType.MINUS:
                case TokenType.STAR:
                case TokenType.SLASH:
                    if (!TypeHelper.IsNumeric(leftType) || !TypeHelper.IsNumeric(rightType))
                    {
                        if (!(leftType is ErrorType) && !(rightType is ErrorType))
                        {
                            ReportError(expr.Operator.Line, expr.Operator.Column,
                                $"Operator {expr.Operator.Lexeme} requires numeric operands, got {leftType} and {rightType}");
                        }
                        return ErrorType.Instance;
                    }
                    return IntType.Instance;

                case TokenType.GREATER:
                case TokenType.GREATER_EQUAL:
                case TokenType.LESS:
                case TokenType.LESS_EQUAL:
                    if (!TypeHelper.IsNumeric(leftType) || !TypeHelper.IsNumeric(rightType))
                    {
                        if (!(leftType is ErrorType) && !(rightType is ErrorType))
                        {
                            ReportError(expr.Operator.Line, expr.Operator.Column,
                                $"Comparison requires numeric operands, got {leftType} and {rightType}");
                        }
                        return ErrorType.Instance;
                    }
                    return BoolType.Instance;

                case TokenType.EQUAL_EQUAL:
                case TokenType.BANG_EQUAL:
                    return BoolType.Instance;

                default:
                    return ErrorType.Instance;
            }
        }

        public Type VisitGroupingExpr(Expr.Grouping expr)
        {
            return CheckExpr(expr.Expression);
        }

        public Type VisitLiteralExpr(Expr.Literal expr)
        {
            return TypeHelper.FromLiteral(expr.Value);
        }

        public Type VisitLogicalExpr(Expr.Logical expr)
        {
            Type leftType = CheckExpr(expr.Left);
            Type rightType = CheckExpr(expr.Right);

            if (!TypeHelper.IsBoolean(leftType) && !(leftType is ErrorType))
            {
                ReportError(expr.Operator.Line, expr.Operator.Column,
                    $"Logical operator requires boolean operands, got {leftType}");
            }

            if (!TypeHelper.IsBoolean(rightType) && !(rightType is ErrorType))
            {
                ReportError(expr.Operator.Line, expr.Operator.Column,
                    $"Logical operator requires boolean operands, got {rightType}");
            }

            return BoolType.Instance;
        }

        public Type VisitUnaryExpr(Expr.Unary expr)
        {
            Type rightType = CheckExpr(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.MINUS:
                    if (!TypeHelper.IsNumeric(rightType) && !(rightType is ErrorType))
                    {
                        ReportError(expr.Operator.Line, expr.Operator.Column,
                            $"Unary minus requires numeric operand, got {rightType}");
                        return ErrorType.Instance;
                    }
                    return IntType.Instance;

                case TokenType.BANG:
                    if (!TypeHelper.IsBoolean(rightType) && !(rightType is ErrorType))
                    {
                        ReportError(expr.Operator.Line, expr.Operator.Column,
                            $"Logical NOT requires boolean operand, got {rightType}");
                        return ErrorType.Instance;
                    }
                    return BoolType.Instance;

                default:
                    return ErrorType.Instance;
            }
        }

        public Type VisitVariableExpr(Expr.Variable expr)
        {
            Symbol? symbol = _symbolTable.Lookup(expr.Name.Lexeme);

            if (symbol == null)
            {
                ReportError(expr.Name.Line, expr.Name.Column,
                    $"Undefined variable '{expr.Name.Lexeme}'");
                return ErrorType.Instance;
            }

            return symbol.Type;
        }

        public Type VisitDoStmt(Stmt.Do stmt)
        {
            Type conditionType = CheckExpr(stmt.Condition);
            if (!TypeHelper.IsBoolean(conditionType) && !(conditionType is ErrorType))
            {
                ReportError(0, 0, $"Do-While condition must be boolean, got {conditionType}");
            }

            _loopDepth++;
            CheckStmt(stmt.Body);
            _loopDepth--;
            return VoidType.Instance;
        }

        public Type VisitArrayAccessExpr(Expr.ArrayAccess expr)
        {
            Symbol? symbol = _symbolTable.Lookup(expr.Name.Lexeme);
            if (symbol == null)
            {
                ReportError(expr.Name.Line, expr.Name.Column, $"Undefined variable '{expr.Name.Lexeme}'");
                return ErrorType.Instance;
            }

            if (!(symbol.Type is ArrayType arrayType))
            {
                ReportError(expr.Name.Line, expr.Name.Column, $"Variable '{expr.Name.Lexeme}' is not an array.");
                return ErrorType.Instance;
            }

            Type indexType = CheckExpr(expr.Index);
            if (!TypeHelper.IsNumeric(indexType) && !(indexType is ErrorType))
            {
                ReportError(expr.Name.Line, expr.Name.Column, $"Array index must be numeric, got {indexType}");
            }

            return arrayType.ElementType;
        }

        public Type VisitArrayAssignExpr(Expr.ArrayAssign expr)
        {
            Type valueType = CheckExpr(expr.Value);
            Symbol? symbol = _symbolTable.Lookup(expr.Name.Lexeme);

            if (symbol == null)
            {
                ReportError(expr.Name.Line, expr.Name.Column, $"Undefined variable '{expr.Name.Lexeme}'");
                return ErrorType.Instance;
            }

            if (!(symbol.Type is ArrayType arrayType))
            {
                ReportError(expr.Name.Line, expr.Name.Column, $"Variable '{expr.Name.Lexeme}' is not an array.");
                return ErrorType.Instance;
            }

            Type indexType = CheckExpr(expr.Index);
            if (!TypeHelper.IsNumeric(indexType) && !(indexType is ErrorType))
            {
                ReportError(expr.Name.Line, expr.Name.Column, $"Array index must be numeric, got {indexType}");
            }

            if (!arrayType.ElementType.IsCompatibleWith(valueType) && !(valueType is ErrorType))
            {
                ReportError(expr.Name.Line, expr.Name.Column, $"Cannot assign {valueType} to array of type {arrayType.ElementType}");
                return ErrorType.Instance;
            }

            return valueType;
        }

        private void ReportError(int line, int column, string message)
        {
            Console.Error.WriteLine($"[{line}:{column}] Type Error: {message}");
            _hadError = true;
        }
    }
}
