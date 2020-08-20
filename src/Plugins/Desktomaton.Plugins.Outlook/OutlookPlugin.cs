﻿using Desktomaton.PluginBase;
using OutlookApp = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Desktomaton.Plugins.Outlook
{

  [Serializable]
  public class OutlookPlugin : DesktomatonTrigger
  {
    /// <summary>
    /// Amount of time to cache appointment data for, allowing for consequtive triggers
    /// </summary>
    private const int CACHE_TIMEOUT_MIN = 2;

    private static List<OutlookApp.AppointmentItem> _appointmentCache;
    private static DateTime _appointmentCacheUpdateTime;

    public OutlookPlugin()
    {
      Debug.WriteLine("OutlookPlugin() created");
    }

    public override string Name => "📅 Outlook";

    private enum PropertyIndexes
    {
      Subject = 0,
      BusyStatus,
      Category
    }

    [DesktomatonProperty]
    public List<string> Subject { get; set; }

    [DesktomatonProperty(PrettyTitle = "Busy Status")]
    public OutlookApp.OlBusyStatus? BusyStatus { get; set; }

    [DesktomatonProperty]
    public string Category { get; set; }

    public bool Test { get; set; }

    public override async Task<bool> EvaluateAsync()
    {
      Debug.WriteLine("OutlookPlugin: Evaluate()");

      // the number of properties set, i.e. the number that need to evaluate to true
      // for Evaluate() to return true
      var propertySetCount = GetSetPropertyCount();

      // ensure that propSubject is initialized and lowercase
      if (Subject != null)
      {
        Subject = Subject.ConvertAll(i => i.ToLower());
      }

      if (propertySetCount == 0)
        throw new ArgumentException("You must set Trigger properties before evaluating the Trigger...");

      var appointments = GetAppointments();

      foreach (var appointment in appointments)
      {
        var count = 0;

        if (Subject != null)
        {
          foreach (var subject in Subject)
          {
            if (appointment.Subject.ToLower().Contains(subject))
            {
              count++;
            }
          }
        }

        if (BusyStatus != null && appointment.BusyStatus == BusyStatus)
          count++;

        if (Category != null && appointment.Categories != null && appointment.Categories.Contains(Category))
          count++;

        if (count == propertySetCount)
          return true;
      }

      return false;
    }

    private List<OutlookApp.AppointmentItem> GetAppointments()
    {
      if (DateTime.Now.Subtract(_appointmentCacheUpdateTime).TotalMinutes < CACHE_TIMEOUT_MIN)
        return _appointmentCache;

      var appointments = new List<OutlookApp.AppointmentItem>();
      var calendars = GetCalendars();

      foreach (var calendar in calendars)
      {
        appointments.AddRange(GetCurrentAppointments(calendar));
      }

      _appointmentCache = appointments;
      _appointmentCacheUpdateTime = DateTime.Now;

      return appointments;
    }

    public static List<OutlookApp.Folder> GetCalendars()
    {
      var outlook = new OutlookApp.Application();
      var stores = outlook.Session.Stores;

      var folders = new List<OutlookApp.Folder>();

      Debug.WriteLine($"***Store Count {stores.Count}");

      // foreach on COM objects can sometimes get into weird states when encountering
      // a corrupt pst,  where null objects repeat themselves, and a foreach goes into
      // an infinite loop, so prefer traditional for
      for (int i = 0; i < stores.Count; i++)
      {
        OutlookApp.Store store = null;
        
        try
        {
          // this is in the try since sometimes COM will freak out and throw
          // IndexOutOfRangeException even though we're < Count (corrupt pst situation)
          store = stores[i];

          // amazingly this is possible...
          if (store == null)
            continue;

          // ignore public folders (causes slow Exchange calls, and we don't have a use case
          // for interactions with those)
          if (store.ExchangeStoreType == OutlookApp.OlExchangeStoreType.olExchangePublicFolder)
            continue;

          var folder = (OutlookApp.Folder)store.GetDefaultFolder(OutlookApp.OlDefaultFolders.olFolderCalendar);
          System.Diagnostics.Debug.WriteLine(folder.Name);

          folders.Add(folder);
        }
        catch (Exception e)
        {
          // Not every root folder has a calendar, so this exception can be ignored
          Debug.WriteLine($"Failed to get Calendar for {store?.DisplayName} type: {store?.ExchangeStoreType}:\n{e}");
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
