using SteamKit2;
using System;
using System.Collections.Generic;

namespace SteamRelayBot.Commands
{
    class Stock : ICommand
    {
        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "stock";
        }

        public string GetDescription()
        {
            return "shows stock value for a company";
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string company = String.Join(" ", strings.ToArray());
            bot.FriendMessage(callback.Sender, Util.GetYahooStocks(company));
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string company = String.Join(" ", strings.ToArray());
            bot.ChatroomMessage(bot.chatRoomID, Util.GetYahooStocks(company));
        }
    }
}
