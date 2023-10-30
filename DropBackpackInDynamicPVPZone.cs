using Oxide.Core.Plugins;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Drop Backpack In Dynamic PVP Zone", "WhiteThunder", "1.0.0")]
    [Description("Forces player backpacks to drop when they are killed in a DynamicPVP zone.")]
    internal class DropBackpackInDynamicPVPZone : CovalencePlugin
    {
        #region Fields

        [PluginReference]
        private readonly Plugin Backpacks, DynamicPVP, ZoneManager;

        #endregion

        #region Hooks

        // Handle player death by normal means.
        private void OnEntityDeath(BasePlayer player, HitInfo info)
        {
            OnEntityKill(player);;
        }

        // Handle player death while sleeping in a safe zone.
        private void OnEntityKill(BasePlayer player)
        {
            if (player.IsNpc || !IsPlayerInPvpZone(player))
                return;

            var droppedItemContainer = Backpacks?.Call("API_DropBackpack", player);
            if (droppedItemContainer != null)
            {
                ChatMessage(player, LangEntry.BackpackDropped);
            }
        }

        #endregion

        #region Helpers

        private bool IsPlayerInPvpZone(BasePlayer player)
        {
            var inPvpDelay = DynamicPVP?.Call("IsPlayerInPVPDelay", player.userID);
            if (inPvpDelay is bool && (bool)inPvpDelay)
            {
                // Player recently exited a PVP zone and can still be killed.
                return true;
            }

            var zoneIdList = ZoneManager?.Call("GetPlayerZoneIDs", player) as string[];
            if (zoneIdList == null)
            {
                // Player is not in a zone, or Zone Manager is not loaded.
                return false;
            }

            foreach (var zoneId in zoneIdList)
            {
                var isPvpZone = DynamicPVP?.Call("IsDynamicPVPZone", zoneId);
                if (isPvpZone is bool && (bool)isPvpZone)
                {
                    // Player is in a dynamic PVP zone.
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Localization

        private class LangEntry
        {
            public static readonly List<LangEntry> AllLangEntries = new List<LangEntry>();

            public static readonly LangEntry BackpackDropped = new LangEntry("BackpackDropped", "Your backpack contents were dropped because you were killed during PvP.");

            public string Name;
            public string English;

            public LangEntry(string name, string english)
            {
                Name = name;
                English = english;

                AllLangEntries.Add(this);
            }
        }

        private string GetMessage(string playerId, LangEntry langEntry) =>
            lang.GetMessage(langEntry.Name, this, playerId);

        private void ChatMessage(BasePlayer player, LangEntry langEntry) =>
            player.ChatMessage(GetMessage(player.UserIDString, langEntry));

        protected override void LoadDefaultMessages()
        {
            var englishLangKeys = new Dictionary<string, string>();

            foreach (var langEntry in LangEntry.AllLangEntries)
            {
                englishLangKeys[langEntry.Name] = langEntry.English;
            }

            lang.RegisterMessages(englishLangKeys, this, "en");
        }

        #endregion
    }
}
