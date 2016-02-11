using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace SteamRelayBot.Commands
{
    class AddTrivia : ICommand
    {
        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "addtrivia";
        }

        public string GetDescription()
        {
            return "adds a new piece of trivia to the database";
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            SteamFriends sf = args[0] as SteamFriends;
            AddNewTrivia(callback.Message, sf.GetFriendPersonaName(callback.Sender));
            bot.FriendMessage(callback.Sender, String.Format("Thank you {0}, trivia added", sf.GetFriendPersonaName(callback.Sender)));
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            SteamFriends sf = args[0] as SteamFriends;
            AddNewTrivia(callback.Message, sf.GetFriendPersonaName(callback.ChatterID));
            bot.ChatroomMessage(bot.chatRoomID, String.Format("Thank you {0}, trivia added", sf.GetFriendPersonaName(callback.ChatterID)));
        }

        public void AddNewTrivia(string message, string author)
        {
            //Create a sanitised string with trivia
            List<string> strings = new List<string>(message.Split(' '));
            strings.RemoveAt(0);
            string trivia = String.Join(" ", strings.ToArray());
            trivia = System.Text.RegularExpressions.Regex.Escape(trivia.Replace("'", @"''"));

            SQLiteDatabase db = new SQLiteDatabase();
            Dictionary<String, String> data = new Dictionary<String, String>();
            data.Add("Body", trivia);
            data.Add("Author", author);

            try
            {
                db.Insert("Trivia", data);
            }
            catch (Exception crap)
            {
                Logger.GetLogger().Error(crap.Message);
            }
        }
    }
}
