using Desktomaton.PluginBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Desktomaton.PluginManagement
{
  public class PluginManager
  {

    public List<DesktomatonTrigger> Triggers { get; private set; }
    public List<DesktomatonAction> Actions { get; private set; }

    public void Load(string[] paths)
    {
      var actions = new List<DesktomatonAction>();
      var triggers = new List<DesktomatonTrigger>();

      // Navigate up to the solution root
      string pluginDir = Path.GetDirectoryName(typeof(PluginManager).Assembly.Location);

      Debug.WriteLine($"Loading plugins from: {pluginDir}");
      var loadContext = new PluginLoadContext(pluginDir);

      foreach (var file in Directory.GetFiles(pluginDir, "*.Plugins.*.dll"))
      {
        Debug.WriteLine($"Attempting to load {file}");

        var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(file)));
        triggers.AddRange(LoadType<DesktomatonTrigger>(assembly));
        actions.AddRange(LoadType<DesktomatonAction>(assembly));
      }

      Triggers = triggers;
      Actions = actions;
    }

    public T Get<T>(string type)
    {
      List<T> plugins;

      if (typeof(T) == typeof(DesktomatonAction))
      {
        plugins = Actions as List<T>;
      }
      else if (typeof(T) == typeof(DesktomatonTrigger))
      {
        plugins = Triggers as List<T>;
      }
      else
      {
        throw new TypeAccessException($"T must be DesktomatonAction or DesktomatonTrigger, not {typeof(T).Name}");
      }

      foreach (var plugin in plugins)
      {
        if (plugin.GetType().Name == type)
        {
          return plugin;
        }
      }

      return default(T);
    }

    private IEnumerable<T> LoadType<T>(Assembly assembly) where T : class
    {

      foreach (var type in assembly.GetTypes())
      {
        if (typeof(T).IsAssignableFrom(type))
        {
          Debug.WriteLine($"{assembly.FullName}: Activating {type}");

          if (Activator.CreateInstance(type) is T rv)
            yield return rv;
        }
      }
    }

    private List<T> Load<T>(string[] paths)
    {
      throw new NotImplementedException();
    }
  }
}
