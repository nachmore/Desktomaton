using Desktomaton.PluginBase;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Desktomaton.RulesManagement
{
  public class Rule
  {

    private List<IDesktomatonTrigger> _triggers;
    public List<IDesktomatonTrigger> Triggers
    {
      get
      {
        if (_triggers == null)
        {
          _triggers = new List<IDesktomatonTrigger>();
        }

        return _triggers;
      }
      set { _triggers = value; }
    }

    private List<IDesktomatonAction> _actions;
    public List<IDesktomatonAction> Actions
    {
      get
      {
        if (_actions == null)
        {
          _actions = new List<IDesktomatonAction>();
        }

        return _actions;
      }
      set { _actions = value; }
    }


    public RuleTypes RuleType { get; set; }

    public async Task<bool> EvaluateAsync()
    {
      var rv = false;

      // fall through rule - i.e. this rule will always execute its actions because it
      // has no triggers
      if (Triggers.Count == 0)
      {
        rv = true;
        await RunActions();
      }

      foreach (var trigger in Triggers)
      {
        rv = await trigger.EvaluteAsync();

        if (this.RuleType == RuleTypes.OR && rv)
        {
          await RunActions();
          return true;
        } 
        else if (RuleType == RuleTypes.AND && !rv) 
        {
          return false;
        }
      }

      // RuleType == AND
      if (rv)
        await RunActions();

      // we return the value because it's important for RuleGroup evaluation
      return rv;
    }

    private async Task RunActions()
    {
      foreach (var action in Actions)
      {
        await action.RunAsync();
      }
    }
  }
}
