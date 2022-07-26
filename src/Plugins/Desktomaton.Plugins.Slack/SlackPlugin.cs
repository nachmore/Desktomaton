using Desktomaton.PluginBase;
using Newtonsoft.Json;
using SlackAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Desktomaton.Plugins.Slack
{
  public enum SlackAuthTypes
  {
    OAuth,
    Browser
  }

  public enum SlackAction
  {
    Status,
    DND,
    Available
  }

  [Serializable]
  public class SlackPlugin : DesktomatonAction
  {
    private const uint DEFAULT_EXPIRATION = 5;

    public override string Name => "Slack";

    [DesktomatonProperty(PrettyTitle="Slack Authentication Type")]
    public SlackAuthTypes AuthType { get; set; }

    [DesktomatonProperty(PrettyTitle = "Slack Token")]
    public string SlackToken { get; set; }

    [DesktomatonProperty]
    public SlackAction? Action { get; set; }

    [DesktomatonProperty(PrettyTitle = "Action Parameter")]
    public string ActionParameter { get; set; }

    [DesktomatonProperty]
    public uint? Expiration { get; set; }

    // for now we're cheating. What should happen is that auth should be a configurable type within PluginManagement
    // and you should be able to set a default for all plugins of a certain type, or a specific one per plugin instance
    private static SlackAuth _auth = new SlackBrowserAuth();

    public override async Task RunAsync(uint? SuggestedExpiry)
    {

      var expiration = Expiration ?? SuggestedExpiry ?? DEFAULT_EXPIRATION;

      _auth.Retrieve();

      SlackToken = _auth.Token;

      var slackClient = new SlackTaskClient(SlackToken, _auth.Cookies);
      
      switch (Action)
      {
        case null:
          throw new ArgumentNullException("Action property was not set!");
        case SlackAction.DND:
          await SetDnd(slackClient, expiration);
          break;
        case SlackAction.Status:
          await SetStatus(slackClient, ActionParameter, expiration);
          break;
        default:
          throw new NotImplementedException($"Unimplemented action {Action}");
      }

      Debug.WriteLine("SlackPlugin: Run()");
    }

    private async Task SetStatus(SlackTaskClient slackClient, string param, uint? expiration)
    {
      var emoji = "";
      var text = param;

      if (param.StartsWith(":"))
      {
        var emoji_index = param.IndexOf(":", 1);
        emoji = param.Substring(0, emoji_index + 1);

        text = param.Substring(emoji_index + 1);
      }

      var profile_parameters =
        new
        {
          status_text = text,
          status_emoji = emoji,
          status_expiration = (expiration > 0 ? DateTimeOffset.Now.ToUnixTimeSeconds() + (60 * expiration) : 0)
        };

      var profile = new Tuple<string, string>("profile", JsonConvert.SerializeObject(profile_parameters));
      
      var response = await slackClient.APIRequestWithTokenAsync<ProfileSetResponse>(profile);

      if (!response.ok)
      {
        Debug.WriteLine($"** Slack Request Failed: {response.error}\n\tProfile is: {profile_parameters}");
      }

    }

    //TODO: Do we need a way to end Dnd? Either with expiration 0 or via dnd.endSnooze
    private async Task SetDnd(SlackTaskClient slackClient, uint? expiration)
    {
      // unlike status, expiration == 0 means expire now, so override this with a small default
      if (expiration.GetValueOrDefault() == 0)
        expiration = DEFAULT_EXPIRATION;

      var param = new Tuple<string, string>("num_minutes", expiration.ToString());

      var response = await slackClient.APIRequestWithTokenAsync<DndSetSnoozeResponse>(param);

      if (!response.ok)
      {
        Debug.WriteLine($"** Slack Request Failed: {response.error}\n\tExpiration is: {expiration}");
      }
    }
  }

}
