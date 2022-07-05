using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Desktomaton.PluginManagement
{
  /// <summary>
  /// See: https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
  /// </summary>
  class PluginLoadContext : AssemblyLoadContext
  {
    private AssemblyDependencyResolver _resolver;
    private string _pluginPath;

    public PluginLoadContext(string pluginPath)
    {
      _pluginPath = pluginPath;
      _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
      string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
      if (assemblyPath != null)
      {
        return LoadFromAssemblyPath(assemblyPath);
      }

      // fall back... TODO: investigate this, since this looks to break the context loading
      // which unfortunately doesn't work as dependencies (System.Runtime!) are not loaded
      return Assembly.LoadFrom(Path.Combine(_pluginPath, assemblyName.Name + ".dll"));
    }
  }
}
