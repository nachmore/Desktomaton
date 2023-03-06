using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Desktomaton.PluginBase
{
  [Serializable]
  public abstract class DesktomatonTrigger : DesktomatonPluginBase
  {
    public abstract Task<bool> EvaluateAsync();

    /// <summary>
    /// Allows a trigger to suggest an expiry to the action
    /// </summary>
    public virtual uint? SuggestedExpiry { get; set; }

    /// <summary>
    /// Allows a trigger to suggest a status update to the action
    /// </summary>
    public virtual string SuggestedStatus { get; set; }

    public DesktomatonTrigger CreateInstance()
    {
      return (DesktomatonTrigger)Activator.CreateInstance(this.GetType());
    }
  }
}
