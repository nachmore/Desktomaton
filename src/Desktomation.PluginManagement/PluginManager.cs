﻿using Desktomaton.PluginBase;
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

    public List<IDesktomatonTrigger> Triggers { get; private set; }
    public List<IDesktomatonAction> Actions { get; private set; }

    public void Load(string[] paths)
    {
      var actions = new List<IDesktomatonAction>();
      var triggers = new List<IDesktomatonTrigger>();

      // Navigate up to the solution root
      string pluginDir = Path.GetDirectoryName(typeof(PluginManager).Assembly.Location);
     
      Debug.WriteLine($"Loading plugins from: {pluginDir}");
      var loadContext = new PluginLoadContext(pluginDir);

      foreach (var file in Directory.GetFiles(pluginDir, "*.Plugins.*.dll"))
      {
        Debug.WriteLine($"Attempting to load {file}");

        var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(file)));

        triggers.AddRange(LoadType<IDesktomatonTrigger>(assembly));
        actions.AddRange(LoadType<IDesktomatonAction>(assembly));
      }

      Triggers = triggers;
      Actions = actions;
    }

    private IEnumerable<T> LoadType<T>(Assembly assembly) where T: class
    {
      
      foreach (var type in assembly.GetTypes())
      {
        if (typeof(T).IsAssignableFrom(type))
        {
          Debug.WriteLine($"{assembly.FullName}: Activating {type}");
          var rv = Activator.CreateInstance(type) as T;

          if (rv != null)
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
