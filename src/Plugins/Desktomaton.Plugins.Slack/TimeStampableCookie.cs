using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Desktomaton.Plugins.Slack
{
  /// <summary>
  /// A regular Cookie class (https://docs.microsoft.com/en-us/dotnet/api/system.net.cookie?view=net-6.0) 
  /// that can be used for caching cookies, since you can update the TimeStamp to match the original 
  /// cookie creation time.
  /// </summary>
  class TimeStampableCookie
  {
    public string Comment { get; set; }
    public Uri CommentUri { get; set; }
    public bool HttpOnly { get; set; }
    public bool Discard { get; set; }
    public string Domain { get; set; }
    public bool Expired { get; set; }
    public DateTime Expires { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string Port { get; set; }
    public bool Secure { get; set; }
    public DateTime TimeStamp { get; set; }
    public string Value { get; set; }
    public int Version { get; set; }

    public Cookie ToCookie()
    {
      var cookie = new Cookie(Name, Value, Path, Domain)
      {
        Expires = Expires,
        Comment = Comment,
        CommentUri = CommentUri,
        Discard = Discard,
        Secure = Secure,
        Version = Version,
        HttpOnly = HttpOnly
      };

      // explicitly setting port to "" restricts ports, which is unintended
      // so only set it if the value is actually set
      // see https://docs.microsoft.com/en-us/dotnet/api/system.net.cookie.port?view=net-6.0#remarks
      if (!string.IsNullOrWhiteSpace(Port))
      {
        cookie.Port = Port;
      }

      // ok, yup, this is disgusting. Cookie doesn't let you set the creation timestamp (it uses
      // object creation time). This prevents serializing and then deserializing some cookies that
      // use the creation time as a validation mechanism.
      //
      // To work around this, since we can't subclass Cookie, we overwrite the internal structures
      // using reflection. 🤮
      var _timeStampField = cookie.GetType().GetRuntimeFields().Where(a => a.Name == "m_timeStamp").FirstOrDefault();
      _timeStampField.SetValue(cookie, TimeStamp);

      return cookie;
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.AppendLine($"{Name}");
      sb.AppendLine($"\tValue    : {Value}");
      sb.AppendLine($"\tComment  : {Comment}");
      sb.AppendLine($"\tVersion  : {Version}");
      sb.AppendLine($"\tExpires  : {Expires}");
      sb.AppendLine($"\tExpired  : {Expired}");
      sb.AppendLine($"\tTimeStamp: {TimeStamp}");
      sb.AppendLine($"\tDomain   : {Domain}");
      sb.AppendLine($"\tPath     : {Path}");
      sb.AppendLine($"\tPort     : {Port}");
      sb.AppendLine($"\tDiscard  : {Discard}");
      sb.AppendLine($"\tHttpOnly : {HttpOnly}");
      sb.AppendLine($"\tSecure   : {Secure}");

      return sb.ToString();
    }
  }
}