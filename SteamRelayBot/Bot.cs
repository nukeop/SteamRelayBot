using SteamKit2;
using SteamRelayBot.Commands;
using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;

namespace SteamRelayBot
{
    //Struct storing pairs of ids and usernames
    struct SteamUserInfo
    {
        public SteamUserInfo(SteamID _id, string _username)
        {
            id = _id;
            username = _username;
        }
        public SteamID id;
        public string username;
    }

    class Bot
    {
        //Logger
        private Logger log;

        //List of people greeted by this bot
        List<SteamID> mGreeted;

        //List of people seen chatting
        List<SteamUserInfo> mChattingUsers;

        //Dictionary of available commands
        Dictionary<string, ICommand> mCommands;

        //Used to communicate with steam
        SteamUser steamUser;
        SteamFriends steamFriends;
        SteamClient steamClient;

        //Credentials
        string user = ConfigurationManager.AppSettings["user"];
        string pass = ConfigurationManager.AppSettings["password"];
        public string apikey = ConfigurationManager.AppSettings["apikey"];
        string authCode = "";

        //ID of the chatroom we're in
        public SteamID chatRoomID;

        //IDs of chatrooms to autojoin when connected
        public List<string> mAutojoinChatRooms = new List<string>();

        //Continue running
        public bool isRunning;

        //Lists of commands
        List<string> mSimpleGroupCommands = new List<string> { "8ball", "joke", "trivia", "nsa" };
        List<string> mArgGroupCommands = new List<string> { "stock", "ddg", "urban", "addjoke", "spillthebeans" };
        List<string> mSimpleUserCommands = new List<string> { "8ball", "joke", "trivia", "nsa" };
        List<string> mArgUserCommands = new List<string> { "stock", "ddg", "urban", "addjoke", "spillthebeans" };


        public Bot(SteamUser user, SteamFriends friends, SteamClient client)
        {
            mGreeted = new List<SteamID>();
            mChattingUsers = new List<SteamUserInfo>();
            mCommands = new Dictionary<string, ICommand>();
            log = Logger.GetLogger();

            steamUser = user;
            steamFriends = friends;
            steamClient = client;

            //Load chatrooms to automatically join
            AutojoinGroupChatConfigurationSection sec = (AutojoinGroupChatConfigurationSection)ConfigurationManager.GetSection("autojoinGroupChats");
            GroupChatElementCollection gcec = sec.SteamIDs;
            for(int i=0; i<sec.SteamIDs.Count; i++)
            {
                mAutojoinChatRooms.Add(gcec[i].SteamID);
            }

            //Add instances of commands to the list
            List<ICommand> commandsToAdd = new List<ICommand> {
                new ListCommands(),
                new EightBall(),
                new Insult(),
                new Stock(),
                new DuckDuckGoDefine(),
                new UrbanDictionary(),
                new Joke(),
                new AddJoke(),
                new Trivia(),
                new AddTrivia(),
                new SpillTheBeans(),
                new Nsa(),
                new Game(),
            };

            foreach(ICommand com in commandsToAdd)
                mCommands[com.GetCommandString()] = com;
        }

        public void Connect(SteamClient.ConnectedCallback callback, uint attempts)
        {
            if (callback.Result != EResult.OK)
            {
                log.Error(String.Format("Unable to connect to Steam: {0}", callback.Result));

                isRunning = false;

                if (attempts > 0)
                {
                    log.Info(String.Format("Retrying, remaining attempts: {0}", attempts));
                    Connect(callback, attempts - 1);
                }
                else
                    return;
            }
			
            log.Info(String.Format("Connected to Steam! Logging in '{0}'...", user));
            
        }

        public void OnConnected(SteamClient.ConnectedCallback callback)
        {
            Connect(callback, 10);

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }
			
            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,

                AuthCode = authCode,

                SentryFileHash = sentryHash,
            });

        }

        public void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            log.Info("Disconnected from Steam");

            //Wait before reconnecting
            log.Info("Sleeping for 5000ms before reconnecting");
            Thread.Sleep(5000);

            steamClient.Connect();
        }

        public void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result == EResult.AccountLogonDenied)
            {
                Console.WriteLine("This account is SteamGuard protected.");

                Console.Write("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);

                authCode = Console.ReadLine();

                return;
            }
            if (callback.Result != EResult.OK)
            {
                log.Error(String.Format("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult));
                isRunning = false;
                return;
            }
            log.Info("Successfully logged on!");

            log.Info("Joining chatrooms automatically");
            foreach(string chatroomID in mAutojoinChatRooms)
            {
                SteamID chatroomSteamID = new SteamID(ulong.Parse(chatroomID));
                steamFriends.JoinChat(chatroomSteamID);
                chatRoomID = chatroomSteamID;
            }
        }

        public void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            steamFriends.SetPersonaState(EPersonaState.Online);
        }

        public void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            int friendCount = steamFriends.GetFriendCount();

            log.Info(String.Format("We have {0} friends", friendCount));

            for (int x = 0; x < friendCount; x++)
            {
                SteamID steamIdFriend = steamFriends.GetFriendByIndex(x);

                log.Info(String.Format("Friend: {0} ({1})", steamFriends.GetFriendPersonaName(steamIdFriend), steamIdFriend.Render()));
            }

            foreach (var friend in callback.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    // add everyone who invited us to friends
                    log.Info(String.Format("User {0} ({1}) added me to his/her friends", steamFriends.GetFriendPersonaName(friend.SteamID), friend.SteamID.Render()));
                    steamFriends.AddFriend(friend.SteamID);
                }
            }
        }

        public void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            log.Info(String.Format("{0} is now a friend", callback.PersonaName));
        }

        public void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            log.Info(String.Format("Logged off of Steam: {0}", callback.Result));
        }

        public void OnChatInvite(SteamFriends.ChatInviteCallback callback)
        {
            log.Info(String.Format("Invited to {0} by {1}", callback.ChatRoomName, steamFriends.GetFriendPersonaName(callback.PatronID)));
            steamFriends.JoinChat(callback.ChatRoomID);
            chatRoomID = callback.ChatRoomID;
        }

        public void OnFriendMessage(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.EntryType.Equals(EChatEntryType.Typing))
                log.Info(String.Format("{0} started typing a message to me", steamFriends.GetFriendPersonaName(callback.Sender)));

            if (callback.EntryType.Equals(EChatEntryType.ChatMsg))
                log.Info(String.Format("Message from {0}: {1}", steamFriends.GetFriendPersonaName(callback.Sender), callback.Message));

            ParseCommands(callback);
        }

        public void OnChatEnter(SteamFriends.ChatEnterCallback callback)
        {
            if (callback.EnterResponse == EChatRoomEnterResponse.NotAllowed)
            {
                log.Warning(String.Format("Not allowed to join {}", chatRoomID));
            }

            ChatroomMessage(chatRoomID, String.Format("RelayBot™ signed in and joined chatroom: {0}", callback.ChatRoomName));
        }

        public void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");
            byte[] sentryHash = CryptoHelper.SHAHash(callback.Data);
            File.WriteAllBytes("sentry.bin", callback.Data);
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,
                FileName = callback.FileName,
                BytesWritten = callback.BytesToWrite,
                FileSize = callback.Data.Length,
                Offset = callback.Offset,
                Result = EResult.OK,
                LastError = 0,
                OneTimePassword = callback.OneTimePassword,
                SentryFileHash = sentryHash,
            });
            Console.WriteLine("Done!");
        }

        public void OnChatroomMessage(SteamFriends.ChatMsgCallback callback)
        {
            //Set the current chatroom id
            this.chatRoomID = callback.ChatRoomID;

            //Log to a separate file for every chatroom
            string currentLogFile = Logger.filename;
            Logger.filename = Logger.CreateLogFilename(callback.ChatRoomID.Render());

            if (callback.ChatMsgType.Equals(EChatEntryType.ChatMsg))
            {
                //Add them to chatting users list
                SteamUserInfo sui = new SteamUserInfo(callback.ChatterID, steamFriends.GetFriendPersonaName(callback.ChatterID));
                if (!mChattingUsers.Contains(sui)) ;
                mChattingUsers.Add(sui);

                log.Info(String.Format("{0}[[{1}]]: {2}", steamFriends.GetFriendPersonaName(callback.ChatterID), callback.ChatterID.Render(), callback.Message));
            }

            if (callback.Message == "hi" || callback.Message == "hello" || callback.Message == "hey")
            {
                log.Info(String.Format("{0} entered group chat: {1}", steamFriends.GetFriendPersonaName(callback.ChatterID), chatRoomID));
                if (!mGreeted.Contains(callback.ChatterID))
                {
                    ChatroomMessage(chatRoomID, String.Format("Hello {0}", steamFriends.GetFriendPersonaName(callback.ChatterID)));
                    mGreeted.Add(callback.ChatterID);
                }
            }

            //Youtube titles
            ParseYoutubeLinks(callback);

            //Various commands
            ParseCommands(callback);

            if (callback.ChatMsgType.Equals(EChatEntryType.Disconnected) || callback.ChatMsgType.Equals(EChatEntryType.LeftConversation))
            {
                mChattingUsers.Remove(new SteamUserInfo(callback.ChatterID, steamFriends.GetFriendPersonaName(callback.ChatterID)));
                log.Info(String.Format("{0}[[{1}]] left the chat", steamFriends.GetFriendPersonaName(callback.ChatterID), callback.ChatterID.Render()));
            }

            //Restore old log filename
            Logger.filename = currentLogFile;
        }

        public void ChatroomMessage(SteamID chatID, string msg)
        {
            steamFriends.SendChatRoomMessage(chatID, EChatEntryType.ChatMsg, msg);
            log.Info(String.Format("[[ME]]: {0}", msg));
        }

        public void FriendMessage(SteamID friendID, string msg)
        {
            steamFriends.SendChatMessage(friendID, EChatEntryType.ChatMsg, msg);
            log.Info(String.Format("[[ME]] (to {0}): {1}", steamFriends.GetFriendPersonaName(friendID), msg));
        }

        public void TryCallCommandGroup(SteamFriends.ChatMsgCallback callback, string command, Object[] args = null)
        {
            ICommand com = null;
            mCommands.TryGetValue(command, out com);
            if (com != null)
            {
                com.GroupRun(callback, this, args);
            }
            else
            {
                ChatroomMessage(chatRoomID, String.Format("Command !{0} not found.", command));
            }

        }

        public void TryCallCommandFriend(SteamFriends.FriendMsgCallback callback, string command, Object[] args = null)
        {
            ICommand com = null;
            mCommands.TryGetValue(command, out com);
            if (com != null)
            {
                if (com.AvailableForFriends())
                    com.FriendRun(callback, this, args);
            }
            else
            {
                FriendMessage(callback.Sender, String.Format("Command !{0} not found.", command));
            }
        }

        //Parses commands from users
        private void ParseCommands(SteamFriends.FriendMsgCallback callback)
        {

            //Commands that do not require any additional arguments
            foreach (string com in mSimpleUserCommands)
                if (callback.Message.Equals("!" + com))
                    TryCallCommandFriend(callback, com);

            //Commands that take arguments from steam chat
            foreach (string com in mArgUserCommands)
                if (callback.Message.Contains("!" + com + " "))
                    TryCallCommandFriend(callback, com);


            //Complex commands that need specific arguments passed when called
            if (callback.Message.Contains("!say "))
            {
                List<string> msgstrings = new List<string>(callback.Message.Split(' '));
                msgstrings.RemoveAt(0);
                ChatroomMessage(chatRoomID, String.Join(" ", msgstrings.ToArray()));
            }
            else if (callback.Message.Equals("!commands"))
            {
                TryCallCommandFriend(callback, "commands", new Object[] { mCommands });
            }
            else if (callback.Message.Contains("!addtrivia"))
            {
                TryCallCommandFriend(callback, "addtrivia", new Object[] { steamFriends });
            }

        }

        //Parses commands from group chat
        private void ParseCommands(SteamFriends.ChatMsgCallback callback)
        {
            //Commands that do not require any additional arguments
            foreach (string com in mSimpleGroupCommands)
                if (callback.Message.Equals("!"+com))
                    TryCallCommandGroup(callback,com);

            //Commands that take arguments from steam chat
            foreach (string com in mArgGroupCommands)
                if (callback.Message.Contains("!" + com + " "))
                    TryCallCommandGroup(callback, com);

            //Complex commands that need specific arguments passed when called
            if (callback.Message.Equals("!commands"))
            {
                TryCallCommandGroup(callback, "commands", new Object[] { mCommands });
            }
            else if (callback.Message.Contains("!insult "))
            {
                TryCallCommandGroup(callback, "insult", new Object[] { mChattingUsers });
            }
            else if (callback.Message.Contains("!games "))
            {
                TryCallCommandGroup(callback, "games", new Object[] { mChattingUsers });
            }
            else if (callback.Message.Contains("!addtrivia"))
            {
                TryCallCommandGroup(callback, "addtrivia", new Object[] { steamFriends });
            }
        }

        private void ParseYoutubeLinks(SteamFriends.ChatMsgCallback callback)
        {
            Regex ytRegex = new Regex("(((youtube.*(v=|/v/))|(youtu\\.be/))(?<ID>[-_a-zA-Z0-9]+))");
            if (ytRegex.IsMatch(callback.Message))
            {
                Match ytMatch = ytRegex.Match(callback.Message);
                string youtubeMessage = Util.GetYoutubeTitle(ytMatch.Groups["ID"].Value);
                ChatroomMessage(chatRoomID, youtubeMessage);
            }
        }
    }
}
