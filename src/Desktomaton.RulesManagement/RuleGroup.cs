using Desktomaton.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Desktomaton.RulesManagement
{
  [Serializable]
  public class RuleGroup
  {

    public string Name { get; set; }
    public bool Enabled { get; set; } = true;

    private List<Rule> _rules;
    public List<Rule> Rules
    {
      get
      {
        // make sure we always have a default, but only if needed
        if (_rules == null)
        {
          _rules = new List<Rule>();
        }

        return _rules;
      }
      set { _rules = value; }
    }

    public async Task<bool> EvaluateAsync()
    {
      if (Enabled)
      {
        foreach (var rule in Rules)
        {

          if (rule.Actions.Count == 0 && rule.Triggers.Count == 0)
          {
            if (Debugger.IsAttached)
            {
              throw new InvalidOperationException("DEBUG Only: Rule has no actions and no triggers");
            }

            Log.WriteLine($"❌ Ignoring {rule.Name} as it has no actions and no triggers");

            // ignore this rule
            continue;
          }


          Log.WriteLine($"▶️ Evaluating {rule.Name}");

          // stop as soon as one is successful
          // TODO: AND / OR for group (not just rule), though perhaps that's just another group?
          if (await rule.EvaluateAsync())
          {
            Log.WriteLine($"  ✅ Rule successful");
            return true;
          }

        }
      }

      return false;

    }

  }
}
