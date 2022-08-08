using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using System;
using System.Reflection;
using SDG.NetTransport;
using Rocket.API.Collections;
using Rocket.API;

namespace SpeedMann.ReconnectBan
{
    
    public class ReconnectRestrictor : RocketPlugin<ReconnectRestrictorConfiguration>
    {
        public static ReconnectRestrictor Inst;
        public static ReconnectRestrictorConfiguration Conf;

        private string Version;
        private Dictionary<CSteamID,List<DateTime>> ReconnectDict;

        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                { "reconnect_warning", "Reconnect {0} of {1} in {2} min!" },
                { "reconnect_ban_reason", "You where banned for reconnecting {0} times in {1}min" },
                { "ban_reason", "You where banned for rapidly reconnecting!"}
            };

        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;
            Version = readFileVersion();

            ReconnectDict = new Dictionary<CSteamID, List<DateTime>>();

            Logger.Log($"Loading ReconnectRestrictor {Version} by SpeedMann");

            Conf.checkConfig(Version);

            // Enter / Leave
            U.Events.OnPlayerConnected += onPlayerConnection;
            U.Events.OnPlayerDisconnected += onPlayerDisconnection;
        }

        protected override void Unload()
        {
            // Enter / Leave
            U.Events.OnPlayerConnected -= onPlayerConnection;
            U.Events.OnPlayerDisconnected -= onPlayerDisconnection;
        }

        private void Update()
        {

        }
        #region EventHooks
        private void onPlayerDisconnection(UnturnedPlayer player)
        {
            checkTimestaps(player.CSteamID);
            if(ReconnectDict.TryGetValue(player.CSteamID, out List<DateTime> timeStamps) && timeStamps.IsEmpty())
            {
                ReconnectDict.Remove(player.CSteamID);
            }
        }

        private void onPlayerConnection(UnturnedPlayer player)
        {
            if (player.IsAdmin && Conf.IgnoreAdmins || player.GetPermissions().Any(x => x.Name.ToLower() == "reconnectban.bypass")) return;

            if (ReconnectDict.TryGetValue(player.CSteamID, out List<DateTime> timeStamps))
            {
                checkTimestaps(player.CSteamID);
                if (timeStamps.Count >= Conf.WarnAfterReconnects)
                {
                    UnturnedChat.Say(player, Util.Translate("reconnect_warning", timeStamps.Count, Conf.AllowedReconnects, Conf.TimeTresholdMinutes), Color.red);
                }
                if (timeStamps.Count > Conf.AllowedReconnects)
                {
                    Provider.ban(player.CSteamID, Util.Translate("ban_reason", Conf.AllowedReconnects+1, Conf.TimeTresholdMinutes), Conf.BanDuration);
                    timeStamps.Clear();
                }
                timeStamps.Add(DateTime.Now);
            }
            else
            {
                ReconnectDict.Add(player.CSteamID, new List<DateTime> { DateTime.Now });
            }
            
        }

        #endregion

        #region HelperFunctions 
        private void checkTimestaps(CSteamID playerId)
        {
            if (!ReconnectDict.TryGetValue(playerId, out List<DateTime> timeStamps)) return;

            while (!timeStamps.IsEmpty() && DateTime.Now.Subtract(timeStamps[0]).Minutes > Conf.TimeTresholdMinutes)
            {
                timeStamps.RemoveAt(0);
            }
        }
        private static string readFileVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }
        #endregion
    }
}