using Desktomaton.Views;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Drawing;
using System.Windows;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using ContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using ToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;
using ToolStripSeparator = System.Windows.Forms.ToolStripSeparator;
using System.Diagnostics;
using System.Media;
using Desktomaton.Logger;
using System.Threading;

namespace Desktomaton
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : PrismApplication
  {
    private Mutex _singleInstanceMutex;

    public MainWindow AppWindow { get; private set; }

    protected override Window CreateShell()
    {
      return null;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      if (!EnsureSingleInstance())
        return;

      InitTrayIcon();

      PrivateRuleRunner.Instance.Start();

      base.OnStartup(e);
    }

    private bool EnsureSingleInstance()
    {
      var mutexCreated = false;

      // not setting into a global context in case multiple users want to run desktomaton
      _singleInstanceMutex = new Mutex(true, "DesktomatonSingleInstance", out mutexCreated);

      if (!mutexCreated)
      {
        Shutdown();

        return false;
      }

      return true;
    }

    private void InitTrayIcon()
    {
      // make sure we don't shut down when the UI window is closed
      Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

      var menuRefresh = new ToolStripMenuItem("&Refresh");
      menuRefresh.Click += TrayMenuRefresh_Click;

      var menu = new ContextMenuStrip();
      var menuExit = new ToolStripMenuItem("E&xit");
      menuExit.Click += TrayMenuExit_Click;

      menu.Items.Add(menuRefresh);
      menu.Items.Add(new ToolStripSeparator());
      menu.Items.Add(menuExit);

      var icon = new NotifyIcon
      {
        Icon = new Icon(GetResourceStream(new Uri("pack://application:,,,/robot.ico")).Stream),
        Visible = true,
        ContextMenuStrip = menu,
        Text = "Desktomaton"
      };

      icon.DoubleClick += new EventHandler(TrayIconShowWindow);
    }

    void TrayIconShowWindow(object sender, EventArgs e)
    {
      if (AppWindow == null)
      {
        AppWindow = Container.Resolve<MainWindow>();
      }

      try
      {
        AppWindow.Show();
      }
      catch (ArgumentException ex)
      {
        // this happens when you click on the icon super fast, there is some kind of 
        // race condition in the framework that is triggered. Validated by locking this
        // function and seeing it still repro (and SO reports etc).
        // Since this only happens when the dialog is already open, just ignore the exception.
        Debug.WriteLine(ex);
      }
    }

    private void TrayMenuRefresh_Click(object sender, EventArgs e)
    {
      PrivateRuleRunner.Instance.RunOnce();
    }

    private void TrayMenuExit_Click(object sender, EventArgs e)
    {
      Application.Current.Shutdown();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {

    }
  }
}
