using Desktomaton.PluginBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Desktomaton
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      var pm = new PluginManagement.PluginManager();
      pm.Load(null);

      var rg = new RulesManagement.RuleGroup();
      var rule = new RulesManagement.Rule();

      var outlookTrigger = pm.Triggers[0].CreateInstance();
      var slackAction = pm.Actions[0].CreateInstance();

      outlookTrigger.SetValue("Subject", "i");
      slackAction.SetValue("Slack Token", SLACK_TOKEN);

      rule.Triggers.Add(outlookTrigger);
      rule.Actions.Add(slackAction);

      rg.Rules.Add(rule);
      var re = new RulesManagement.RulesEngine();

      re.RunAsync(new List<RulesManagement.RuleGroup>() { rg });


    }
  }
}
