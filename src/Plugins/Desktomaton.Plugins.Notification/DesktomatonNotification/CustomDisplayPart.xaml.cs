using Desktomaton.Plugins.Notification.DesktomatonNotification;
using System.Windows;
using ToastNotifications.Core;

namespace Desktomaton.Plugins.Notification.DesktomatonNotification
{
  /// <summary>
  /// Interaction logic for CustomNotification.xaml
  /// </summary>
  internal partial class CustomDisplayPart : NotificationDisplayPart
  {
    public CustomDisplayPart(NotificationData data)
    {
      InitializeComponent();
      Bind(data);
    }

    private void CloseClicked(object sender, RoutedEventArgs e)
    {
      Notification.Close();
    }
  }
}
