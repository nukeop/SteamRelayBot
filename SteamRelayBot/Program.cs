using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

using SteamKit2;

namespace SteamRelayBot
{
    class Logger
    {

        string filename;

        public Logger(string f)
        {
            filename = f;
        }

        public void LogMessage(string message, string level)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true);

            message = DateTime.Now + " - " + level +" - " + message;

            Console.WriteLine(message);

            file.WriteLine(message);

            file.Close();
        }

        public void Debug(string message)
        {
            LogMessage(message, "DEBUG");
        }

        public void Info(string message)
        {
            LogMessage(message, "INFO");
        }

        public void Warning(string message)
        {
            LogMessage(message, "WARNING");
        }

        public void Error(string message)
        {
            LogMessage(message, "ERROR");
        }

    }

    class Program
    {
        static SteamClient steamClient;
        static CallbackManager manager;

        static SteamUser steamUser;
        static SteamFriends steamFriends;

        static bool isRunning;

        static string user, pass, authCode = "";

        static Logger log;

        static List<SteamID> greeted;

        static void Main(string[] args)
        {
            log = new Logger("relaybot.log");

            greeted = new List<SteamID>();

            if (args.Length < 2)
            {
                log.Error("No username and password specified!");
                return;
            }

            user = args[0];
            pass = args[1];

            steamClient = new SteamClient(System.Net.Sockets.ProtocolType.Tcp);
            manager = new CallbackManager(steamClient);

            steamUser = steamClient.GetHandler<SteamUser>();
            steamFriends = steamClient.GetHandler<SteamFriends>();

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
            manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
            manager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);

            manager.Subscribe<SteamFriends.ChatInviteCallback>(OnChatInvite);
            manager.Subscribe<SteamFriends.ChatEnterCallback>(OnChatEnter);
            manager.Subscribe<SteamFriends.FriendMsgCallback>(OnFriendMessage);

            manager.Subscribe<SteamFriends.ChatMsgCallback>(OnChatroomMessage);

            isRunning = true;

            log.Info("Connecting to Steam...");

            steamClient.Connect();

            //callback loop
            while (isRunning)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                log.Error(String.Format("Unable to connect to Steam: {0}", callback.Result));

                isRunning = false;
                return;
            }

            log.Info(String.Format("Connected to Steam! Logging in '{0}'...", user));

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,
            });
        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            log.Info("Disconnected from Steam");

            isRunning = false;
        }

        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.AccountLogonDenied)
                {

                    log.Error("Unable to logon to Steam: This account is SteamGuard protected.");

                    isRunning = false;

                    return;
                }

                log.Error(String.Format("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult));

                isRunning = false;
                return;
            }

            log.Info("Successfully logged on!");
        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            steamFriends.SetPersonaState(EPersonaState.Online);
        }

        static void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            int friendCount = steamFriends.GetFriendCount();

            log.Info(String.Format("We have {0} friends", friendCount));

            for (int x = 0; x < friendCount; x++)
            {
                SteamID steamIdFriend = steamFriends.GetFriendByIndex(x);

                log.Info(String.Format("Friend: {0}", steamIdFriend.Render()));
            }

            foreach (var friend in callback.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    // add everyone who invited us to friends
                    log.Info(String.Format("User {0} added me to his/her friends", friend.SteamID.Render()));
                    steamFriends.AddFriend(friend.SteamID);
                }
            }
        }

        static void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            log.Info(String.Format("{0} is now a friend", callback.PersonaName));
        }

        static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            log.Info(String.Format("Logged off of Steam: {0}", callback.Result));
        }

        static void OnChatInvite(SteamFriends.ChatInviteCallback callback)
        {
            log.Info(String.Format("Invited to {0} by {1}", callback.ChatRoomName, steamFriends.GetFriendPersonaName(callback.PatronID)));
            steamFriends.JoinChat(callback.ChatRoomID);
        }

        static void OnFriendMessage(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.EntryType.Equals(EChatEntryType.Typing))
                log.Info(String.Format("{0} started typing a message to me", steamFriends.GetFriendPersonaName(callback.Sender)));

            if (callback.EntryType.Equals(EChatEntryType.ChatMsg))
                log.Info(String.Format("Message from {0}: {1}", steamFriends.GetFriendPersonaName(callback.Sender), callback.Message));
        }

        static void OnChatEnter(SteamFriends.ChatEnterCallback callback)
        {
            if (callback.EnterResponse == EChatRoomEnterResponse.NotAllowed)
            {
                log.Warning(String.Format("Not allowed to join {}", callback.ChatID));
            }

            steamFriends.SendChatRoomMessage(callback.ChatID, EChatEntryType.ChatMsg, String.Format("RelayBot™ signed in and joined chatroom: {0}", callback.ChatRoomName));
            log.Info(String.Format("[[ME]]: RelayBot™ signed in and joined chatroom: {0}", callback.ChatRoomName));

        }

        static void OnChatroomMessage(SteamFriends.ChatMsgCallback callback)
        {
            if(callback.ChatMsgType.Equals(EChatEntryType.ChatMsg))
                log.Info(String.Format("{0}[[{1}]]: {2}", steamFriends.GetFriendPersonaName(callback.ChatterID), callback.ChatterID.Render(), callback.Message));

            if (callback.Message == "hi" || callback.Message == "hello" || callback.Message == "hey")
            //if (callback.ChatMsgType.Equals(EChatEntryType.Entered))
            {
                log.Info(String.Format("{0} entered group chat: {1}", steamFriends.GetFriendPersonaName(callback.ChatterID), steamFriends.GetClanName(callback.ChatRoomID)));
                if (!greeted.Contains(callback.ChatterID))
                {
                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, String.Format("Hello {0}", steamFriends.GetFriendPersonaName(callback.ChatterID)));
                    log.Info(String.Format("[[ME]]: Hello {0}", steamFriends.GetFriendPersonaName(callback.ChatterID)));
                    greeted.Add(callback.ChatterID);
                }
            }

            if (callback.ChatMsgType.Equals(EChatEntryType.Disconnected) || callback.ChatMsgType.Equals(EChatEntryType.LeftConversation))
                log.Info(String.Format("{0}[[{1}]] left the chat", steamFriends.GetFriendPersonaName(callback.ChatterID), callback.ChatterID.Render()));

            if (callback.Message.Contains("what's the count") || callback.Message.Contains("whats the count"))
            {
                steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "good. real good");
                log.Info("[[ME]]: good. real good");
            }
            
            Regex ytRegex = new Regex("(((youtube.*(v=|/v/))|(youtu\\.be/))(?<ID>[-_a-zA-Z0-9]+))");
            if (ytRegex.IsMatch(callback.Message))
            {
                Match ytMatch = ytRegex.Match(callback.Message);
                string youtubeMessage = GetTitle(ytMatch.Groups["ID"].Value);
                steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, youtubeMessage);
            }
        }
        
        public static string GetTitle(string id)
        {
            //string id = GetArgs(url, "v", '?');
            WebClient client = new WebClient();
            return GetArgs(client.DownloadString("http://youtube.com/get_video_info?video_id=" + id), "title", '&');
        }

        private static string GetArgs(string args, string key, char query)
        {
            int iqs = args.IndexOf(query);
            string querystring = null;

            if (iqs != -1)
            {
                querystring = (iqs < args.Length - 1) ? args.Substring(iqs + 1) : String.Empty;
                NameValueCollection nvcArgs = HttpUtility.ParseQueryString(querystring);
                return nvcArgs[key];
            }
            return String.Empty; // or throw an error
        }
    }
}
