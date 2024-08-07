using Desktomaton.Logger;
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

    public override async Task RunAsync(uint? SuggestedExpiry, string SuggestedStatus)
    {
      var expiration = Expiration ?? SuggestedExpiry ?? DEFAULT_EXPIRATION;

      await _auth.Retrieve();

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

          var status = SuggestedStatus ?? ActionParameter;

          await SetStatus(slackClient, status, expiration);
          break;
        default:
          throw new NotImplementedException($"Unimplemented action {Action}");
      }

      Log.WriteLine("SlackPlugin: Run()");
    }

    private async Task SetStatus(SlackTaskClient slackClient, string param, uint? expiration)
    {
      var emoji = "";
      var text = param.Trim();

      if (text.StartsWith(":"))
      {
        var emoji_index = text.IndexOf(":", 1);
        emoji = text.Substring(0, emoji_index + 1);

        text = text.Substring(emoji_index + 1).Trim();
      }

      var profile_parameters =
        new
        {
          status_text = text,
          status_emoji = emoji,
          status_expiration = (expiration > 0 ? DateTimeOffset.Now.ToUnixTimeSeconds() + (60 * expiration) : 0)
        };

      var profile = new Tuple<string, string>("profile", JsonConvert.SerializeObject(profile_parameters));

      try
      {
        var response = await SlackAPIRequestAsync<ProfileSetResponse>(slackClient, profile);

        if (!response.ok)
        {
          Log.WriteLine($"** Slack Request Failed: {response.error}\n\tProfile is: {profile_parameters}");
        }
      } 
      catch (NullReferenceException)
      {
        Log.WriteLine("Failed to call Slack - likely due to lack of internet (or Slack is down) - (NullReferenceException)");
      }
      catch (Exception e)
      {
        Log.WriteLine($"Failed to call Slack due to an unexpected exception: {e})");
      }
    }

    //TODO: Do we need a way to end Dnd? Either with expiration 0 or via dnd.endSnooze
    private async Task SetDnd(SlackTaskClient slackClient, uint? expiration)
    {
      // unlike status, expiration == 0 means expire now, so override this with a small default
      if (expiration.GetValueOrDefault() == 0)
        expiration = DEFAULT_EXPIRATION;

      var param = new Tuple<string, string>("num_minutes", expiration.ToString());

      var response = await SlackAPIRequestAsync<DndSetSnoozeResponse>(slackClient, param);

      if (!response.ok)
      {
        Log.WriteLine($"** Slack Request Failed: {response.error}\n\tExpiration is: {expiration}");
      }
    }

    private async Task<T> SlackAPIRequestAsync<T>(SlackTaskClient slackClient, Tuple<string, string> param) where T : SlackAPI.Response, new()
    {
      var response = new T() { ok = false };

      try {
        response = await slackClient.APIRequestWithTokenAsync<T>(param);

        if (!response.ok && response.error == "invalid_auth")
        {
          _auth.ClearCache();
        }

        return response;
      } 
      catch (NullReferenceException e)
      {
        Log.WriteLine($"Failed to call Slack - likely due to lack of internet (or Slack is down). Type: {typeof(T).Name}\nParameters: {param}\nException: {e}");
      }
      catch (Exception e)
      {
        Log.WriteLine($"Failed to call Slack due to an unexpected exception. Type: {typeof(T).Name}\nParameters: {param}\nException: {e}");
      }

      return response;
    }
  }

}
