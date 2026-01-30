// ReflectionTest.cs
using System;
using System.Reflection;
using Gtk;

public class ReflectionTest
{
    public static void Main()
    {
        Gtk.Application.Init();
        
        Console.WriteLine("=== Exploring Gtk.Application ===");
        Type appType = typeof(Gtk.Application);
        Console.WriteLine($"Type: {appType.FullName}");
        
        // Look for all Invoke methods
        MethodInfo[] invokeMethods = appType.GetMethods(BindingFlags.Public | BindingFlags.Static);
        foreach (var method in invokeMethods)
        {
            if (method.Name == "Invoke")
            {
                Console.WriteLine($"Found Invoke method: {method}");
                var parameters = method.GetParameters();
                foreach (var param in parameters)
                {
                    Console.WriteLine($"  Parameter: {param.ParameterType} {param.Name}");
                }
            }
        }
        
        // Look for GLib.Source related types
        Console.WriteLine("\n=== Exploring GLib assemblies ===");
        Assembly glibAssembly = typeof(GLib.Source).Assembly;
        Console.WriteLine($"Assembly: {glibAssembly.FullName}");
        
        // Find SourceProxy type
        Type[] types = glibAssembly.GetTypes();
        foreach (Type t in types)
        {
            if (t.Name.Contains("Source") || t.Name.Contains("Proxy"))
            {
                Console.WriteLine($"\nFound type: {t.FullName}");
                Console.WriteLine($"  IsPublic: {t.IsPublic}");
                Console.WriteLine($"  Fields:");
                foreach (FieldInfo field in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    Console.WriteLine($"    {field.FieldType} {field.Name}");
                }
            }
        }
    }
}