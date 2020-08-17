using Desktomaton.PluginBase;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading;
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

namespace Desktomaton.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private RulesManagement.RuleGroup _ruleGroup = new RulesManagement.RuleGroup();

    public MainWindow()
    {
      InitializeComponent();

      NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
      Navigate(NavView.SelectedItem);
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
      if (args.IsSettingsInvoked)
      {
        //Navigate(typeof(SettingsPage));
        //TODO...
      }
      else
      {
        Navigate(args.InvokedItemContainer);
      }
    }

    private void Navigate(object item)
    {
      if (item is NavigationViewItem menuItem)
      {
        Type pageType = GetPageType(menuItem);
        if (ContentFrame.CurrentSourcePageType != pageType)
        {
          ContentFrame.Navigate(pageType);
        }
      }
    }

    private void Navigate(Type sourcePageType)
    {
      if (ContentFrame.CurrentSourcePageType != sourcePageType)
      {
        ContentFrame.Navigate(sourcePageType);
      }
    }

    private Type GetPageType(NavigationViewItem item)
    {
      return item.Tag as Type;
    }

  }
}
