using Desktomaton.PluginBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Desktomaton.Plugins.Outlook
{
  public class OutlookPlugin : IDesktomatonTrigger
  {
    public string Name => "Outlook";

    public List<IPluginProperty> Properties => throw new NotImplementedException();

    public bool Evalute()
    {
      Debug.WriteLine("OutlookPlugin: Evaluate()");
      
      return true;
    }
  }
}
