using Desktomaton.PluginBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Desktomaton.Plugins.Slack
{
  public enum SlackAction
  {
    Status,
    DND
  }

  public class SlackPlugin : IDesktomatonAction
  {

    public List<IPluginProperty> Properties { get; } = new List<IPluginProperty>()
    {
      new PluginProperty<string>("Slack Token"),
      new PluginProperty<SlackAction>("Action"),
    };

    public string Name => "Slack";

    public void Run()
    {
      Debug.WriteLine("SlackPlugin: Run()");
    }
  }
}
