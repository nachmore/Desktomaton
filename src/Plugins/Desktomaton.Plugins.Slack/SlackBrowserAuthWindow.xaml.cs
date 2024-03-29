﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Desktomaton.Plugins.Slack
{
  /// <summary>
  /// Interaction logic for SlackBrowserAuth.xaml
  /// </summary>
  public partial class SlackBrowserAuthWindow : Window
  {
    public SlackBrowserAuthWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var uri = "https://slack.com/signin";

      await browser.EnsureCoreWebView2Async();

      browser.CoreWebView2.Navigate(uri);
    }

    private void browser_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
    {
      browser.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

      browser.CoreWebView2.AddWebResourceRequestedFilter("*", Microsoft.Web.WebView2.Core.CoreWebView2WebResourceContext.All);
      browser.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
    }

    private async void CoreWebView2_WebResourceRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequestedEventArgs e)
    {
      var request = e.Request;

      Debug.WriteLine(request.Uri);

      if (!request.Uri.Contains("edgeapi.") && !request.Uri.Contains(".enterprise.") && request.Content != null)
      {

        var contentStream = request.Content;

        var reader = new StreamReader(contentStream);

        var content = reader.ReadToEnd();

        if (content.Contains("xoxc"))
        {
          browser.CoreWebView2.WebResourceRequested -= CoreWebView2_WebResourceRequested;

          // tokens are 111 spaces:
          // xoxc-1234567890987-1234567890123-1234567890123-1234567890987654321234567890987654321234567890987654321234567890
          var token = content.Substring(content.IndexOf("xoxc"), 111);

          var cookies = await browser.CoreWebView2.CookieManager.GetCookiesAsync("");

          var cookieContainer = new CookieContainer();

          foreach (var cookie in cookies)
          {
            if (cookie.Domain.EndsWith("slack.com"))
            {
              cookieContainer.Add(ConvertCookie(cookie));
            }
          }

          browser.Stop();
          Close();

          OnAuthRetrieved(token, cookieContainer);
        }
      }
    }

    private async Task Test(string token, CookieContainer cookies)
    {

      var values = new Dictionary<string, string>
            {
                { "token", token },
                { "profile", "{\"status_text\":\"dfs9000-00m\"}" }
            };

      var handler = new HttpClientHandler() { CookieContainer = cookies };
      HttpClient client = new HttpClient(handler);

      var content = new FormUrlEncodedContent(values);
      var response = await client.PostAsync("https://slack.com/api/users.profile.set", content);
      
      var responseString = await response.Content.ReadAsStringAsync();
      Console.WriteLine(responseString);

    }

    private Cookie ConvertCookie(Microsoft.Web.WebView2.Core.CoreWebView2Cookie wv2Cookie)
    {
      var cookie = new Cookie(wv2Cookie.Name, wv2Cookie.Value, wv2Cookie.Path, wv2Cookie.Domain);

      return cookie;
    }

    private void CoreWebView2_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
    {
      e.NewWindow = browser.CoreWebView2;

      browser.CoreWebView2.AddWebResourceRequestedFilter("*", Microsoft.Web.WebView2.Core.CoreWebView2WebResourceContext.All);
      browser.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
    }

    public event EventHandler<AuthRetrievedArgs> AuthRetrieved;

    protected virtual void OnAuthRetrieved(string token, CookieContainer cookies)
    {
      AuthRetrieved?.Invoke(this, new AuthRetrievedArgs(token, cookies));
    }
  }

  public class AuthRetrievedArgs : EventArgs
  {
    public string Token { private set; get; }
    public CookieContainer Cookies { private set; get; }

    public AuthRetrievedArgs(string token, CookieContainer cookies)
    {
      Token = token;
      Cookies = cookies;
    }
  }
}
