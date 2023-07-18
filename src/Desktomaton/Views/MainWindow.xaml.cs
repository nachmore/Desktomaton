using System.Collections.Generic;
using System.Windows;

namespace Desktomaton.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {

    RuleRunner _ruleRunner;

    public MainWindow()
    {
      InitializeComponent();

      _ruleRunner = PrivateRuleRunner.Instance;
    }

    private void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
      _ruleRunner.RunOnce();
    }

    private void btnPaged_Click(object sender, RoutedEventArgs e)
    {
      var manualPagedTrigger = _ruleRunner.NamedTriggers["manualPageTrigger"];

      var curVal = manualPagedTrigger.GetProperty("CurrentValue") as string;

      if (curVal == "Paged")
      {
        manualPagedTrigger.SetProperty("CurrentValue", null);
        btnPaged.Content = "🚨";
      }
      else
      {
        manualPagedTrigger.SetProperty("CurrentValue", "Paged");
        btnPaged.Content = "❌🚨";
      }
      
      _ruleRunner.RunOnce();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      // we're a tray icon app, hide the window instead of closing it completely
      // TODO: should move all functionality out of the window and into its own class
      //       and then the window can be destroyed and recreated when needed to save memory.
      e.Cancel = true;
      this.Hide();
    }
  }
}
