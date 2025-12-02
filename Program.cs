using System;
using System.Collections.Generic;
using System.IO;

namespace Compilador
{
    class Program
    {
        private static readonly Interpreter _interpreter = new Interpreter();
        private static bool _hadError = false;
        private static bool _hadRuntimeError = false;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: Compilador [script]");
                System.Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            string source = File.ReadAllText(path);
            Run(source);

            if (_hadError) System.Environment.Exit(65);
            if (_hadRuntimeError) System.Environment.Exit(70);
        }

        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                string? line = Console.ReadLine();
                if (line == null) break;
                Run(line);
                _hadError = false;
            }
        }

        private static void Run(string source)
        {
            Lexer lexer = new Lexer(source);
            List<Token> tokens = lexer.ScanTokens();

            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();

            if (_hadError) return;

            TypeChecker typeChecker = new TypeChecker();
            typeChecker.Check(statements);

            if (typeChecker.HadError) return;

            IRGenerator irGenerator = new IRGenerator();
            irGenerator.Generate(statements);
            
            Console.WriteLine("--- Intermediate Code ---");
            Console.WriteLine(irGenerator.Code.ToString());
            Console.WriteLine("-------------------------");

            IRInterpreter irInterpreter = new IRInterpreter();
            try 
            {
                irInterpreter.Execute(irGenerator.Code);
            }
            catch (Exception e)
            {
                _hadRuntimeError = true;
                Console.WriteLine(e.Message);
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            _hadError = true;
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"{error.Message}\n[line {error.Token.Line}]");
            _hadRuntimeError = true;
        }
    }
}
