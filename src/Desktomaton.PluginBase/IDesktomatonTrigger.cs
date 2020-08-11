using System;
using System.Threading.Tasks;

namespace Desktomaton.PluginBase
{
  public interface IDesktomatonTrigger : IDesktomatonPluginBase
  {
    Task<bool> EvaluteAsync();

    public IDesktomatonTrigger CreateInstance()
    {
      return (IDesktomatonTrigger)Activator.CreateInstance(this.GetType());
    }

  }
}
