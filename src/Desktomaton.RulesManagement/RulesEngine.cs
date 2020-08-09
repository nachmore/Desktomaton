using System.Collections.Generic;

namespace Desktomaton.RulesManagement
{
  public class RulesEngine
  {

    /// <summary>
    /// Returns the number of groups that triggered successfully
    /// </summary>
    /// <param name="ruleGroups"></param>
    /// <returns></returns>
    public int Run(List<RuleGroup> ruleGroups)
    {

      var rv = 0;

      foreach (var group in ruleGroups)
      {
        if (group.Evaluate())
          rv++;
      }

      return rv;
    }

  }
}
