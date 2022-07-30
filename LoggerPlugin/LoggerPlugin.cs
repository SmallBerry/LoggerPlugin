using Newtonsoft.Json;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Core.Steam;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using LoggerPlugin.Connection;
using System.Threading.Tasks;

namespace LoggerPlugin
{
    public class LoggerPlugin : RocketPlugin<LoggerPluginConfiguration>
    {
        public static LoggerPlugin Instance { get; private set; }
        public static LoggerPluginConfiguration Conf;
        private string FilePath;
        public string LogFormat;

        protected override void Load()
        {
            Instance = this;
            Conf = Configuration.Instance;
            FilePath = Instance.Directory + @"CommandLogger.txt";
            LogFormat = "[{0}] {1} ({2}) picked up {3}";

            ItemManager.onTakeItemRequested += OnTakeItemRequested;
            Logger.Log("---------------------------------------------------");
            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded");
            Logger.Log("---------------------------------------------------");
        }

        private void OnTakeItemRequested(Player player, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromPlayer(player);
            ItemAsset itemAsset = Assets.find(EAssetType.ITEM, itemData.item.id) as ItemAsset;

            Logger.Log($"{unturnedPlayer.DisplayName} ({unturnedPlayer.CSteamID}) has picked up " +
                        $"item {itemAsset}");

            Log(string.Format(LogFormat, DateTime.Now, unturnedPlayer.DisplayName,
                unturnedPlayer.CSteamID, itemAsset));
            // not async
            //Webhook(player, itemAsset);
            // async
            var playerName = unturnedPlayer.DisplayName;
            var assetName = itemAsset;
            var playerSteamID = unturnedPlayer.CSteamID.ToString();
            var icon = new Profile(ulong.Parse(playerSteamID)).AvatarMedium.ToString();
            var serverName = Provider.serverName;

            var task = DiscordWebhook.SendDiscordWebhookAsync(playerName, assetName, icon, playerSteamID, serverName);
            Task.Run(async () => await task);
            
        }
        private void Log(string Message)
        {
            TextWriter tw = new StreamWriter(FilePath, true);
            tw.WriteLine(Message);
            tw.Close();
        }
        protected override void Unload()
        {
            Instance = null;
            Conf = null;
            ItemManager.onTakeItemRequested -= OnTakeItemRequested;
            Logger.Log($"{Name} has been unloaded");

        }
        public void Webhook(Player player, ItemAsset itemAsset)
        {
            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromPlayer(player);
            var steamId = unturnedPlayer.CSteamID.ToString();
            var icon = new Profile(ulong.Parse(steamId)).AvatarMedium.ToString();

            WebRequest wr = (HttpWebRequest)WebRequest.Create(Conf.WebhookLink);
            wr.ContentType = "application/json";
            wr.Method = "POST";

            using (var sw = new StreamWriter(wr.GetRequestStream()))
            {
                
                string json = JsonConvert.SerializeObject(new
                {
                    username = Conf.LoggerName,
                    avatar_url = Conf.LoggerAvatarUrl,
                    embeds = new[]
                    {
                    new
                    {
                            description = $"{unturnedPlayer.DisplayName } picked up **{itemAsset}**",
                            color = $"{int.Parse(Conf.EmbedColorHex, NumberStyles.HexNumber)}",
                            author = new
                        {
                            icon_url = $"{icon}",
                            name = $"{unturnedPlayer.DisplayName} - {unturnedPlayer.CSteamID}",
                            url = $"https://steamcommunity.com/profiles/{unturnedPlayer.CSteamID}",
                        },
                        footer = new
                        {
                            text = $"{Provider.serverName}",
                        }
                    }
                    }
                });

                sw.Write(json);
            }
            var response = (HttpWebResponse)wr.GetResponse();
        }
    }
}
