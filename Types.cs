using System;

namespace Compilador
{
    public abstract class Type
    {
        public abstract string Name { get; }

        public virtual bool IsCompatibleWith(Type other)
        {
            return this.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Type other)
            {
                return this.Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class IntType : Type
    {
        private static IntType? _instance;
        public static IntType Instance => _instance ?? (_instance = new IntType());

        private IntType() { }

        public override string Name => "int";
    }

    public class BoolType : Type
    {
        private static BoolType? _instance;
        public static BoolType Instance => _instance ?? (_instance = new BoolType());

        private BoolType() { }

        public override string Name => "bool";
    }

    public class StringType : Type
    {
        private static StringType? _instance;
        public static StringType Instance => _instance ?? (_instance = new StringType());

        private StringType() { }

        public override string Name => "string";

        public override bool IsCompatibleWith(Type other)
        {
            return true;
        }
    }

    public class VoidType : Type
    {
        private static VoidType? _instance;
        public static VoidType Instance => _instance ?? (_instance = new VoidType());

        private VoidType() { }

        public override string Name => "void";
    }

    public class ErrorType : Type
    {
        private static ErrorType? _instance;
        public static ErrorType Instance => _instance ?? (_instance = new ErrorType());

        private ErrorType() { }

        public override string Name => "error";

        public override bool IsCompatibleWith(Type other)
        {
            return true;
        }
    }
    public class ArrayType : Type
    {
        public Type ElementType { get; }
        public int Size { get; }

        public ArrayType(Type elementType, int size)
        {
            ElementType = elementType;
            Size = size;
        }

        public override string Name => $"{ElementType.Name}[{Size}]";

        public override string ToString() => $"{ElementType}[{Size}]";

        public override bool IsCompatibleWith(Type other)
        {
            if (other is ArrayType otherArray)
            {
                return ElementType.IsCompatibleWith(otherArray.ElementType);
            }
            return false;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ArrayType otherArray)
            {
                return ElementType.Equals(otherArray.ElementType) && Size == otherArray.Size;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ElementType, Size);
        }
    }
    public static class TypeHelper
    {
        public static Type FromTokenType(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.INT:
                    return IntType.Instance;
                case TokenType.BOOL:
                    return BoolType.Instance;
                case TokenType.STRING_TYPE:
                    return StringType.Instance;
                case TokenType.FLOAT:
                    return IntType.Instance; 
                default:
                    return ErrorType.Instance;
            }
        }

        public static Type FromLiteral(object? value)
        {
            if (value == null) return VoidType.Instance;
            if (value is double || value is int) return IntType.Instance;
            if (value is bool) return BoolType.Instance;
            if (value is string) return StringType.Instance;
            return ErrorType.Instance;
        }

        public static bool IsNumeric(Type type)
        {
            return type is IntType;
        }

        public static bool IsBoolean(Type type)
        {
            return type is BoolType;
        }
    }
}
