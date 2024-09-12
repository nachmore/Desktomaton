using Desktomaton.Logger;
using Desktomaton.PluginBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Desktomaton.RulesManagement
{
  [Serializable]
  public class Rule
  {

    private List<DesktomatonTrigger> _triggers;
    public List<DesktomatonTrigger> Triggers
    {
      get
      {
        if (_triggers == null)
        {
          _triggers = new List<DesktomatonTrigger>();
        }

        return _triggers;
      }
      set { _triggers = value; }
    }

    private List<DesktomatonAction> _actions;
    public List<DesktomatonAction> Actions
    {
      get
      {
        if (_actions == null)
        {
          _actions = new List<DesktomatonAction>();
        }

        return _actions;
      }
      set { _actions = value; }
    }

    /// <summary>
    /// Default type is AND (i.e. all triggers must match)
    /// </summary>
    public RuleTypes RuleType { get; set; } = RuleTypes.AND;

    /// <summary>
    /// User friendly name for this rule
    /// </summary>
    public string Name { get; private set; }

    public Rule(string name)
    {
      Name = name;
    }

    public async Task<bool> EvaluateAsync()
    {
      var rv = false;

      // fall through rule - i.e. this rule will always execute its actions because it
      // has no triggers
      if (Triggers.Count == 0)
      {
        rv = true;
      }

      uint? suggestedExpiry = null;
      string suggestedStatus = null;

      foreach (var trigger in Triggers)
      {
        rv = await trigger.EvaluateAsync();

        if (this.RuleType == RuleTypes.OR && rv)
        {
          await RunActions(trigger.SuggestedExpiry, trigger.SuggestedStatus);
          return true;
        } 
        else if (RuleType == RuleTypes.AND && !rv) 
        {
          return false;
        }

        if (rv)
        {
          if (suggestedExpiry == null)
          {
            suggestedExpiry = trigger.SuggestedExpiry;
          }
          else
          {
            // suggestedExpiry in AND is the lowest non null SuggestedExpiry
            suggestedExpiry = (suggestedExpiry.HasValue &&
                              trigger.SuggestedExpiry.HasValue &&
                              suggestedExpiry > trigger.SuggestedExpiry ? trigger.SuggestedExpiry : suggestedExpiry);
          }

          // first suggested status wins
          suggestedStatus ??= trigger.SuggestedStatus;
        }
      }

      // RuleType == AND
      if (rv)
        await RunActions(suggestedExpiry, suggestedStatus);
      
      // we return the value because it's important for RuleGroup evaluation
      return rv;
    }

    private async Task RunActions(uint? SuggestedExpiry, string SuggestedStatus)
    {
      foreach (var action in Actions)
      {
        try
        {
          Log.WriteLine($"▶️ Running {action.Name}");
          await action.RunAsync(SuggestedExpiry, SuggestedStatus);
        }
        catch (Exception e)
        {
          Log.WriteLine($"{e.GetType().Name} thrown running {action.Name}:\n{e}");

          if (Debugger.IsAttached)
            Debugger.Break();
        }
      }
    }
  }
}
