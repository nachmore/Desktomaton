using Desktomaton.PluginBase;

namespace Desktomaton.Plugins.Manual
{
  [Serializable]
  public class ManualPlugin : DesktomatonTrigger
  {
    public override string Name => "Manual";

    [DesktomatonProperty(PrettyTitle = "Current Value")]
    public string? CurrentValue { get; set; }

    [DesktomatonProperty(PrettyTitle = "Trigger Value")]
    public string? TriggerValue { get; set; }

    public override async Task<bool> EvaluateAsync()
    {
      return TriggerValue == CurrentValue;
    }
  }
}
