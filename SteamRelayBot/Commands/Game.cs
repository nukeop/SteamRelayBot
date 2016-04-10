using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamRelayBot.Commands
{
    class Game : ICommand
    {
        private Logger mLog;

        public Game()
        {
            mLog = Logger.GetLogger();
        }

        public bool AvailableForFriends()
        {
            return false;
        }

        public string GetCommandString()
        {
            return "games";
        }

        public string GetDescription()
        {
            return "shows info about a user's top played games";
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {}

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string user = (String.Join(" ", strings.ToArray()));
            bool found = false;

            SteamUserInfo userInfo = new SteamUserInfo();
            List<SteamUserInfo> chattingUsers = args[0] as List<SteamUserInfo>;
            foreach (SteamUserInfo sui in chattingUsers)
            {
                if (sui.username.Equals(user))
                {
                    found = true;
                    userInfo = sui;
                    break;
                }
            }

            if (found)
            {
                string accId = userInfo.id.ConvertToUInt64().ToString();

                //Begin building a message
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("{0}'s stats:\n", user));

                using (dynamic steamPlayerService = WebAPI.GetInterface("IPlayerService"))
                {

                    Dictionary<string, string> funArgs = new Dictionary<string, string>();
                    funArgs["steamid"] = accId;
                    funArgs["key"] = bot.apikey;

                    //Get total number of games
                    KeyValue results = steamPlayerService.Call("GetOwnedGames", 1, funArgs);
                    string total = results["game_count"].Value.ToString();
                    sb.Append(String.Format("Games owned: " + total + "\n"));

                    funArgs.Clear();
                    funArgs["steamid"] = accId;
                    funArgs["key"] = bot.apikey;

                    //Get info about recently played games
                    results = steamPlayerService.Call("GetRecentlyPlayedGames", 1, funArgs);
                    string totalPlayed = results["total_count"].Value.ToString();
                    var games = results["games"];
                    
                    sb.Append(String.Format("Games played: " + totalPlayed + "\n\n"));

                    try
                    {
                        foreach (var child in games.Children)
                        {
                            Dictionary<string, string> gameInfo = new Dictionary<string, string>();
                            foreach (var elem in child.Children)
                            {
                                gameInfo[elem.Name] = elem.Value;
                            }

                            int playtimeForever = int.Parse(gameInfo["playtime_forever"]);
                            int playtime2weeks = int.Parse(gameInfo["playtime_2weeks"]);

                            sb.Append(gameInfo["name"]);
                            sb.Append(", total time played: ");
                            sb.Append(String.Format("{0} hours {1} minutes", playtimeForever / 60, playtimeForever % 60));
                            sb.Append(", last two weeks: ");
                            sb.Append(String.Format("{0} hours {1} minutes", playtime2weeks / 60, playtime2weeks % 60));
                            sb.Append("\n");
                        }
                    }
                    catch(Exception e)
                    {
                        mLog.Error(String.Format("Exception encountered in Game.GroupRun: {0}", e.ToString()));
                        bot.ChatroomMessage(bot.chatRoomID, String.Format("Could not retrieve info about {0}, profile might be private.", user));
                        return;
                    }


                    bot.ChatroomMessage(bot.chatRoomID, sb.ToString());
                }
            }
        }

        
    }
}
