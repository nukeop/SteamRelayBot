using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace SteamRelayBot.Commands
{
    class DuckDuckGoDefine : ICommand
    {
        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "ddg";
        }

        public string GetDescription()
        {
            return "shows a definiton of a term retrieved from DuckDuckGo";
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string term = String.Join(" ", strings.ToArray());

            bot.FriendMessage(callback.Sender, Util.GetDDGTopicSummary(term));
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string term = String.Join(" ", strings.ToArray());

            bot.ChatroomMessage(bot.chatRoomID, Util.GetDDGTopicSummary(term));
        }
    }
}
