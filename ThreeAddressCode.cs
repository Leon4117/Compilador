using System.Collections.Generic;

namespace Compilador
{
    public enum OpCode
    {
        ADD, SUB, MUL, DIV,
        LT, LTE, GT, GTE, EQ, NEQ,
        AND, OR, NOT,
        ASSIGN,
        LOAD,
        STORE,
        GOTO,
        IF_FALSE,
        LABEL,
        PRINT,
        ARRAY_LOAD,
        ARRAY_STORE,
        NEW_ARRAY,
        HALT
    }

    public class Instruction
    {
        public OpCode Op { get; }
        public string? Arg1 { get; }
        public string? Arg2 { get; }
        public string? Result { get; }

        public Instruction(OpCode op, string? arg1 = null, string? arg2 = null, string? result = null)
        {
            Op = op;
            Arg1 = arg1;
            Arg2 = arg2;
            Result = result;
        }

        public override string ToString()
        {
            switch (Op)
            {
                case OpCode.ADD: return $"{Result} = {Arg1} + {Arg2}";
                case OpCode.SUB: return $"{Result} = {Arg1} - {Arg2}";
                case OpCode.MUL: return $"{Result} = {Arg1} * {Arg2}";
                case OpCode.DIV: return $"{Result} = {Arg1} / {Arg2}";
                
                case OpCode.LT:  return $"{Result} = {Arg1} < {Arg2}";
                case OpCode.LTE: return $"{Result} = {Arg1} <= {Arg2}";
                case OpCode.GT:  return $"{Result} = {Arg1} > {Arg2}";
                case OpCode.GTE: return $"{Result} = {Arg1} >= {Arg2}";
                case OpCode.EQ:  return $"{Result} = {Arg1} == {Arg2}";
                case OpCode.NEQ: return $"{Result} = {Arg1} != {Arg2}";
                
                case OpCode.ASSIGN: return $"{Result} = {Arg1}";
                case OpCode.LOAD:   return $"{Result} = LOAD {Arg1}";
                case OpCode.STORE:  return $"STORE {Result} = {Arg1}";
                
                case OpCode.GOTO:     return $"GOTO {Result}";
                case OpCode.IF_FALSE: return $"IF_FALSE {Arg1} GOTO {Result}";
                case OpCode.LABEL:    return $"{Result}:";
                
                case OpCode.PRINT: return $"PRINT {Arg1}";
                
                case OpCode.ARRAY_LOAD: return $"{Result} = {Arg1}[{Arg2}]";
                case OpCode.ARRAY_STORE: return $"{Result}[{Arg1}] = {Arg2}";
                case OpCode.NEW_ARRAY: return $"{Result} = NEW_ARRAY {Arg1}";

                case OpCode.HALT:  return "HALT";
                
                default: return $"{Op} {Arg1} {Arg2} {Result}";
            }
        }
    }

    public class ThreeAddressCode
    {
        public List<Instruction> Instructions { get; } = new List<Instruction>();

        public void Emit(OpCode op, string? arg1 = null, string? arg2 = null, string? result = null)
        {
            Instructions.Add(new Instruction(op, arg1, arg2, result));
        }

        public override string ToString()
        {
            string output = "";
            foreach (var instr in Instructions)
            {
                output += instr.ToString() + "\n";
            }
            return output;
        }
    }
}
