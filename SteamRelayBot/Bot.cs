using System;
using System.Collections.Generic;

using System.Net;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

using IrcDotNet;
using SteamKit2;

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

        //Used to communicate with steam
        SteamUser steamUser;
        SteamFriends steamFriends;

        //ID of the chatroom we're in
        SteamID chatRoomID;

        //Continue running
        public bool isRunning;

        //Credentials
        private string user = "relaybot";
        private string pass = "l0rd_gumblert";

        public Bot(SteamUser user, SteamFriends friends)
        {
            mGreeted = new List<SteamID>();
            mChattingUsers = new List<SteamUserInfo>();
            log = Logger.GetLogger();

            steamUser = user;
            steamFriends = friends;
        }

        public void OnConnected(SteamClient.ConnectedCallback callback)
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

        public void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            log.Info("Disconnected from Steam");

            isRunning = false;
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

            if (callback.Message.Contains("!say "))
            {
                List<string> msgstrings = new List<string>(callback.Message.Split(' '));
                msgstrings.RemoveAt(0);
                ChatroomMessage(chatRoomID, String.Join(" ", msgstrings.ToArray()));
            }
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

            if (callback.ChatMsgType.Equals(EChatEntryType.Disconnected) || callback.ChatMsgType.Equals(EChatEntryType.LeftConversation))
            {
                mChattingUsers.Remove(new SteamUserInfo(callback.ChatterID, steamFriends.GetFriendPersonaName(callback.ChatterID)));
                log.Info(String.Format("{0}[[{1}]] left the chat", steamFriends.GetFriendPersonaName(callback.ChatterID), callback.ChatterID.Render()));
            }

            if (callback.Message.Contains("what's the count") || callback.Message.Contains("whats the count"))
            {
                ChatroomMessage(chatRoomID, "good. real good");
            }

            if (callback.Message.Equals("!commands"))
            {
                ListCommands();
            }
            else if (callback.Message.Equals("!8ball"))
            {
                eightball();
            }
            else if (callback.Message.Contains("!insult"))
            {
                List<string> userstrings = new List<string>(callback.Message.Split(' '));
                userstrings.RemoveAt(0);
                Insult(String.Join(" ", userstrings.ToArray()));
            }
        }

        private void ChatroomMessage(SteamID chatID, string msg)
        {
            steamFriends.SendChatRoomMessage(chatID, EChatEntryType.ChatMsg, msg);
            log.Info(String.Format("[[ME]]: {0}", msg));
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

        private void Insult(string user)
        {
            bool found = false;
            foreach (SteamUserInfo sui in mChattingUsers)
            {
                if (sui.username.Equals(user))
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                string insult = Util.RandomChoice<string>(Util.insults);
                ChatroomMessage(chatRoomID, String.Format(insult, user));
            }
            else
            {
                ChatroomMessage(chatRoomID, "I'm not going to insult someone who isn't even here, or hasn't talked yet.");
            }
        }

        private void eightball()
        {
            Random rnd = new Random();

            string result = Util.RandomChoice<string>(Util.eightballAnswers);

            ChatroomMessage(chatRoomID, result);
        }

        private void ListCommands()
        {
            string commands = @"
                !commands - shows a list of all commands
                !8ball - answers a yes/no question
                !insult <user> - insults a user
                !stock <company> - shows current stocks values";
            ChatroomMessage(chatRoomID, commands);
        }
    }
}
