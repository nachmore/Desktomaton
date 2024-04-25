using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace Desktomaton.Plugins.Slack
{
  /// <summary>
  /// Interaction logic for SlackBrowserAuth.xaml
  /// </summary>
  public partial class SlackBrowserAuthWindow : Window
  {

    private string _token = null;
    private CookieContainer _cookieContainer = null;

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

      if (!request.Uri.Contains("edgeapi.") && request.Content != null)
      {
        var contentStream = request.Content;

        var reader = new StreamReader(contentStream);

        var content = reader.ReadToEnd();

        if (content.Contains("xoxc"))
        {
          browser.CoreWebView2.WebResourceRequested -= CoreWebView2_WebResourceRequested;

          // tokens are 111 spaces:
          // xoxc-1234567890987-1234567890123-1234567890123-1234567890987654321234567890987654321234567890987654321234567890
          _token = content.Substring(content.IndexOf("xoxc"), 111);

          var cookies = await browser.CoreWebView2.CookieManager.GetCookiesAsync("");

          _cookieContainer = new CookieContainer();

          foreach (var cookie in cookies)
          {
            if (cookie.Domain.EndsWith("slack.com"))
            {
              _cookieContainer.Add(ConvertCookie(cookie));
            }
          }

          browser.Stop();
          Close();
        }
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      // if the window is closed manually we'll still invoke the callback so that it doesn't block forever,
      // it will just be called with nulls
      OnAuthRetrieved(_token, _cookieContainer);
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
