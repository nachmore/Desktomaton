using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Windows;


namespace Desktomaton.Plugins.Slack
{
  public partial class SlackOauthWindow : Window
  {
    public SlackOauthWindow()
    {
      InitializeComponent();
    }

    // https://api.slack.com/authentication/oauth-v2
    // https://api.slack.com/methods/oauth.v2.access
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      // https://slack.com/oauth/v2/authorize?scope=incoming-webhook,commands&client_id=3336676.569200954261

      var auth_url_format = "https://slack.com/oauth/v2/authorize?client_id={0}&user_scope={1}";
        //"https://slack.com/oauth?client_id={0}&scope={1}&redirect_uri=&state=&granular_bot_scope=0&single_channel=0&install_redirect=&tracked=1&team=";

      //var slackClientHelpers = new SlackClientHelpers();
      var clientId = "---replace---";
      var uri = string.Format(auth_url_format, clientId, "dnd:read,dnd:write,users.profile:read,users.profile:write");

      await browser.EnsureCoreWebView2Async();
      
      browser.CoreWebView2.Navigate(uri);
    }

    private void browser_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
    {
      System.Diagnostics.Debug.WriteLine(browser.Source);

      if (browser.Source.ToString().StartsWith("https://localhost/"))
      {
        GetAuthToken(browser.Source.ToString());
      }

    }

    public event EventHandler<string> TemporaryTokenRetrieved;
        
    protected virtual void OnTemporaryTokenRetrieved(string token)
    {
      TemporaryTokenRetrieved?.Invoke(this, token);
    }

    private async void GetAuthToken(string url)
    {
      //https://localhost/?code=2293233160849.3468125840998.0ce55798339ba0f31136a64aa803c17ac14f012d74871963872a040690f3f30f&state=

      // extract code from url
      var code = url.Substring(url.IndexOf("=") + 1, url.Length - (url.IndexOf("=") + 1) - (url.Length - url.IndexOf("&")));
      System.Diagnostics.Debug.WriteLine(code);

      var httpClient = new HttpClient();

      var requestContent = new FormUrlEncodedContent(new[]
      {
        new KeyValuePair<string, string>("code", code),
        new KeyValuePair<string, string>("client_id", "----replace----"),
        new KeyValuePair<string, string>("client_secret", "---replace----"),
      });

      var response = await httpClient.PostAsync("https://slack.com/api/oauth.v2.access", requestContent);

      System.Diagnostics.Debug.WriteLine(response.Content);

      using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
      {
        var output = await reader.ReadToEndAsync();
        // Write the output.
        System.Diagnostics.Debug.WriteLine(output);
      }

      //OnTemporaryTokenRetrieved(code);

      // curl -F code=1234 -F client_id=3336676.569200954261 -F client_secret=ABCDEFGH https://slack.com/api/oauth.v2.access

    }
  }
}
