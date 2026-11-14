// Guards the modular monolith: a module may use another module only through its Contracts namespace.
using System.Reflection;
using Auction.Core.Common;

namespace Auction.UnitTests.Architecture;

public class ModuleBoundaryTests
{
    private const string CoreRoot = "Auction.Core";
    private static readonly string[] Modules = ["Platform", "Users", "Lots", "Bidding"];
    private static readonly Assembly CoreAssembly = typeof(AppError).Assembly;

    public static TheoryData<string> ModuleNames => new(Modules);

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_depends_on_other_modules_only_through_contracts(string module)
    {
        var violations = CoreAssembly.GetTypes()
            .Where(type => ModuleOf(type) == module)
            .SelectMany(type => ReferencedTypes(type).Select(referenced => (type, referenced)))
            .Where(pair => IsForeignImplementation(pair.referenced, module))
            .Select(pair => $"{pair.type.FullName} -> {pair.referenced.FullName}")
            .Distinct()
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Common_does_not_depend_on_any_module()
    {
        var violations = CoreAssembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith($"{CoreRoot}.Common", StringComparison.Ordinal) == true)
            .SelectMany(type => ReferencedTypes(type).Select(referenced => (type, referenced)))
            .Where(pair => ModuleOf(pair.referenced) is not null)
            .Select(pair => $"{pair.type.FullName} -> {pair.referenced.FullName}")
            .Distinct()
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Core_does_not_reference_infrastructure_or_api()
    {
        var references = CoreAssembly.GetReferencedAssemblies().Select(a => a.Name).ToList();

        Assert.DoesNotContain("Auction.Infrastructure", references);
        Assert.DoesNotContain("Auction.Api", references);
    }

    [Fact]
    public void Infrastructure_does_not_reference_api()
    {
        var infrastructure = typeof(Auction.Infrastructure.InfrastructureModule).Assembly;
        var references = infrastructure.GetReferencedAssemblies().Select(a => a.Name).ToList();

        Assert.DoesNotContain("Auction.Api", references);
    }

    private static string? ModuleOf(Type type) =>
        Modules.FirstOrDefault(module => IsInNamespace(type, $"{CoreRoot}.{module}"));

    private static bool IsForeignImplementation(Type referenced, string module)
    {
        var owner = ModuleOf(referenced);
        return owner is not null
            && owner != module
            && !IsInNamespace(referenced, $"{CoreRoot}.{owner}.Contracts");
    }

    private static bool IsInNamespace(Type type, string root) =>
        type.Namespace == root || type.Namespace?.StartsWith(root + ".", StringComparison.Ordinal) == true;

    private static IEnumerable<Type> ReferencedTypes(Type type)
    {
        const BindingFlags members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;

        var direct = new List<Type?> { type.BaseType };
        direct.AddRange(type.GetInterfaces());
        direct.AddRange(type.GetFields(members).Select(f => f.FieldType));
        direct.AddRange(type.GetProperties(members).Select(p => p.PropertyType));

        foreach (var method in type.GetMethods(members))
        {
            direct.Add(method.ReturnType);
            direct.AddRange(method.GetParameters().Select(p => p.ParameterType));
        }

        foreach (var constructor in type.GetConstructors(members))
        {
            direct.AddRange(constructor.GetParameters().Select(p => p.ParameterType));
        }

        return direct.OfType<Type>().SelectMany(Unwrap);
    }

    private static IEnumerable<Type> Unwrap(Type type)
    {
        if (type.HasElementType)
        {
            return Unwrap(type.GetElementType()!);
        }

        if (type.IsGenericType)
        {
            return type.GetGenericArguments().SelectMany(Unwrap).Prepend(type.GetGenericTypeDefinition());
        }

        return [type];
    }
}
