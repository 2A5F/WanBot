﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WanBot.Api.Mirai;
using WanBot.Api.Mirai.Message;

namespace WanBot.Api.Message
{
    /// <summary>
    /// 群成员发送的事件
    /// </summary>
    public class GroupSender : ISender
    {
        public long GroupId { get; }
        public long Id { get; }
        private MiraiBot _bot;

        public string InternalName { get; }
        public string DisplayName { get; }

        public GroupSender(MiraiBot bot, string internalName, string displayName, long groupId, long qqId)
        {
            InternalName = internalName;
            DisplayName = displayName;
            GroupId = groupId;
            _bot = bot;
            Id = qqId;
        }

        public async Task ReplyAsync(MessageChain messageChain)
        {
            await _bot.SendGroupMessageAsync(GroupId, null, messageChain);
        }

        public async Task ReplyAsync(string message, int? replyId = null)
        {
            await _bot.SendGroupMessageAsync(GroupId, replyId, message);
        }

        public async Task ReplyAsync(IMessageBuilder messageBuilder, int? replyId = null)
        {
            await _bot.SendGroupMessageAsync(GroupId, replyId, messageBuilder);
        }

        public async Task NudgeAsync()
        {
            await _bot.SendGroupNudgeAsync(Id, GroupId);
        }

        public async Task<Profile> GetProfileAsync()
        {
            return await _bot.MemberProfileAsync(GroupId, Id);
        }

        public override int GetHashCode()
        {
            return (GroupId ^ Id).GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not GroupSender groupSender)
                return false;
            return 
                groupSender.Id == Id && 
                groupSender.GroupId == GroupId;
        }
    }
}
