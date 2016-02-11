using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Generic;
using SteamKit2;

namespace SteamRelayBot.Commands
{
    class Trivia : ICommand
    {
        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "trivia";
        }

        public string GetDescription()
        {
            return "tells a random piece of trivia";
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            string msg = GetRandomTriviaMsg();

            bot.FriendMessage(callback.Sender, System.Text.RegularExpressions.Regex.Unescape(msg));
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            string msg = GetRandomTriviaMsg();

            bot.ChatroomMessage(bot.chatRoomID, System.Text.RegularExpressions.Regex.Unescape(msg));
        }

        public string GetRandomTriviaMsg()
        {
            string query = "select * from Trivia";
            SQLiteDatabase db = new SQLiteDatabase();
            DataTable trivia;
            trivia = db.GetDataTable(query);

            List<DataRow> rows = new List<DataRow>();
            foreach (DataRow dr in trivia.Rows)
            {
                rows.Add(dr);
            }

            var row = Util.RandomChoice(rows);

            string body = row["Body"].ToString();
            string author = row["Author"].ToString();


            return String.Format("{0} shared the following piece of trivia: \n\n {1}", author, body);

        }
    }
}
