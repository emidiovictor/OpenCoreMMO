﻿using NeoServer.Game.Common.Talks;
using NeoServer.Game.Contracts.Chats;
using NeoServer.Game.Contracts.Creatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoServer.Game.Chats
{
    public class ChatChannel : IChatChannel
    {
        public event AddMessage OnMessageAdded;

        protected IDictionary<uint, UserChat> users = new Dictionary<uint, UserChat>();

        public ChatChannel(ushort id, string name)
        {
            Id = id;
            Name = name;
        }

        public ushort Id { get; }
        public virtual string Name { get; }
        public ChannelRule JoinRule { get; init; }
        public virtual ChannelRule WriteRule { get; init; }
        public MuteRule MuteRule { get; init; }
        public virtual SpeechType ChatColor { get; init; } = SpeechType.ChannelYellowText;
        public string Description { get; init; }
        public virtual bool Opened { get; init; }

        public Dictionary<byte, SpeechType> ChatColorByVocation { private get; init; }
        public virtual IEnumerable<IUserChat> Users => users.Values;

        public virtual SpeechType GetTextColor(IPlayer player)
        {
            if (player is null) return ChatColor;
            if (ChatColorByVocation is not null && ChatColorByVocation.TryGetValue(player.VocationType, out var color)) return color;

            return ChatColor;
        }
        public virtual bool HasUser(IPlayer player) => users.TryGetValue(player.Id, out var user) && user.Removed == false;
        public virtual bool AddUser(IPlayer player)
        {
            if (!PlayerCanJoin(player)) return false;

            if (users.TryGetValue(player.Id, out var user))
            {
                if (user.Removed == true)
                {
                    user.MarkAsAdded();
                    return true;
                }
                return false;
            }
            return users.TryAdd(player.Id, new UserChat { Player = player });
        }

        public virtual bool RemoveUser(IPlayer player)
        {
            if (users.TryGetValue(player.Id, out var user))
            {
                if (!user.IsMuted) users.Remove(player.Id);
                else user.MarkAsRemoved();
            }
            return true;
        }
        public bool PlayerCanJoin(IPlayer player) => Validate(JoinRule, player);
        public bool PlayerCanWrite(IPlayer player) => player is null ? true : player is not null && users.ContainsKey(player.Id) && Validate(WriteRule, player);
        public bool PlayerIsMuted(IPlayer player, out string cancelMessage)
        {
            cancelMessage = default;
            if (player is null) return false;

            if (users.TryGetValue(player.Id, out var user) && user.IsMuted)
            {
                cancelMessage = string.IsNullOrWhiteSpace(MuteRule.CancelMessage) ? $"You are muted for {user.RemainingMutedSeconds} seconds" : MuteRule.CancelMessage;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Write a message on channel without identification, useful for server message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancelMessage"></param>
        /// <returns></returns>
        public bool WriteMessage(string message, out string cancelMessage, SpeechType speechType = SpeechType.None)
        {
            cancelMessage = default;
            if (!(users?.Any() ?? false)) return false;

            var color = speechType == SpeechType.None ? ChatColor : speechType;

            OnMessageAdded?.Invoke(null, this, color, message);
            return true;
        }

        public bool WriteMessage(ISociableCreature creature, string message, out string cancelMessage, SpeechType speechType = SpeechType.None)
        {
            cancelMessage = default;

            IPlayer player = creature is IPlayer ? (IPlayer) creature : null;

            if (!PlayerCanWrite(player))
            {
                cancelMessage = "You cannot send message to this channel";
                return false;
            }

            if (PlayerIsMuted(player, out cancelMessage)) return false;

            if (users.TryGetValue(player?.Id ?? 0, out var user))
            {
                user.UpdateLastMessage(MuteRule);
            }

            var color = speechType == SpeechType.None ? GetTextColor(player) : speechType;

            OnMessageAdded?.Invoke(creature, this, color, message);
            return true;
        }

        public bool Validate(ChannelRule rule, IPlayer player)
        {
            if (rule.None) return true;
            if (rule.AllowedVocations?.Length > 0 && !rule.AllowedVocations.Contains(player.VocationType)) return false;

            if (rule.MinMaxAllowedLevel.Item1 > 0 && player.Level <= rule.MinMaxAllowedLevel.Item1) return false;
            if (rule.MinMaxAllowedLevel.Item2 > 0 && player.Level > rule.MinMaxAllowedLevel.Item2) return false;
            return true;
        }
    }
}
