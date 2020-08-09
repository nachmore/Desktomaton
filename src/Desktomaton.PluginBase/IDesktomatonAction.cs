using System;
using System.Collections.Generic;

namespace Desktomaton.PluginBase
{
    public interface IDesktomatonAction : IDesktomatonPluginBase
    {
        void Run();
    }
}
