using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace SteamRelayBot.Commands
{
    class UrbanDictionary : ICommand
    {
        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "urban";
        }

        public string GetDescription()
        {
            return "shows the Urban Dictionary definition of a term";
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string term = String.Join(" ", strings.ToArray());

            bot.FriendMessage(callback.Sender, Util.GetUrbanDictionaryDefiniton(term));
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string term = String.Join(" ", strings.ToArray());

            bot.ChatroomMessage(bot.chatRoomID, Util.GetUrbanDictionaryDefiniton(term));
        }
    }
}
