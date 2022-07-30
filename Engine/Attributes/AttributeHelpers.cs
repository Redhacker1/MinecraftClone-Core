using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Engine.Attributes;
using System.Reflection;

/// <summary>
/// Contains helpers for using attributes, most of these will be used in engine.
/// </summary>
public static class AttributeHelpers
{
    static List<Assembly> gameAssemblies = new List<Assembly>() {GetEngineAssembly()};

    // Called at startup to init the assembly.
    internal static void GetEngineAssembly(Assembly assembly)
    {
        
    }

    [Obsolete("Will probably delete if I cannot find a use for it")]
    public static Assembly[] GetAllAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies();
    }

    public static Assembly GetEngineAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }
    
    public static Assembly GetRunningAssembly()
    {
        return Assembly.GetCallingAssembly();
    }

    public static object[] GetAttributes(Type type)
    {
        return type.GetCustomAttributes(false);
    }

    public static object[] GetAttributes<T>(Type type) where T : Attribute
    {
        Console.WriteLine(typeof(T));
        return type.GetCustomAttributes(typeof(T), false);
    }

    public static IEnumerable<Attribute> GetAttributes(Type t, string MethodName)
    {
        return t.GetMethod(MethodName)?.GetCustomAttributes();
    }
    
    public static List<Type> FindAllTypeOfAttribute<T>(bool publicOnly = true) where T: Attribute
    {
        List<Type> types = new List<Type>();

        foreach (Assembly assembly in GetAllAssemblies())
        {
            Type[] operatingTypes = publicOnly ? assembly.GetExportedTypes() : assembly.GetTypes();
            
            foreach (Type type in assembly.GetTypes())
            {
                foreach (Attribute attribute in GetAttributes(type))
                {
                    if (attribute is T)
                    {  
                        types.Add(type);
                    }
                }
            }
        }

        return types;
    }
    

}