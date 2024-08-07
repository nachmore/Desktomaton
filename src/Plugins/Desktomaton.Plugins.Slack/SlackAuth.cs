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
using Desktomaton.Logger;

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

        if (_token != null && UseCache)
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

        if (_cookies != null && UseCache)
          CacheCookies();
      }
    }

    public bool UseCache { get; set; } = true;
    public string CachePath { get; set; } = Path.GetTempPath();

    private bool RefreshCache { get; set; } = false;

    abstract public Task<bool> Retrieve();

    internal bool LoadFromCache()
    {
      // we've been asked to refresh the cache, so return a fail so that the cache
      // is recreated
      if (RefreshCache)
      {
        RefreshCache = false;
        return false;
      }

      var cookieFile = Path.Combine(CachePath, _cacheCookiesFileName);
      var tokenFile = Path.Combine(CachePath, _cacheTokenFileName);

      if (Token == null && File.Exists(tokenFile) && File.Exists(cookieFile))
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
          Log.WriteLine(e);
        }
      }

      return Token != null;
    }

    public void ClearCache()
    {
      // don't actually clear the cache file, just signal to fail loading so that the cache is refreshed
      RefreshCache = true;

      // do clear cached values
      Token = null;
      Cookies = null;
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