using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Desktomaton.PluginBase
{
  [Serializable]
  public abstract class DesktomatonTrigger : DesktomatonPluginBase
  {
    public abstract Task<bool> EvaluateAsync();

    public DesktomatonTrigger CreateInstance()
    {
      return (DesktomatonTrigger)Activator.CreateInstance(this.GetType());
    }

  }
}
