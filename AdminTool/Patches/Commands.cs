using HarmonyLib;
using System;
using System.Linq;

namespace AdminTool.Patches
{
    public class Commands
    {
        public static bool error = false;
        public static bool system = false;
        public static bool noaccess = false;

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        public class AdminToolCommands
        {
            public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer, ref string chatText)
            {
                if (__instance != HudManager.Instance.Chat) return true;

                if (chatText.ToLower().StartsWith("!id"))
                {
                    if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        string message = "All players ids:\n";
                        foreach (var player in PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId))
                        {
                            message += $"{player.GetDefaultOutfit().PlayerName}: {player.PlayerId}\n";
                        }
                        message.Remove(message.Length - 1, 1);
                        chatText = message;
                        system = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }
                if (chatText.ToLower().StartsWith("!kick "))
                {
                    if (GameData.Instance.GetHost() == sourcePlayer.Data && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        if (chatText[6..].IsNullOrWhiteSpace())
                        {
                            chatText = "You must specify a player id as parameter, use /id to get the list of all players ids.";
                            error = true;
                        }
                        if (Convert.ToByte(chatText[6..]) == PlayerControl.LocalPlayer.PlayerId)
                        {
                            chatText = "You can't kick yourself!";
                            error = true;
                        }
                        else
                        {
                            var id = Convert.ToByte(chatText[6..]);
                            var playerWithId = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId == id).ToList();
                            if (!playerWithId.Any())
                            {
                                chatText = "You must give a valid player id, use /id to get the list of all players ids.";
                                error = true;
                            }
                            else
                            {
                                foreach (var player in playerWithId)
                                {
                                    AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
                                }
                                chatText = "The player was kicked successfully.";
                                system = true;
                            }
                        }
                    }
                    else if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        chatText = "You don't have access to this command!";
                        noaccess = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }

                if (chatText.ToLower().StartsWith("!ban "))
                {
                    if (GameData.Instance.GetHost() == sourcePlayer.Data && sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        if (chatText[5..].IsNullOrWhiteSpace())
                        {
                            chatText = "You must specify a player id as parameter, use /id to get the list of all players ids.";
                            error = true;
                        }
                        if (Convert.ToByte(chatText[5..]) == PlayerControl.LocalPlayer.PlayerId)
                        {
                            chatText = "You can't ban yourself!";
                            error = true;
                        }
                        else
                        {
                            var id = Convert.ToByte(chatText[5..]);
                            var playerWithId = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId == id).ToList();
                            if (!playerWithId.Any())
                            {
                                chatText = "You must give a valid player id, use /id to get the list of all players ids.";
                                error = true;
                            }
                            else
                            {
                                foreach (var player in playerWithId)
                                {
                                    AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
                                }
                                chatText = "The player was banned successfully.";
                                system = true;
                            }
                        }
                    }
                    else if (sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        chatText = "You don't have access to this command!";
                        noaccess = true;
                    }
                    return sourcePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
        public class ChatFixName
        {
            public static bool Prefix(ChatBubble __instance)
            {
                if (error)
                {
                    __instance.NameText.text = "ERROR";
                    __instance.NameText.color = Palette.ImpostorRed;
                    __instance.NameText.ForceMeshUpdate(true, true);
                    __instance.Xmark.enabled = true;
                    __instance.Background.color = Palette.White;
                    __instance.votedMark.enabled = false;
                    error = false;
                    return false;
                }
                else if (system)
                {
                    __instance.NameText.text = "SYSTEM MESSAGE";
                    __instance.NameText.color = Palette.CrewmateBlue;
                    __instance.NameText.ForceMeshUpdate(true, true);
                    __instance.Xmark.enabled = false;
                    __instance.Background.color = Palette.White;
                    __instance.votedMark.enabled = false;
                    system = false;
                    return false;
                }
                else if (noaccess)
                {
                    __instance.NameText.text = "NO ACCESS";
                    __instance.NameText.color = Palette.Blue;
                    __instance.NameText.ForceMeshUpdate(true, true);
                    __instance.Xmark.enabled = true;
                    __instance.Background.color = Palette.White;
                    __instance.votedMark.enabled = false;
                    noaccess = false;
                    return false;
                }
                else return true;
            }
        }
    }
    public static class Extensions
    {
        public static NetworkedPlayerInfo.PlayerOutfit GetDefaultOutfit(this PlayerControl playerControl)
        {
            return playerControl.Data.DefaultOutfit;
        }
    }
}
