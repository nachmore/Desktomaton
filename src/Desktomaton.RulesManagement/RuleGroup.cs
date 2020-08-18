﻿using System;
using System.Collections.Generic;
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
          // stop as soon as one is successful
          // TODO: AND / OR for group (not just rule), though perhaps that's just another group?
          if (await rule.EvaluateAsync())
          {
            return true;
          }

        }
      }

      return false;

    }

  }
}
