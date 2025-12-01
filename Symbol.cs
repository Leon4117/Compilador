namespace Compilador
{
    public class Symbol
    {
        public string Name { get; }
        public Type Type { get; }
        public int ScopeLevel { get; }
        public int DeclarationLine { get; }
        public int DeclarationColumn { get; }
        public bool IsInitialized { get; set; }

        public Symbol(string name, Type type, int scopeLevel, int line, int column)
        {
            Name = name;
            Type = type;
            ScopeLevel = scopeLevel;
            DeclarationLine = line;
            DeclarationColumn = column;
            IsInitialized = false;
        }

        public override string ToString()
        {
            return $"{Name}: {Type} (scope {ScopeLevel}) at [{DeclarationLine}:{DeclarationColumn}]";
        }
    }
}
