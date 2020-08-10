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

      bool a = false;

      var pm = new PluginManagement.PluginManager();
      pm.Load(null);

      var rg = new RulesManagement.RuleGroup();

      var rule = new RulesManagement.Rule();

      var trigger = pm.Triggers[0];
      var prop = pm.Triggers[0].Properties[0];
      var stringProp = ((PluginProperty<string>)(pm.Triggers[0].Properties[0]));

      stringProp.Value = "Interview";

      rule.Triggers.Add(pm.Triggers[0]);
      rule.Actions.Add(pm.Actions[0]);

      rg.Rules.Add(rule);

      var re = new RulesManagement.RulesEngine();

      re.Run(new List<RulesManagement.RuleGroup>() { rg });


    }
  }
}
