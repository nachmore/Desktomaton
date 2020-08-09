using Desktomaton.PluginBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Desktomaton.Plugins
{
  public class PluginManager
  {

    public List<IDesktomatonTrigger> Triggers { get; private set; }
    public List<IDesktomatonAction> Actions { get; private set; }

    public void Load(string[] paths)
    {

      Triggers = Load<IDesktomatonTrigger>(paths);
      Actions = Load<IDesktomatonAction>(paths);

    }

    private List<T> Load<T>(string[] paths)
    {
      throw new NotImplementedException();
    }
  }
}
