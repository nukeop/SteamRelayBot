using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web.Script.Serialization;
using SteamKit2;

namespace SteamRelayBot.Commands
{
    class Movie : ICommand
    {
        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "movie";
        }

        public string GetDescription()
        {
            return "shows information about a movie";
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string term = String.Join(" ", strings.ToArray());

            bot.FriendMessage(callback.Sender, GetMovieInfo(term));
        } 

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string term = String.Join(" ", strings.ToArray());

            bot.ChatroomMessage(bot.chatRoomID, GetMovieInfo(term));
        }

        public string GetMovieInfo(string term)
        {
            WebClient client = new WebClient();
            string query = String.Format("http://www.omdbapi.com/?t={0}", term);
            string site = client.DownloadString(query);

            if (!String.IsNullOrEmpty(site))
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var info = serializer.Deserialize< Dictionary<string, string>>(site);

                if (info["Response"]=="True")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Title: {0}\n", info["Title"]);
                    sb.AppendFormat("Year: {0}\n", info["Year"]);
                    sb.AppendFormat("Rated: {0}\n", info["Rated"]);
                    sb.AppendFormat("Genre: {0}\n", info["Genre"]);
                    sb.AppendFormat("Director: {0}\n", info["Director"]);
                    sb.AppendFormat("Actors: {0}\n", info["Actors"]);
                    sb.AppendFormat("Plot: {0}\n", info["Plot"]);
                    sb.AppendFormat("Poster: {0}\n", info["Poster"]);
                    sb.AppendFormat("Rating: {0}", info["imdbRating"]);

                    return sb.ToString();
                } 
            }

            return String.Format("No information about movie {0} could be found.", term);
        }
    }
}
