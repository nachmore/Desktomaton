using System;
using System.Collections.Generic;
using System.Text;

namespace Desktomaton.RulesManagement
{
  public class RuleGroup
  {

    public string Name { get; set; }

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

    public bool Evaluate()
    {

      foreach (var rule in Rules)
      {

        // stop as soon as one is successful
        if (rule.Evaluate())
        {
          return true;
        }

      }

      return false;

    }

  }
}
