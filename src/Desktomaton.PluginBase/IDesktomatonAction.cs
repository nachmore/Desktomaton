using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Desktomaton.PluginBase
{
  public interface IDesktomatonAction : IDesktomatonPluginBase
  {
    Task RunAsync();

    public IDesktomatonAction CreateInstance()
    {
      return (IDesktomatonAction)Activator.CreateInstance(this.GetType());
    }
  }
}
