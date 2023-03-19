using System.Reflection;
using Jaktnat.Compiler.ObjectModel;
using Jaktnat.Runtime;

namespace Jaktnat.Compiler.Reflection;

internal static class FreeFunctionResolver
{
    public static void PopulateGlobals(CompilationContext context)
    {
        foreach (var assembly in context.LoadedAssemblies)
        {
            var types = assembly.GetTypes().Where(i => i.IsClass && i.IsVisible);

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Select(i => new
                    {
                        Attribute = i.GetCustomAttribute<FreeFunctionAttribute>(),
                        Method = i,
                    })
                    .Where(i => i.Attribute != null);

                foreach (var method in methods)
                {
                    var name = method.Attribute!.Name;
                    var newFunction = new RuntimeMethodBaseFunction(method.Method);

                    context.CompilationUnit.DeclareFreeFunction(name, newFunction);
                }
            }
        }
    }
}