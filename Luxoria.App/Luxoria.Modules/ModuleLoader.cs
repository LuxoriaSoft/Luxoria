using Luxoria.Modules.Interfaces;
using System;
using System.IO;
using System.Reflection;

namespace Luxoria.Modules;

public class ModuleLoader
{
    public IModule LoadModule(string path)
    {
        // Check if file exists
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Module not found : [" + path + "]");
        }

        // Load the assembly
        Assembly assembly = Assembly.LoadFrom(path);

        // Find the module type
        Type[] moduleTypes = assembly.GetTypes();

        foreach (Type type in moduleTypes)
        {
            // Check if the type implements IModule
            if (typeof(IModule).IsAssignableFrom(type) && !type.IsAbstract)
            {
                // Create an instance of the module
                IModule? module = Activator.CreateInstance(type) as IModule;

                // Check for null and throw if necessary
                if (module == null)
                {
                    throw new InvalidOperationException($"Failed to create instance of module type: {type.FullName}");
                }

                return module;
            }
        }
        throw new InvalidOperationException("No valid module found in assembly.");
    }
}
