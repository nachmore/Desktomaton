using System.Collections.Generic;
using System.Threading.Tasks;

namespace Desktomaton.RulesManagement
{
  public class RulesEngine
  {

    /// <summary>
    /// Returns the number of groups that triggered successfully
    /// </summary>
    /// <param name="ruleGroups"></param>
    /// <returns></returns>
    public async Task<int> RunAsync(List<RuleGroup> ruleGroups)
    {
      var rv = 0;

      foreach (var group in ruleGroups)
      {
        if (await group.EvaluateAsync())
          rv++;
      }

      return rv;
    }

  }
}
