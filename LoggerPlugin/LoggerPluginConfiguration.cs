using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggerPlugin
{
    public class LoggerPluginConfiguration : IRocketPluginConfiguration
    {
        public string WebhookLink { get; set; }
        public string LoggerName { get; set; }
        public string LoggerAvatarUrl { get; set; }
        public string EmbedColorHex { get; set; }

        public void LoadDefaults()
        {
            WebhookLink = "https://discord.com/api/webhooks/1002464072895307776/BD0-Wa3dYZCmq5MsbP8cT6N2Kp5_28_sfSXAa8-mSmGIlyyh4FUl-TiwIyZ0BP2Aarh-";
            LoggerName = "BerrysLogger";
            LoggerAvatarUrl = "https://i.imgur.com/CbL2v5t.jpeg";
            EmbedColorHex = "66ff99";
        }
    }
}
