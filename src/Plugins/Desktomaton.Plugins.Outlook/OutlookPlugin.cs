using Desktomaton.PluginBase;
using OutlookApp = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Desktomaton.Plugins.Outlook
{

  public class OutlookPlugin : IDesktomatonTrigger
  {
    public OutlookPlugin()
    {
      Debug.WriteLine("OutlookPlugin() created");
    }
    
    public string Name => "Outlook";

    private enum PropertyIndexes
    {
      Subject = 0,
      BusyStatus,
      Category
    }

    public List<IPluginProperty> Properties { get; }  = new List<IPluginProperty>()
    {
      new PluginProperty<string>("Subject"),
      new PluginProperty<OutlookApp.OlBusyStatus?>("Busy Status"),
      new PluginProperty<string>("Category"),
    };

    public async Task<bool> EvaluteAsync()
    {
      Debug.WriteLine("OutlookPlugin: Evaluate()");

      // get properties
      // TODO: need to make this easier
      var propSubject = ((PluginProperty<string>)(Properties[(int)PropertyIndexes.Subject])).Value?.ToLower();
      var propBusyStatus = ((PluginProperty<OutlookApp.OlBusyStatus?>)(Properties[(int)PropertyIndexes.BusyStatus])).Value;
      var propCategory = ((PluginProperty<string>)(Properties[(int)PropertyIndexes.Category])).Value?.ToLower();

      // the number of properties set, i.e. the number that need to evaluate to true
      // for Evaluate() to return true
      var propertySetCount = (propSubject != null ? 1 : 0) + (propBusyStatus != null ? 1 : 0) + (propCategory != null ? 1 : 0);

      if (propertySetCount == 0)
        throw new ArgumentException("You must set Trigger properties before evaluating the Trigger...");

      var calendars = GetCalendars();

      // get current appointments
      foreach (var calendar in calendars)
      {
        var curAppointments = GetCurrentAppointments(calendar);

        foreach (var appointment in curAppointments)
        {
          var count = 0;

          if (propSubject != null && appointment.Subject.ToLower().Contains(propSubject))
            count++;

          if (propBusyStatus != null && appointment.BusyStatus == propBusyStatus)
            count++;

          if (propCategory != null && appointment.Categories != null && appointment.Categories.ToLower().Contains(propCategory))
            count++;

          if (count == propertySetCount)
            return true;
        }
      }

      return false;
    }

    public static List<OutlookApp.Folder> GetCalendars()
    {
      var outlook = new OutlookApp.Application();
      var stores = outlook.Session.Stores;

      var folders = new List<OutlookApp.Folder>();

      foreach (OutlookApp.Store store in stores)
      {
        try
        {

          var folder = (OutlookApp.Folder)store.GetDefaultFolder(OutlookApp.OlDefaultFolders.olFolderCalendar);
          System.Diagnostics.Debug.WriteLine(folder.Name);

          folders.Add(folder);
        }
        catch (Exception e)
        {
          // Not every root folder has a calendar (for example, Public folders), so this exception can be ignored
          Debug.WriteLine("Failed to get Calendar:\n" + e);
        }
      }

      if (folders.Count > 0)
        return folders;

      throw new InvalidOperationException("Couldn't find a Calendar in the current Outlook installation");
    }

    public static List<OutlookApp.AppointmentItem> GetCurrentAppointments(OutlookApp.Folder calendar)
    {
      var now = DateTime.Now;

      return GetAppointmentsInRange(calendar, now, now.AddMinutes(1));
    }

    public static List<OutlookApp.AppointmentItem> GetAppointmentsInRange(OutlookApp.Folder folder, DateTime start, DateTime end, bool includeRecurrences = true)
    {
      // we originally combined the filters with an "OR" but it would seemingly miss some random meetings for no apparent reason
      // explored whether or not there was API caching but couldn't find any. Splitting it out into two queries seems to solve the 
      // issue, even if the code is a little more weird.

      // find all meetings that start within the period (regardless of when they end specifically)
      var filter = $"[Start] >= '{start.ToString("g")}' AND [Start] <= '{end.ToString("g")}'";

      var rv = RestrictItems(folder, filter, includeRecurrences);

      // find meetings that are in-progress during the period (for example, all day events that start before the period but end
      // during or after it
      filter = $"[Start] < '{start.ToString("g")}' AND [End] >= '{start.ToString("g")}'";

      rv.AddRange(RestrictItems(folder, filter, includeRecurrences));

      return rv;
    }

    public static List<OutlookApp.AppointmentItem> RestrictItems(OutlookApp.Folder folder, string filter, bool includeRecurrences)
    {
      var rv = new List<OutlookApp.AppointmentItem>();

      var items = folder.Items;
      items.IncludeRecurrences = includeRecurrences;
      items.Sort("[Start]", Type.Missing);

      var restricted = items.Restrict(filter);

      foreach (OutlookApp.AppointmentItem item in restricted)
      {
        rv.Add(item);

        Debug.WriteLine("++: " + item.Start + " -> " + item.End + ": " + item.Subject);
      }

      return rv;
    }


  }
}
