using SteamKit2;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;

namespace SteamRelayBot.Commands
{
    class Joke : ICommand
    {
        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "joke";
        }

        public string GetDescription()
        {
            return "tells a random joke";
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            string query = "select * from Jokes";
            SQLiteDatabase db = new SQLiteDatabase();
            DataTable jokes;
            jokes = db.GetDataTable(query);

            List<string> alljokes = new List<string>();
            foreach (DataRow dr in jokes.Rows)
            {
                alljokes.Add(dr["JokeBody"].ToString());
            }

            bot.ChatroomMessage(bot.chatRoomID, System.Text.RegularExpressions.Regex.Unescape(Util.RandomChoice(alljokes)));
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            string query = "select * from Jokes";
            SQLiteDatabase db = new SQLiteDatabase();
            DataTable jokes;
            jokes = db.GetDataTable(query);

            List<string> alljokes = new List<string>();
            foreach(DataRow dr in jokes.Rows)
            {
                alljokes.Add(dr["JokeBody"].ToString());
            }

            bot.FriendMessage(callback.Sender, System.Text.RegularExpressions.Regex.Unescape(Util.RandomChoice(alljokes)));
        }
    }
}
