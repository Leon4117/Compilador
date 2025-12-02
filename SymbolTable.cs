using System;
using System.Collections.Generic;

namespace Compilador
{
    public class SymbolTable
    {
        private class Scope
        {
            public Dictionary<string, Symbol> Symbols { get; } = new Dictionary<string, Symbol>();
            public int Level { get; }

            public Scope(int level)
            {
                Level = level;
            }
        }

        private Stack<Scope> _scopes = new Stack<Scope>();
        private int _currentLevel = 0;

        public SymbolTable()
        {
            EnterScope();
        }

        public void EnterScope()
        {
            _scopes.Push(new Scope(_currentLevel++));
        }

        public void ExitScope()
        {
            if (_scopes.Count > 1)
            {
                _scopes.Pop();
                _currentLevel--;
            }
        }

        public void Declare(string name, Type type, int line, int column)
        {
            Scope currentScope = _scopes.Peek();
            
            if (currentScope.Symbols.ContainsKey(name))
            {
                Symbol existing = currentScope.Symbols[name];
                throw new SemanticError(
                    $"Variable '{name}' already declared in this scope at [{existing.DeclarationLine}:{existing.DeclarationColumn}]",
                    line, column);
            }

            Symbol symbol = new Symbol(name, type, currentScope.Level, line, column);
            currentScope.Symbols[name] = symbol;
        }

        public Symbol? Lookup(string name)
        {
            foreach (Scope scope in _scopes)
            {
                if (scope.Symbols.ContainsKey(name))
                {
                    return scope.Symbols[name];
                }
            }
            return null;
        }

        public bool ExistsInCurrentScope(string name)
        {
            return _scopes.Peek().Symbols.ContainsKey(name);
        }

        public List<Symbol> GetAllSymbols()
        {
            List<Symbol> allSymbols = new List<Symbol>();
            foreach (Scope scope in _scopes)
            {
                allSymbols.AddRange(scope.Symbols.Values);
            }
            return allSymbols;
        }

        public int CurrentScopeLevel => _scopes.Peek().Level;
    }

    public class SemanticError : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public SemanticError(string message, int line, int column) : base(message)
        {
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"[{Line}:{Column}] Semantic Error: {Message}";
        }
    }
}
