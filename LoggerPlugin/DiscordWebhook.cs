using Newtonsoft.Json;
using Rocket.Core.Logging;
using Rocket.Core.Utils;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoggerPlugin.Connection
{
    public class DiscordWebhook
    {
        public static async Task SendDiscordWebhookAsync(string playerName, ItemAsset assetInfo, string icon, string playerSteamID, string serverName)
        {
            HttpWebRequest request = WebRequest.CreateHttp("https://discord.com/api/webhooks/1002464072895307776/BD0-Wa3dYZCmq5MsbP8cT6N2Kp5_28_sfSXAa8-mSmGIlyyh4FUl-TiwIyZ0BP2Aarh-");
            request.ContentType = "application/json";
            request.Method = "POST";

            using (var sw = new StreamWriter(await request.GetRequestStreamAsync()))
            {

                string json = JsonConvert.SerializeObject(new
                {
                    username = "BerrysLogger",
                    avatar_url = "https://i.imgur.com/CbL2v5t.jpeg",
                    embeds = new[]
                    {
                    new
                    {
                            description = $"{playerName} picked up **{assetInfo}**",
                            color = "66ff99",
                            author = new
                        {
                            icon_url = $"{icon}",
                            name = $"{playerName} - {playerSteamID}",
                            url = $"https://steamcommunity.com/profiles/{playerSteamID}",
                        },
                        footer = new
                        {
                            text = $"{serverName}",
                        }
                    }
                    }
                });

                await sw.WriteAsync(json);
                await sw.FlushAsync();
            }
            try
            {
                await request.GetResponseAsync();
            }
            catch (WebException we)
            {
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Failed to Post to Discord API (Status: {we.Status}) - {we}");
                });
            }
            
        }
    }
}
