using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace SteamRelayBot.Commands
{
    class AddJoke : ICommand
    {
        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "addjoke";
        }

        public string GetDescription()
        {
            return "adds a new joke to the database (you can add links to funny images but try not to abuse this)";
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            AddNewJoke(callback.Message);

            bot.FriendMessage(callback.Sender, "Thank you, joke added");
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            AddNewJoke(callback.Message);

            bot.ChatroomMessage(bot.chatRoomID, "Thank you, joke added");
        }

        public void AddNewJoke(string message)
        {
            List<string> strings = new List<string>(message.Split(' '));
            strings.RemoveAt(0);
            string joke = String.Join(" ", strings.ToArray());

            SQLiteDatabase db = new SQLiteDatabase();
            Dictionary<String, String> data = new Dictionary<String, String>();
            data.Add("JokeBody", joke);
            try
            {
                db.Insert("Jokes", data);
            }
            catch (Exception crap)
            {
                Logger.GetLogger().Error(crap.Message);
            }
        }
    }
}
