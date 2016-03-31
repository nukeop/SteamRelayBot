using SteamKit2;
using System;
using System.Collections.Generic;
using System.IO;

namespace SteamRelayBot.Commands
{
    class Nsa : ICommand
    {
        //Location of the file with monitored phrases
        //Hardcoded as it's unlikely to be moved
        private string mListOfPhrases = "Data/dhs.txt";
        private List<string> mPhrases;

        public Nsa()
        {
            //Load the list of phrases
            mPhrases = new List<string>();
            using (StreamReader sr = new StreamReader(mListOfPhrases))
            {
                string phr = sr.ReadToEnd();
                string[] l = phr.Split('\n');
                foreach (string s in l)
                    mPhrases.Add(s.TrimEnd(new char[] { '\r' }));
            }
        }

        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "nsa";
        }

        public string GetDescription()
        {
            return "says a random phrase monitored by the american DHS";
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            bot.ChatroomMessage(bot.chatRoomID, System.Text.RegularExpressions.Regex.Unescape(Util.RandomChoice(mPhrases)));
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            bot.FriendMessage(callback.Sender, System.Text.RegularExpressions.Regex.Unescape(Util.RandomChoice(mPhrases)));
        }
    }
}
