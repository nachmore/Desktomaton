using Desktomaton.PluginBase;
using System;
using System.Collections.Generic;
using System.Text;

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

    public bool Evaluate()
    {
      var rv = false;

      foreach (var trigger in Triggers)
      {
        rv = trigger.Evalute();

        if (this.RuleType == RuleTypes.OR && rv)
        {
          RunActions();
          return true;
        } 
        else if (RuleType == RuleTypes.AND && !rv) 
        {
          return false;
        }
      }

      // RuleType == AND
      if (rv)
        RunActions();

      // we return the value because it's important for RuleGroup evaluation
      return rv;
    }

    private void RunActions()
    {
      foreach (var action in Actions)
      {
        action.Run();
      }
    }
  }
}
