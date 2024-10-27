using Luxoria.Modules.Interfaces;
using System;
using System.IO;
using System.Reflection;

namespace Luxoria.Modules
{
    public class ModuleLoader
    {
        private readonly Func<string, bool> _fileExists;
        private readonly Func<string, Assembly> _loadAssembly;
        private readonly Func<Type, object?> _createInstance;

        public ModuleLoader(
            Func<string, bool>? fileExists = null, 
            Func<string, Assembly>? loadAssembly = null, 
            Func<Type, object?>? createInstance = null)
        {
            _fileExists = fileExists ?? File.Exists;
            _loadAssembly = loadAssembly ?? Assembly.LoadFrom;
            _createInstance = createInstance ?? Activator.CreateInstance;
        }

        public IModule LoadModule(string path)
        {
            // Check if file exists
            if (!_fileExists(path))
            {
                throw new FileNotFoundException($"Module not found: [{path}]");
            }

            // Load the assembly
            Assembly assembly = _loadAssembly(path);

            // Find and return the first valid module type
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IModule).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    try
                    {
                        // Attempt to create an instance of the module
                        var instance = _createInstance(type);
                        if (instance is IModule module)
                        {
                            return module;
                        }
                        else if (instance == null)  // Handle the case where instance is null
                        {
                            throw new InvalidOperationException($"Failed to create instance of module type: {type.FullName}. Instance creation returned null.");
                        }
                        // If instance is not null and not IModule, continue to next type
                    }
                    catch (MissingMethodException)
                    {
                        throw new InvalidOperationException($"Failed to create instance of module type: {type.FullName}. No parameterless constructor found.");
                    }
                    catch (TargetInvocationException ex)
                    {
                        // Handle both cases where InnerException is null or not
                        string innerMessage = ex.InnerException?.Message ?? "An error occurred during module instantiation.";
                        throw new InvalidOperationException($"Failed to create instance of module type: {type.FullName}. {innerMessage}");
                    }
                }
            }

            throw new InvalidOperationException("No valid module found in assembly.");
        }
    }
}
