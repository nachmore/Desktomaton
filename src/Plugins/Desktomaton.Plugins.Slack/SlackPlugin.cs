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
    DND
  }

  public class SlackPlugin : IDesktomatonAction
  {

    public List<IPluginProperty> Properties { get; } = new List<IPluginProperty>()
    {
      new PluginProperty<string>("Slack Token"),
      new PluginProperty<SlackAction>("Action"),
      new PluginProperty<string>("Action Parameter")
    };

    public string Name => "Slack";

    public async Task RunAsync()
    {
      var slackClient = new SlackTaskClient((string)Properties[0].GetValue());

      var requestParams = new List<Tuple<string, string>>();

      requestParams.Add(new Tuple<string, string>("status_text", "this fsfdis a status"));
      requestParams.Add(new Tuple<string, string>("status_emoji", ":mario:"));
      requestParams.Add(new Tuple<string, string>("status_expiration", "0"));

      var test =
      new {
        status_text = "this is a status",
        status_emoji = ":mario:",
        status_expiration = DateTimeOffset.Now.ToUnixTimeSeconds() + (60 * 10)
      };

      var profile = new Tuple<string, string>("profile", JsonConvert.SerializeObject(test));

      var response = await slackClient.APIRequestWithTokenAsync<ProfileSetResponse>(profile);

      Debug.WriteLine("SlackPlugin: Run()");
    }
  }

  [RequestPath("users.profile.set")]
  internal class ProfileSetResponse : Response
  {
  }
}
