// Test program to discover AutoCount SDK types and namespaces

using System;
using System.Reflection;

Console.WriteLine("=== AutoCount SDK Type Discovery ===\n");

// List all loaded assemblies that contain "AutoCount"
var assemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName?.Contains("AutoCount", StringComparison.OrdinalIgnoreCase) == true)
    .ToList();

Console.WriteLine($"Found {assemblies.Count} AutoCount assemblies:\n");

foreach (var assembly in assemblies)
{
    Console.WriteLine($"Assembly: {assembly.GetName().Name}");
    Console.WriteLine($"Version: {assembly.GetName().Version}");
    Console.WriteLine($"Location: {assembly.Location}");
    
    // Get public types
    try
    {
        var types = assembly.GetExportedTypes()
            .Where(t => t.IsPublic && !t.IsNested)
            .OrderBy(t => t.Namespace)
            .ThenBy(t => t.Name)
            .ToList();
        
        Console.WriteLine($"Public Types: {types.Count}");
        
        // Group by namespace
        var namespaces = types.GroupBy(t => t.Namespace ?? "(no namespace)")
            .OrderBy(g => g.Key);
        
        foreach (var ns in namespaces)
        {
            Console.WriteLine($"\n  Namespace: {ns.Key}");
            foreach (var type in ns.Take(10)) // Show first 10 types per namespace
            {
                Console.WriteLine($"    - {type.Name}");
            }
            if (ns.Count() > 10)
            {
                Console.WriteLine($"    ... and {ns.Count() - 10} more types");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error loading types: {ex.Message}");
    }
    
    Console.WriteLine("\n" + new string('-', 80) + "\n");
}

// Try to instantiate some common types
Console.WriteLine("\n=== Testing Common Types ===\n");

try
{
    Console.WriteLine("Testing AutoCount.Data.DBSetting...");
    var dbSetting = Activator.CreateInstance("AutoCount", "AutoCount.Data.DBSetting");
    Console.WriteLine("✓ AutoCount.Data.DBSetting exists");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ AutoCount.Data.DBSetting: {ex.Message}");
}

try
{
    Console.WriteLine("\nTesting AutoCount.Data.CompanyDataAccess...");
    var cda = Activator.CreateInstance("AutoCount", "AutoCount.Data.CompanyDataAccess");
    Console.WriteLine("✓ AutoCount.Data.CompanyDataAccess exists");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ AutoCount.Data.CompanyDataAccess: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

