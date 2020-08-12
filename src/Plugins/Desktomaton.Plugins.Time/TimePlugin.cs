using Desktomaton.PluginBase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Desktomaton.Plugins.Time
{
  [Serializable]
  public class TimePlugin : DesktomatonTrigger
  {
    public override string Name => "⌚ Time";

    public override List<IPluginProperty> Properties { get; } = new List<IPluginProperty>()
    {
      new PluginProperty<uint>("Start Hour"),
      new PluginProperty<uint>("Start Minute"),
      new PluginProperty<uint>("End Hour"),
      new PluginProperty<uint>("End Minute"),
      new PluginProperty<List<DayOfWeek>>("Days of Week")
    };

    public override async Task<bool> EvaluateAsync()
    {
      // the number of properties set, i.e. the number that need to evaluate to true
      // for Evaluate() to return true
      var propertySetCount = GetSetPropertyCount();

      if (propertySetCount == 0)
        throw new ArgumentException("You must set Trigger properties before evaluating the Trigger...");

      var startHourProperty = Properties[0];
      var startMinuteProperty = Properties[1];
      var endHourProperty = Properties[2];
      var endMinuteProperty = Properties[2];
      var daysOfWeekProperty = Properties[4];

      if ((startHourProperty.IsSet && !endHourProperty.IsSet) ||
          (endHourProperty.IsSet && !startHourProperty.IsSet))
        throw new ArgumentException("Either both or none of Start and End Hour must be set");

      var now = DateTime.Now;
      var daysOfWeek = daysOfWeekProperty.GetValue() as List<DayOfWeek>;

      // we're doing time based trigger
      if (startHourProperty.IsSet)
      {
        // safe to direct cast since these are either set or we'll get the default (0)
        // double cast because can't cast directly to int due to T being uint (it's uint
        // for UX enforcement to avoid negatives)
        var startHour = (int)(uint)startHourProperty.GetValue();
        var startMinute = (int)(uint)startMinuteProperty.GetValue();
        var endHour = (int)(uint)endHourProperty.GetValue();
        var endMinute = (int)(uint)endMinuteProperty.GetValue();

        if (!daysOfWeek?.Contains(now.DayOfWeek) == false)
          return false;

        var start = new DateTime(now.Year, now.Month, now.Day, startHour, startMinute, 0);
        var end = new DateTime(now.Year, now.Month, now.Day, endHour, endMinute, 0);

        // range starts on day 1 and finishes in day 2
        if (startHour > endHour)
          if (endHour > now.Hour)
            // if we've already switched into day 2, then evaluate from yesterday to now
            start = start.AddDays(-1);
          else 
            // otherwise evaluate from today to tomorrow
            end = end.AddDays(1);

        return (now > start && now < end);
      } 
      else
      {
        if (daysOfWeek?.Contains(now.DayOfWeek) == true)
          return true;
      }

      return false;
    }
  }
}
