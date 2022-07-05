using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace Desktomaton.Plugins.Slack
{
  public abstract class SlackAuth
  {
    internal string _cacheCookiesFileName;
    internal string _cacheTokenFileName;

    private string _token;
    public string Token
    {
      get => _token;
      internal set
      {
        _token = value;

        if (UseCache)
          CacheToken();
      }
    }

    private CookieContainer _cookies;
    public CookieContainer Cookies
    {
      get => _cookies;
      internal set
      {
        _cookies = value;

        if (UseCache)
          CacheCookies();
      }
    }

    public bool UseCache { get; set; } = true;
    public string CachePath { get; set; } = Path.GetTempPath();

    abstract public bool Retrieve();

    internal bool LoadFromCache()
    {
      var cookieFile = Path.Combine(CachePath, _cacheCookiesFileName);
      var tokenFile = Path.Combine(CachePath, _cacheTokenFileName);

      if (Token == null && File.Exists(cookieFile) && File.Exists(tokenFile))
      {
        try
        {
          var cookieContainer = new CookieContainer();
          var jsonSerializer = new JsonSerializer();

          using (var streamReader = new StreamReader(cookieFile))
          using (var jsonReader = new JsonTextReader(streamReader))
          {
            // ideal code:
            // var cookieCollection = jsonSerializer.Deserialize<CookieCollection>(jsonReader);
            // cookieContainer.Add(cookieCollection);

            // we need to set the correct timestamp, which Cookie doesn't allow. So cheat.
            // CookieCollection serializes down to the same thing as a List<Cookie>
            var fakeCookieCollection = jsonSerializer.Deserialize<List<TimeStampableCookie>>(jsonReader);

            var cookieCollection = new CookieCollection();

            foreach (var rawCookie in fakeCookieCollection)
            {
              var cookie = rawCookie.ToCookie();
              cookieCollection.Add(cookie);
            }

            cookieContainer.Add(cookieCollection);
          }

          Cookies = cookieContainer;
          Token = File.ReadAllText(tokenFile);
        }
        catch (Exception e)
        {
          Cookies = null;
          Token = null;
          System.Diagnostics.Debug.WriteLine(e);
        }
      }

      return Token != null;
    }

    private void CacheToken()
    {
      var tokenFile = Path.Combine(CachePath, _cacheTokenFileName);

      File.WriteAllText(tokenFile, Token);
    }

    private void CacheCookies()
    {
      var cookieFile = Path.Combine(CachePath, _cacheCookiesFileName);

      var serializer = new JsonSerializer();

      using (StreamWriter sw = new StreamWriter(cookieFile))
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, Cookies.GetAllCookies());
      }
    }
  }
}