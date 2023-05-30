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

    [DesktomatonProperty(PrettyTitle = "Start Hour")]
    public uint? StartHour { get; set; }

    [DesktomatonProperty(PrettyTitle = "Start Minute")]
    public uint? StartMinute { get; set; }

    [DesktomatonProperty(PrettyTitle = "End Hour")]
    public uint? EndHour { get; set; }

    [DesktomatonProperty(PrettyTitle = "End Minute")]
    public uint? EndMinute { get; set; }

    [DesktomatonProperty(PrettyTitle = "Days of Week")]
    public List<DayOfWeek> DaysOfWeek { get; set; }

    public override async Task<bool> EvaluateAsync()
    {
      var rv = false;

      // the number of properties set, i.e. the number that need to evaluate to true
      // for Evaluate() to return true
      var propertySetCount = GetSetPropertyCount();

      if (propertySetCount == 0)
        throw new ArgumentException("You must set Trigger properties before evaluating the Trigger...");

      if ((StartHour != null && EndHour == null) ||
          (EndHour != null && StartHour == null))
        throw new ArgumentException("Either both or none of Start and End Hour must be set");

      var now = DateTime.Now;

      // we're doing time based trigger
      if (StartHour != null)
      {
        var startHour = (int)StartHour.GetValueOrDefault();
        var startMinute = (int)StartMinute.GetValueOrDefault();
        var endHour = (int)EndHour.GetValueOrDefault();
        var endMinute = (int)EndMinute.GetValueOrDefault();

        if (!DaysOfWeek?.Contains(now.DayOfWeek) == false)
          return rv;

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

        rv = (now > start && now < end);

        // if this trigger is set, suggest an expiry time (+1 to account for left over seconds)
        if (rv)
          SuggestedExpiry = (uint)(end.Subtract(now).TotalMinutes + 1);
      }
      else
      {
        rv = (DaysOfWeek?.Contains(now.DayOfWeek) == true);

        // calculate time till midnight tomorrow (+1 to account for left over seconds)
        if (rv) {
          var tomorrow = now.Date.AddDays(1);
          SuggestedExpiry = (uint)(tomorrow.Subtract(now).TotalMinutes + 1);
        }
      }

      return rv;
    }
  }
}
