using Desktomaton.RulesManagement;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace Desktomaton.ViewModels
{
  class RulesPageViewModel : BindableBase
  {

    public List<RuleGroup> RuleGroups { 
      get
      {
        return null;
      } 
    }

    public RulesPageViewModel()
    {

    }

    private DelegateCommand _commandCreateRuleGroup = null;
    public DelegateCommand CommandCreateRuleGroup =>
        _commandCreateRuleGroup ?? (_commandCreateRuleGroup = new DelegateCommand(CommandCreateRuleGroupExecute));

    private void CommandCreateRuleGroupExecute()
    {
      System.Windows.MessageBox.Show("yo");
    }
  }
}
