using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Desktomaton.PluginBase
{
  [Serializable]
  public abstract class DesktomatonAction : DesktomatonPluginBase
  {
    public abstract Task RunAsync(uint? SuggestedExpiry, string SuggestedStatus);

    public DesktomatonAction CreateInstance()
    {
      return (DesktomatonAction)Activator.CreateInstance(this.GetType());
    }
  }
}
