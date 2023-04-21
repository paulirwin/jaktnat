using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Reflection;

public record OverloadResolutionParameter(
    TypeReference? Type, 
    string? Name, 
    bool IsParams,
    object? DefaultValue
);