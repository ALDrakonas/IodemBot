﻿using Discord;
using Discord.WebSocket;
using IodemBot.Core.UserManagement;
using IodemBot.Modules;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IodemBot.Core.Leveling
{
    internal static class Leveling
    {
        internal static ulong[] blackListedChannels = new ulong[] { 358276942337671178, 535082629091950602, 536721357216677891, 536721375323357196, 536721392620535830, 535199363907977226, 565910418741133315 };
        public static int rate = 200;
        public static int cutoff = 125000;

        internal static async void UserSentMessage(SocketGuildUser user, SocketTextChannel channel)
        {
            if (blackListedChannels.Contains(channel.Id))
            {
                return;
            }

            var userAccount = UserAccounts.GetAccount(user);

            // if the user has a timeout, ignore them
            var sinceLastXP = DateTime.UtcNow - userAccount.LastXP;
            uint oldLevel = userAccount.LevelNumber;

            if (sinceLastXP.Minutes >= 2)
            {
                userAccount.LastXP = DateTime.UtcNow;
                userAccount.XP += (uint)(new Random()).Next(30, 60);
            }

            if ((DateTime.Now.Date != userAccount.ServerStats.LastDayActive.Date))
            {
                userAccount.ServerStats.UniqueDaysActive++;
                userAccount.ServerStats.LastDayActive = DateTime.Now.Date;

                if ((DateTime.Now - user.JoinedAt).Value.TotalDays > 30)
                {
                    await GoldenSun.AwardClassSeries("Hermit Series", user, channel);
                }
            }

            if (channel.Id != userAccount.ServerStats.MostRecentChannel)
            {
                userAccount.ServerStats.MostRecentChannel = channel.Id;
                userAccount.ServerStats.ChannelSwitches += 2;
                if (userAccount.ServerStats.ChannelSwitches >= 14)
                {
                    await GoldenSun.AwardClassSeries("Air Pilgrim Series", user, channel);
                }
            }
            else
            {
                if (userAccount.ServerStats.ChannelSwitches > 0)
                {
                    userAccount.ServerStats.ChannelSwitches--;
                }
            }

            if (channel.Id == 546760009741107216)
            {
                userAccount.ServerStats.MessagesInColossoTalks++;
                if (userAccount.ServerStats.MessagesInColossoTalks >= 50)
                {
                    await GoldenSun.AwardClassSeries("Swordsman Series", user, channel);
                }
            }

            UserAccounts.SaveAccounts();
            uint newLevel = userAccount.LevelNumber;

            if (oldLevel != newLevel)
            {
                LevelUp(userAccount, user, channel);
            }

            await Task.CompletedTask;
        }

        internal static async void LevelUp(UserAccount userAccount, SocketGuildUser user, SocketTextChannel channel = null)
        {
            if (userAccount.LevelNumber < 10 && (userAccount.LevelNumber % 5) > 0)
            {
                channel = (SocketTextChannel)user.Guild.Channels.Where(c => c.Id == 358276942337671178).FirstOrDefault();
            }
            if (channel == null)
            {
                return;
            }
            // the user leveled up
            var embed = new EmbedBuilder();
            embed.WithColor(Colors.Get(userAccount.Element.ToString()));
            embed.WithTitle("LEVEL UP!");
            embed.WithDescription("<:Up_Arrow:571309108289077258> " + userAccount.GsClass + " " + user.Mention + " just leveled up!");
            embed.AddField("LEVEL", userAccount.LevelNumber, true);
            embed.AddField("XP", userAccount.XP, true);
            await channel.SendMessageAsync("", embed: embed.Build());
        }

        internal static async void UserAddedReaction(SocketGuildUser user, SocketReaction reaction)
        {
            if (blackListedChannels.Contains(reaction.Channel.Id))
            {
                return;
            }

            if (!Global.Client.Guilds.Any(g => g.Emotes.Any(e => e.Name == reaction.Emote.Name)))
            {
                return;
            }

            var userAccount = UserAccounts.GetAccount(user);
            if (reaction.MessageId == userAccount.ServerStats.MostRecentChannel)
            {
                userAccount.ServerStats.ReactionsAdded++;
            }
            else
            {
                userAccount.ServerStats.ReactionsAdded += 5;
                userAccount.ServerStats.MostRecentChannel = reaction.MessageId;
            }

            if (userAccount.ServerStats.ReactionsAdded >= 50)
            {
                try
                {
                    await GoldenSun.AwardClassSeries("Aqua Pilgrim Series", user, (SocketTextChannel)Global.Client.GetChannel(546760009741107216));
                }
                catch { }
            }
            UserAccounts.SaveAccounts();
        }

        internal static async void UserSentFile(SocketGuildUser user, SocketTextChannel channel)
        {
            await Task.CompletedTask;
        }

        internal static uint XPforNextLevel(uint xp)
        {
            int rate50 = 200;
            int cutoff50 = 125000;
            int rate80 = 1000;
            int cutoff80 = 605000;
            uint level = 1;
            uint xpneeded = 0;
            if (xp <= cutoff50)
            {
                level = (uint)Math.Sqrt(xp / 50);
                xpneeded = (uint)Math.Pow((level + 1), 2) * 50 - xp;
            }
            else if (xp <= cutoff80)
            {
                level = (uint)(50 - Math.Sqrt(cutoff50 / rate50) + Math.Sqrt(xp / rate50));
                xpneeded = (uint)(Math.Pow(level + 1 - 50 + Math.Sqrt(cutoff50 / rate50), 2) * rate50);
            }
            else
            {
                level = (uint)(80 - Math.Sqrt(cutoff80 / rate80) + Math.Sqrt(xp / rate80));
                xpneeded = (uint)(Math.Pow(level + 1 - 80 + Math.Sqrt(cutoff80 / rate80), 2) * rate80);
            }
            return xpneeded - xp;
        }
    }
}