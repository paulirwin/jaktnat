# Jaktnät

[![.NET](https://github.com/paulirwin/jaktnat/actions/workflows/dotnet.yml/badge.svg)](https://github.com/paulirwin/jaktnat/actions/workflows/dotnet.yml)

An experimental port of the [SerenityOS](https://www.serenityos.org/) [Jakt programming language](https://github.com/SerenityOS/jakt) to .NET.

> **WARNING:** This is far from complete and very experimental!

## Basic usage

Compile `Jaktnat.exe` in Visual Studio 2022 or via the `dotnet` CLI (or Rider, or whatever), then run:

```PowerShell
path/to/Jaktnat.exe my_file.jakt
```

This will produce a file called `my_file.exe` in the `bin` subfolder of the current path. 
Note that these exe files are not yet self-executable, you must execute with the `dotnet` CLI:

```PowerShell
dotnet bin/my_file.exe
```

## Overview

Jaktnät aims to implement as much of the Jakt programming language as possible, on the .NET runtime.
Unlike Jakt, which transpiles to C++, it is a compiler that produces .NET assemblies executable via the `dotnet` CLI.
However, it cheats a bit by transforming its Abstract Syntax Tree (AST) into a C# AST via Roslyn, and then uses Roslyn to compile the .NET assembly.
So I guess you could say that it *does* transpile to C#, just via an AST in memory without writing a source file to disk.

(Aside: it would be trivial to make Jaktnät transpile to C# given this, and that is something I aim to implement soon as an option.)

The first version of the backend used Mono.Cecil to emit IL into the assembly, but this is unnecessarily complex if accepting C# language constraints is viable. 
I was able to move much faster using Roslyn instead, so I've relegated the Mono.Cecil backend to an obsolete experiment with only minimal language support currently.
I may revisit that backend in the future.
Note that Mono.Cecil was chosen over System.Reflection.Emit due to its ability to produce an assembly on disk, which is not possible in Emit as of .NET 6.

Jaktnät uses [ANTLR](https://www.antlr.org/) as a parser/lexer generator.
You can see the grammar in the `Jaktnat.g4` file. 
The ANTLR NuGet package automatically builds the lexer and parser and other related files based on this grammar file when the Jaktnat.Compiler project is built.

## Editor Support

As this is just a port of Jakt, currently the syntax is largely the same. 
Therefore, you can get syntax highlighting in your favorite editor (VS Code, Vim, etc.) by following the instructions in the README of the editor-specific folder in the upstream Jakt repo: https://github.com/SerenityOS/jakt/tree/main/editors 

## Language Support

This section tracks the status of language features in Jaktnät.

Language features:

- [X] Top-level functions
- [X] Function parameters
- [X] Function return types
- [X] Basic classes with implicit constructors
- [X] Class static member functions
- [ ] Class non-mutating member functions
- [X] Class mutating member functions
- [ ] Structs
- [ ] Enums
- [X] Arrays
- [X] Built-in type identifiers (i.e. `i32` -> `System.Int32`)
- [X] Binary expressions (i.e. `x + 2`)
- [X] Block syntax and scoping
- [X] `break` and `continue` statements
- [X] Calling functions (i.e. `bubble_sort(values: v)`)
- [X] Accessing non-Jaktnät .NET types (limited support)
- [X] Comments
- [X] Identifier expressions (i.e. `x` resolving to a variable, parameter, etc.)
- [X] `if`/`else` statements
- [X] Indexer access (for arrays or Dictionaries or such, i.e. `x[y + 2]`)
- [X] Integer literals (`System.Int32` by default, if no suffix given)
- [X] Underscore support in integer literals
- [ ] Binary literals
- [X] Boolean literals (`true`, `false`)
- [ ] Byte literals
- [ ] Character literals
- [X] Floating-point literals (partial)
- [ ] Hex literals
- [ ] Octal literals
- [X] String literals
- [ ] String escape sequences
- [X] `loop` statements
- [X] Member access expressions (i.e. `x.y.z`)
- [X] Parenthesized expressions
- [X] `return` statements
- [X] `throw` statements
- [X] `try`/`catch` statements, with either an expression or block for `try`
- [X] Unary expressions (i.e. `x++`)
- [X] Variable declaration (i.e. `let x: i32 = 7`)
- [ ] Enforcing immutable variables
- [X] `while` statements
- [ ] Imports/Modules
- [ ] Optionals
- [ ] `as?` fallible type casts
- [X] `as!` infallible type casts (partial; does not abort program but throws a runtime exception)
- [X] `is` type checking
- [ ] `match` expressions
- [ ] Optional chaining
- [ ] None coalescing for Optionals
- [ ] `defer` statements
- [ ] Closures/Lambdas
- [ ] Dictionaries
- [ ] Sets
- [ ] Tuples
- [ ] Generic types
- [ ] Traits
- [ ] Namespace declarations and access

Other notes from Jakt README:

- [X] Strong pointers (simply regular .NET reference types)
- [ ] Weak pointers (`weak T?`)
- [ ] Raw pointers (`raw T` - low priority)
- [X] Integer overflow safety (built in to .NET)
- [ ] Numeric values are not automatically coerced to `int`
- [ ] Immutable by default (in progress; not yet enforced)
- [X] Argument labels in call expressions
- [X] Pointers are always dereferenced with `.`
- [ ] Trailing closure parameters
- [ ] Error propagation with `ErrorOr<T>` and `try`/`must` keywords
- [ ] Anonymous function parameters (partial; non-anonymous not yet enforced)
- [ ] Allow not specifying argument name if passing variable of same name (partial; not enforced otherwise)
- [ ] Private-by-default members
- [ ] Inheritance
- [ ] Class-based polymorphism
- [ ] `Super` type
- [ ] `Self` type
- [ ] `.` shorthand syntax for accessing members
- [ ] Array initializer size/value syntax (i.e. `[0; 256]`)
- [X] Array initializer expressions (i.e. `[1, 2, 3]`)
- [ ] Declaring/Creating dictionaries
- [X] Indexing dictionaries
- [X] Assigning into dictionaries
- [ ] Creating sets
- [ ] Reference semantics for sets
- [ ] Creating tuples
- [ ] Index tuples
- [ ] Tuple types
- [ ] Inferred enum scope
- [ ] Enums as sub-types
- [ ] Generic enums
- [ ] Enums as names for values of an underlying type
- [ ] Enum scope inference in `match` arms
- [ ] Yielding values from `match` blocks
- [ ] Nested `match` patterns
- [ ] Traits as `match` patterns
- [ ] Support for interop with the `?`, `??` and `!` operators
- [ ] Generic type inference
- [ ] Deep namespace support

Future non-Jakt possibilities to explore:
- [ ] LINQ support
- [ ] Inline C# (instead of inline C++)

## Motivation

I've been a fan of (and very minor contributor to) SerenityOS for a while now, so when Jakt was announced I was intrigued.
I appreciated its similarity to Swift, easy to understand syntax, and that it was, like SerenityOS, built from the ground up without legacy baggage.
But most of all, I could wrap my head around all of its lexer, parser, and compiler code. 

Having already created my own interpreted Lisp- and Scheme-based language for .NET called [Lillisp](https://github.com/paulirwin/lillisp), 
I thought I'd try my hand at a little more complex of a language and implement a Jakt compiler for .NET. 
So this is an experiment for now, and a learning opportunity, more than anything else.

This is not a language that is ready for any kind of serious, production use.

## Unit Tests

As I implement language features, I start by seeing if there is a `samples` folder item in the Jakt repo for the language feature I want to implement next.
I then add these samples as embedded resources in the `Jaktnat.Compiler.Tests` xUnit unit test project in the `samples` folder with the same path.
The `SampleTests` class acts as an embedded resource test runner, that loops over all of these samples, parses out the assertions from the header comment,
compiles the sample, runs the sample, and then asserts based on the configured assertions.

Where possible, I prefer to leave the samples as-is without modification from how they are in the Jakt repo, but sometimes this is unavoidable.
If a modification is necessary (i.e. to call the `.Length` property on arrays rather than `.size()`), I try to ensure that I copy the modified line as a comment 
and note what changed.

## Contributing

This is currently a personal project and is not intended for large-scale community support.
If you'd like to contribute, please file an issue first, and ask if you can be assigned to work on it. 
If so, I'll assign it to you and you can then submit a PR when you are ready.
Please be patient if there is a delay in my response!
