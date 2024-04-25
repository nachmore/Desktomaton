using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Desktomaton.Plugins.Slack
{
  public class SlackBrowserAuth : SlackAuth
  {
    private readonly object _lock = new();
    private readonly ManualResetEvent _resetEvent = new(true);

    private const string _COOKIE_FILE_NAME = "desktomaton.cookies";
    private const string _TOKEN_FILE_NAME = "desktomaton.token";

    public SlackBrowserAuth()
    {
      _cacheCookiesFileName = _COOKIE_FILE_NAME;
      _cacheTokenFileName = _TOKEN_FILE_NAME;
    }

    override public async Task<bool> Retrieve()
    {
      // if we're running on the UI thread then we risk blocking the UI thread
      // (this can happen if we hit this function via a UI feature), so spawn ourselves on a background thread
      if (SynchronizationContext.Current != null)
      {
        return await Task.Run(() => { return Retrieve(); });
      }

      // check for token null to avoid locking for no reason
      if (Token == null)
      {
        // since we start signalled, multiple threads can (and will) call WaitOne and not block,
        // so make sure that we only run this synchronized
        lock (_lock)
        {
          _resetEvent.WaitOne();
          _resetEvent.Reset();
        }

        if (LoadFromCache())
        {
          _resetEvent.Set();
        } 
        else
        {
          Application.Current.Dispatcher.Invoke(() =>
          {
            var browserTokenWindow = new SlackBrowserAuthWindow();
            browserTokenWindow.AuthRetrieved += BrowserTokenWindow_AuthRetrieved;
            browserTokenWindow.Show();
          });

          // wait for creds to be populated
          _resetEvent.WaitOne();
        }
      }

      return Token != null;
    }

    private void BrowserTokenWindow_AuthRetrieved(object sender, AuthRetrievedArgs e)
    {
      {
        Token = e.Token;
        Cookies = e.Cookies;

        _resetEvent.Set();
      }
    }
  }
}