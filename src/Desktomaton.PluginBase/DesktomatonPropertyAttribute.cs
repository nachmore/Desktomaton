using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Desktomaton.PluginBase
{
  [System.AttributeUsage(AttributeTargets.Property)]
  public class DesktomatonPropertyAttribute : Attribute
  {
    private string _callerName;

    private string _prettyTitle;
    public string PrettyTitle 
    {
      get
      {
        // ensure that Pretty Title is always set to something
        if (string.IsNullOrEmpty(_prettyTitle))
        {
          return _callerName;
        }

        return _prettyTitle;
      }
      set { _prettyTitle = value; }
    }

    /// <summary>
    /// Whether or not this property needs to be true for the trigger to be considered successful
    /// </summary>
    public bool CountsTowardsTrigger { get; set; } = true;

    public DesktomatonPropertyAttribute([CallerMemberName] string callerName = null)
    {
      _callerName = callerName;
    }
  }
}
