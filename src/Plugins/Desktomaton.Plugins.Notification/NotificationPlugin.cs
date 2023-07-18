using Desktomaton.PluginBase;
using System;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using System.Windows.Media;
using System.Reflection;
using System.IO;
using Desktomaton.Plugins.Notification.DesktomatonNotification;
using System.Diagnostics;

namespace Desktomaton.Plugins.Notification
{
  public class NotificationPlugin : DesktomatonAction
  {
    static NotificationPlugin()
    {
      _ = Application.Current.Dispatcher.InvokeAsync(() =>
        {
          // This is completely fake code that is never actually run, but forces .NET to load the
          // ToastNotifications.Messages library already at this point, allowing us to add the resource
          // to the dictionary
          if (Application.Current.MainWindow != null)
          {
            var notifier = new Notifier(null);
            notifier.ShowError(null);
          }

          // inject required resources - but only once
          var dictionary = new ResourceDictionary();
          dictionary.Source = new Uri("pack://application:,,,/ToastNotifications.Messages;component/Themes/Default.xaml");

          Application.Current.Resources.MergedDictionaries.Add(dictionary);
        });
    }

    public override string Name => "Notifications";

    [DesktomatonProperty(PrettyTitle = "Icon")]
    public string Icon { get; set; } = "🤖";

    [DesktomatonProperty(PrettyTitle = "Message")]
    public string Message { get; set; }

    [DesktomatonProperty(PrettyTitle = "Title")]
    public string Title { get; set; } = "Desktomaton Notification";

    [DesktomatonProperty(PrettyTitle = "Play Sound?")]
    public bool ShouldPlaySound { get; set; }

    public enum NotificationTypes
    {
      Success,
      Warning,
      Error,
      Information,
      Custom
    }

    [DesktomatonProperty(PrettyTitle = "Notification TYpe")]
    public NotificationTypes NotificationType { get; set; } = NotificationTypes.Custom;

    public override async Task RunAsync(uint? SuggestedExpiry, string SuggestedStatus)
    {
      Notifier notifier = null;

      await Application.Current.Dispatcher.InvokeAsync(() =>
      {
        notifier = new Notifier(cfg =>
        {
          cfg.DisplayOptions.Width = 350;

          cfg.PositionProvider = new PrimaryScreenPositionProvider(
              corner: Corner.BottomRight,
              offsetX: 10,
              offsetY: 10);

          cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
              notificationLifetime: TimeSpan.FromSeconds(300),
              maximumNotificationCount: MaximumNotificationCount.FromCount(5));

          cfg.Dispatcher = Application.Current.Dispatcher;
        });
      });

      // even though we might not use it, it's useful to have this done off the UI thread
      var notificationData = new NotificationData(Message)
      {
        Icon = Icon,
        Title = Title
      };

      _ = Application.Current.Dispatcher.InvokeAsync(() =>
      {
        switch (NotificationType)
        {
          case NotificationTypes.Information:
            notifier.ShowInformation(Message);
            break;

          case NotificationTypes.Success:
            notifier.ShowSuccess(Message);
            break;

          case NotificationTypes.Warning:
            notifier.ShowWarning(Message);
            break;

          case NotificationTypes.Error:
            notifier.ShowError(Message);
            break;

          case NotificationTypes.Custom:
            notifier.Notify(() => notificationData);
            break;

          default:

            if (Debugger.IsAttached)
              throw new ArgumentException($"{nameof(NotificationType)} is invalid and has hit the Default case. Value is: {NotificationType}");

            Debug.WriteLine($"{nameof(NotificationType)} is invalid and has hit the Default case. Value is: {NotificationType}");

            break;
        }

        if (ShouldPlaySound)
          PlaySound();
      });
    }

    private static void PlaySound()
    {
      var player = new MediaPlayer();

      var soundPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Sounds\234524__foolboymedia__notification-up-1.wav");
      var soundUri = new Uri(soundPath);

      player.Open(soundUri);
      player.Play();
    }
  }
}
