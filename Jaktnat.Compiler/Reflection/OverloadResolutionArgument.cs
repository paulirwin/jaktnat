using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Reflection;

public record OverloadResolutionArgument(TypeReference? Type, string? Name);