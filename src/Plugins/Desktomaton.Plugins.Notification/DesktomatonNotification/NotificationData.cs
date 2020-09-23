using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using ToastNotifications.Core;

namespace Desktomaton.Plugins.Notification.DesktomatonNotification
{
  // Not using INotifyPropertyChanged since this class is recreated for each notification
  internal class NotificationData : NotificationBase
  {
    private CustomDisplayPart _displayPart;
    public override NotificationDisplayPart DisplayPart => _displayPart ??= new CustomDisplayPart(this);

    public string Icon { get; set; }
    public string Title { get; set; }

    public NotificationData(string message) : this(message, new MessageOptions())
    {
    }

    public NotificationData(string message, MessageOptions options) : base(message, options)
    {
    }
  }
}
