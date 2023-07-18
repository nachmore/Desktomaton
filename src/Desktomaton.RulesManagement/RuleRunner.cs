using Desktomaton.PluginBase;
using Desktomaton.RulesManagement;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Desktomaton
{
  public abstract class RuleRunner
  {
    private const uint DEFAULT_INTERVAL_MINUTES = 3;

    private CancellationTokenSource _cancellationTokenSource;

    public List<RuleGroup> RuleGroups { get; protected set; } = new List<RuleGroup>();
    public Dictionary<string, DesktomatonTrigger> NamedTriggers { get; protected set; } = new Dictionary<string, DesktomatonTrigger>();

    public abstract void Initialize();

    public void Start(uint interval = DEFAULT_INTERVAL_MINUTES)
    {
      // prevent multiple runs - for now, if cancelled, we're exiting
      if (_cancellationTokenSource != null)
      {
        return;
      }

      _cancellationTokenSource = new CancellationTokenSource();

      var timerTask = RunTimerAsync(RunOnce, TimeSpan.FromMinutes(interval), _cancellationTokenSource.Token);
    }

    private async Task RunTimerAsync(Func<Task> action, TimeSpan interval, CancellationToken token)
    {
      while (!token.IsCancellationRequested)
      {
        _ = Task.Run(action, token);
        await Task.Delay(interval, token);
      }
    }

    public void Cancel()
    {
      _cancellationTokenSource.Cancel();
    }

    public Task RunOnce()
    {
      var rulesEngine = new RulesEngine();
      return rulesEngine.RunAsync(RuleGroups);
    }
  }
}
