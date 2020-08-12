using Desktomaton.PluginBase;
using Newtonsoft.Json;
using SlackAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Desktomaton.Plugins.Slack
{
  public enum SlackAction
  {
    Status,
    DND,
    Available
  }

  public class SlackPlugin : IDesktomatonAction
  {
    private const uint DEFAULT_EXPIRATION = 5;

    public List<IPluginProperty> Properties { get; } = new List<IPluginProperty>()
    {
      new PluginProperty<string>("Slack Token"),
      new PluginProperty<SlackAction>("Action"),
      new PluginProperty<string>("Action Parameter"),
      new PluginProperty<uint>("Expiration")
    };

    public string Name => "Slack";

    public async Task RunAsync()
    {

      var token = Properties[0].GetValue() as string;
      var action = Properties[1].GetValue() as SlackAction?;
      var param = Properties[2].GetValue() as string;
      var expiration = Properties[3].GetValue() as uint?;

      if (expiration == null)
        expiration = DEFAULT_EXPIRATION;

      var slackClient = new SlackTaskClient(token);

      switch(action)
      {
        case null:
          throw new ArgumentNullException("Action property was not set!");
        case SlackAction.DND:
          await SetDnd(slackClient, expiration);
          break;
        case SlackAction.Status:
          await SetStatus(slackClient, param, expiration);
          break;
        default:
          throw new NotImplementedException($"Unimplemented action {action}");
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

      var test =
        new
        {
          status_text = text,
          status_emoji = emoji,
          status_expiration = (expiration > 0 ? DateTimeOffset.Now.ToUnixTimeSeconds() + (60 * expiration) : 0)
        };

      var profile = new Tuple<string, string>("profile", JsonConvert.SerializeObject(test));

      var response = await slackClient.APIRequestWithTokenAsync<ProfileSetResponse>(profile);
    }

    private async Task SetDnd(SlackTaskClient slackClient, uint? expiration)
    {
      // unlike status, expiration == 0 means expire now, so override this with a small default
      if (expiration == 0)
        expiration = DEFAULT_EXPIRATION;

      var param = new Tuple<string, string>("num_minutes", expiration.ToString());

      var response = await slackClient.APIRequestWithTokenAsync<DndSetSnoozeResponse>(param);
    }
  }

  [RequestPath("users.profile.set")]
  internal class ProfileSetResponse : Response
  {
  }


  [RequestPath("dnd.setSnooze")]
  internal class DndSetSnoozeResponse : Response
  {
  }

}
