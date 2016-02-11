﻿using SteamKit2;
using SteamRelayBot.Commands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        //ID of the chatroom we're in
        public SteamID chatRoomID;

        //Continue running
        public bool isRunning;

        //Credentials
        private string user = "relaybot";
        private string pass = "";

        public Bot(SteamUser user, SteamFriends friends, SteamClient client)
        {
            mGreeted = new List<SteamID>();
            mChattingUsers = new List<SteamUserInfo>();
            mCommands = new Dictionary<string, ICommand>();
            log = Logger.GetLogger();

            steamUser = user;
            steamFriends = friends;
            steamClient = client;

            //Add instances of commands to the list
            ICommand com;
            com = new ListCommands();
            mCommands[com.GetCommandString()] = com;
            com = new EightBall();
            mCommands[com.GetCommandString()] = com;
            com = new Insult();
            mCommands[com.GetCommandString()] = com;
            com = new Stock();
            mCommands[com.GetCommandString()] = com;
            com = new DuckDuckGoDefine();
            mCommands[com.GetCommandString()] = com;
            com = new UrbanDictionary();
            mCommands[com.GetCommandString()] = com;
            com = new Joke();
            mCommands[com.GetCommandString()] = com;
            com = new AddJoke();
            mCommands[com.GetCommandString()] = com;
            com = new Trivia();
            mCommands[com.GetCommandString()] = com;
            com = new AddTrivia();
            mCommands[com.GetCommandString()] = com;
            com = new SpillTheBeans();
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

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,
            });
        }

        public void OnConnected(SteamClient.ConnectedCallback callback)
        {
            Connect(callback, 10);
        }

        public void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            log.Info("Disconnected from Steam");

            steamClient.Connect();
        }

        public void OnLoggedOn(SteamUser.LoggedOnCallback callback)
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

        public void OnChatroomMessage(SteamFriends.ChatMsgCallback callback)
        {
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
            if (callback.Message.Contains("!say "))
            {
                List<string> msgstrings = new List<string>(callback.Message.Split(' '));
                msgstrings.RemoveAt(0);
                ChatroomMessage(chatRoomID, String.Join(" ", msgstrings.ToArray()));
            }

            if (callback.Message.Equals("!commands"))
            {
                TryCallCommandFriend(callback, "commands", new Object[] { mCommands });
            }
            else if (callback.Message.Equals("!8ball"))
            {
                TryCallCommandFriend(callback, "8ball");

            }
            else if (callback.Message.Contains("!stock "))
            {
                TryCallCommandFriend(callback, "stock");
            }
            else if(callback.Message.Contains("!ddg "))
            {
                TryCallCommandFriend(callback, "ddg");
            }
            else if (callback.Message.Contains("!urban "))
            {
                TryCallCommandFriend(callback, "urban");
            }
            else if (callback.Message.Equals("!joke"))
            {
                TryCallCommandFriend(callback, "joke");
            }
            else if (callback.Message.Contains("!addjoke "))
            {
                TryCallCommandFriend(callback, "addjoke");
            }
            else if (callback.Message.Contains("!trivia"))
            {
                TryCallCommandFriend(callback, "trivia");
            }
            else if (callback.Message.Contains("!addtrivia"))
            {
                TryCallCommandFriend(callback, "addtrivia", new Object[] { steamFriends });
            }
            else if(callback.Message.Contains("!spillthebeans "))
            {
                TryCallCommandFriend(callback, "spillthebeans");
            }


        }

        //Parses commands from group chat
        private void ParseCommands(SteamFriends.ChatMsgCallback callback)
        {
            if (callback.Message.Equals("!commands"))
            {
                TryCallCommandGroup(callback, "commands", new Object[] { mCommands });
            }
            else if (callback.Message.Equals("!8ball"))
            {
                TryCallCommandGroup(callback, "8ball");
            }
            else if (callback.Message.Contains("!insult "))
            {
                TryCallCommandGroup(callback, "insult", new Object[] { mChattingUsers });
            }
            else if (callback.Message.Contains("!stock "))
            {
                TryCallCommandGroup(callback, "stock");
            }
            else if (callback.Message.Contains("!ddg "))
            {
                TryCallCommandGroup(callback, "ddg");
            }
            else if (callback.Message.Contains("!urban "))
            {
                TryCallCommandGroup(callback, "urban");
            }
            else if(callback.Message.Equals("!joke"))
            {
                TryCallCommandGroup(callback, "joke");
            }
            else if(callback.Message.Contains("!addjoke "))
            {
                TryCallCommandGroup(callback, "addjoke");
            }
            else if (callback.Message.Contains("!trivia"))
            {
                TryCallCommandGroup(callback, "trivia");
            }
            else if (callback.Message.Contains("!addtrivia"))
            {
                TryCallCommandGroup(callback, "addtrivia", new Object[] { steamFriends });
            }
            else if (callback.Message.Contains("!spillthebeans "))
            {
                TryCallCommandGroup(callback, "spillthebeans");
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
