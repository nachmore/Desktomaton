using System;

namespace Desktomaton.PluginBase
{
  public interface IDesktomatonTrigger : IDesktomatonPluginBase
  {
    bool Evalute();

    public IDesktomatonTrigger CreateInstance()
    {
      return (IDesktomatonTrigger)Activator.CreateInstance(this.GetType());
    }

  }
}
