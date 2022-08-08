using Rocket.API;
using Rocket.Core.Logging;
using SDG.Unturned;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SpeedMann.ReconnectBan
{
    public class ReconnectRestrictorConfiguration : IRocketPluginConfiguration
    {
        public string Version = "1.0.0.0";
        public int AllowedReconnects = 3;
        public int WarnAfterReconnects = 2;
        public int TimeTresholdMinutes = 5;
        public uint BanDuration = 200;
        public bool IgnoreAdmins = true;

        public void LoadDefaults()
        {

        }
        internal void checkConfig(string version)
        {
            Version = version;

            ReconnectRestrictor.Inst.Configuration.Save();
        }
    }
}
