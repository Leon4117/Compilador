using System;
using System.Collections.Generic;

namespace Compilador
{
    public class IRInterpreter
    {
        private Dictionary<string, object> _memory = new Dictionary<string, object>();
        private Dictionary<string, int> _labels = new Dictionary<string, int>();

        public void Execute(ThreeAddressCode code)
        {
            for (int i = 0; i < code.Instructions.Count; i++)
            {
                var instr = code.Instructions[i];
                if (instr.Op == OpCode.LABEL)
                {
                    _labels[instr.Result!] = i;
                }
            }

            int ip = 0;
            while (ip < code.Instructions.Count)
            {
                var instr = code.Instructions[ip];
                
                switch (instr.Op)
                {
                    case OpCode.ADD:
                        Set(instr.Result, GetDouble(instr.Arg1) + GetDouble(instr.Arg2));
                        break;
                    case OpCode.SUB:
                        Set(instr.Result, GetDouble(instr.Arg1) - GetDouble(instr.Arg2));
                        break;
                    case OpCode.MUL:
                        Set(instr.Result, GetDouble(instr.Arg1) * GetDouble(instr.Arg2));
                        break;
                    case OpCode.DIV:
                        Set(instr.Result, GetDouble(instr.Arg1) / GetDouble(instr.Arg2));
                        break;

                    case OpCode.LT:
                        Set(instr.Result, GetDouble(instr.Arg1) < GetDouble(instr.Arg2));
                        break;
                    case OpCode.LTE:
                        Set(instr.Result, GetDouble(instr.Arg1) <= GetDouble(instr.Arg2));
                        break;
                    case OpCode.GT:
                        Set(instr.Result, GetDouble(instr.Arg1) > GetDouble(instr.Arg2));
                        break;
                    case OpCode.GTE:
                        Set(instr.Result, GetDouble(instr.Arg1) >= GetDouble(instr.Arg2));
                        break;
                    case OpCode.EQ:
                        Set(instr.Result, IsEqual(Get(instr.Arg1), Get(instr.Arg2)));
                        break;
                    case OpCode.NEQ:
                        Set(instr.Result, !IsEqual(Get(instr.Arg1), Get(instr.Arg2)));
                        break;

                    case OpCode.ASSIGN:
                        Set(instr.Result, Get(instr.Arg1));
                        break;

                    case OpCode.GOTO:
                        ip = _labels[instr.Result!];
                        break;

                    case OpCode.IF_FALSE:
                        if (!IsTruthy(Get(instr.Arg1)))
                        {
                            ip = _labels[instr.Result!];
                        }
                        break;

                    case OpCode.PRINT:
                        Console.WriteLine(Stringify(Get(instr.Arg1)));
                        break;

                    case OpCode.NEW_ARRAY:
                        int size = (int)GetDouble(instr.Arg1);
                        Set(instr.Result, new object?[size]);
                        break;

                    case OpCode.ARRAY_LOAD:
                        {
                            object?[] array = (object?[])Get(instr.Arg1)!;
                            int index = (int)GetDouble(instr.Arg2);
                            Set(instr.Result, array[index]);
                        }
                        break;

                    case OpCode.ARRAY_STORE:
                        {
                            object?[] array = (object?[])Get(instr.Result)!;
                            int index = (int)GetDouble(instr.Arg1);
                            array[index] = Get(instr.Arg2);
                        }
                        break;

                    case OpCode.HALT:
                        return;
                }

                ip++;
            }
        }

        private object? Get(string? arg)
        {
            if (arg == null) return null;
            
            if (arg == "true") return true;
            if (arg == "false") return false;
            if (arg == "nil") return null;
            if (arg.StartsWith("\"") && arg.EndsWith("\""))
            {
                return arg.Substring(1, arg.Length - 2);
            }
            if (double.TryParse(arg, out double val))
            {
                return val;
            }

            if (_memory.ContainsKey(arg))
            {
                return _memory[arg];
            }
            
            throw new Exception($"Runtime Error: Undefined variable or temporary '{arg}'");
        }

        private double GetDouble(string? arg)
        {
            object? val = Get(arg);
            if (val is double d) return d;
            if (val is int i) return (double)i;
            throw new Exception($"Runtime Error: Expected number, got {val}");
        }

        private void Set(string? name, object? value)
        {
            if (name != null) _memory[name] = value!;
        }

        private bool IsTruthy(object? obj)
        {
            if (obj == null) return false;
            if (obj is bool b) return b;
            return true;
        }

        private bool IsEqual(object? a, object? b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }

        private string Stringify(object? obj)
        {
            if (obj == null) return "nil";
            return obj.ToString() ?? "";
        }
    }
}
