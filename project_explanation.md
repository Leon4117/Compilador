# Explicación del Proyecto

Este proyecto implementa un compilador para un lenguaje similar a C#, siguiendo los principios del "Dragon Book" (Compilers: Principles, Techniques, and Tools).

A continuación se describe la responsabilidad de cada archivo en el proyecto:

## Archivos Principales

### `Program.cs`
Punto de entrada de la aplicación.
- Lee el archivo fuente.
- Orquesta el proceso de compilación: Lexer -> Parser -> TypeChecker -> IRGenerator -> IRInterpreter.
- Maneja errores de compilación y ejecución.

### `Lexer.cs`
Analizador Léxico (Scanner).
- Convierte el código fuente (texto) en una secuencia de tokens.
- Maneja palabras clave, identificadores, números (enteros, flotantes, hex), cadenas y operadores.
- Rastrea números de línea y columna para reportar errores.

### `Parser.cs`
Analizador Sintáctico.
- Convierte la lista de tokens en un Árbol de Sintaxis Abstracta (AST).
- Utiliza el método de Descenso Recursivo.
- Define la gramática del lenguaje (declaraciones, sentencias `if`, `while`, `for`, expresiones, etc.).

### `TypeChecker.cs`
Verificador de Tipos (Análisis Semántico).
- Recorre el AST para verificar la corrección de los tipos.
- Asegura que las variables estén declaradas antes de usarse.
- Verifica compatibilidad de tipos en asignaciones y operaciones.
- Valida que las condiciones de `if`, `while`, `for` sean booleanas.

### `IRGenerator.cs`
Generador de Código Intermedio.
- Traduce el AST a Código de Tres Direcciones (Three-Address Code).
- Aplana el control de flujo usando etiquetas (`labels`) y saltos (`goto`, `if_false`).
- Maneja la creación y acceso a arrays en bajo nivel.

### `IRInterpreter.cs`
Intérprete de Código Intermedio (Máquina Virtual).
- Ejecuta las instrucciones del Código de Tres Direcciones.
- Simula la memoria y el flujo de ejecución.
- Maneja operaciones aritméticas, lógicas, saltos y operaciones de array.

## Estructuras de Datos y Definiciones

### `Token.cs`
Define la estructura de un token (tipo, lexema, literal, posición).

### `Expr.cs`
Define los nodos de expresión del AST (Binary, Unary, Literal, Variable, ArrayAccess, etc.).
Implementa el patrón Visitor para recorrer el árbol.

### `Stmt.cs`
Define los nodos de sentencia del AST (Block, If, While, For, Var, Print, etc.).
Implementa el patrón Visitor.

### `Types.cs`
Define el sistema de tipos del lenguaje (`IntType`, `BoolType`, `StringType`, `ArrayType`, `VoidType`).
Incluye lógica para verificar compatibilidad de tipos.

### `SymbolTable.cs`
Tabla de Símbolos.
- Gestiona los ámbitos (scopes) y las declaraciones de variables.
- Permite buscar variables en el ámbito actual y superiores.

### `Symbol.cs`
Representa una variable en la tabla de símbolos (nombre, tipo, nivel de scope).

### `ThreeAddressCode.cs`
Define las instrucciones y códigos de operación (`OpCode`) para el código intermedio.
- Opcodes: `ADD`, `SUB`, `GOTO`, `IF_FALSE`, `NEW_ARRAY`, etc.

### `Environment.cs`
(Usado por el intérprete antiguo `Interpreter.cs`)
Maneja el entorno de ejecución para variables en el intérprete de árbol (Tree-Walk Interpreter).

### `Interpreter.cs`
(Intérprete antiguo / Legacy)
Implementación de un intérprete que recorre directamente el AST. Actualmente el proyecto usa `IRInterpreter` para la ejecución principal, pero este archivo se mantiene como referencia de la implementación anterior.

### `ControlFlowExceptions.cs`
Define excepciones para manejar `break` y `continue` en el intérprete antiguo.
