using SteamKit2;
using System;
using System.Collections.Generic;

namespace SteamRelayBot.Commands
{
    class Insult : ICommand
    {
        public bool AvailableForFriends()
        {
            return false;
        }

        public string GetCommandString()
        {
            return "insult";
        }

        public string GetDescription()
        {
            return "insults a user";
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string user = (String.Join(" ", strings.ToArray()));
            bool found = false;

            List<SteamUserInfo> chattingUsers = args[0] as List<SteamUserInfo>;
            foreach (SteamUserInfo sui in chattingUsers)
            {
                if (sui.username.Equals(user))
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                string insult = Util.RandomChoice<string>(Util.insults);
                bot.ChatroomMessage(bot.chatRoomID, String.Format(insult, user));
            }
            else
            {
                bot.ChatroomMessage(bot.chatRoomID, "I'm not going to insult someobody who isn't even here, or hasn't talked yet.");
            }
        }
    }
}
