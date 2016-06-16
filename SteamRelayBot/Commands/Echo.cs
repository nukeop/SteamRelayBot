using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamRelayBot
{
    class Echo : ICommand
    {
        public Echo ()
        {
        }

        public bool AvailableForFriends ()
        {
            return true;
        }

        public string GetCommandString ()
        {
            return "echo";
        }

        public string GetDescription ()
        {
            return "lets you receive messages from a selected group chat " +
            		"(private chat only). 'echo channels' lets you see " +
            		"a list of groups the bot is connected to, 'echo start [" +
                	"channel name] starts relaying messages to you, and " +
                	"'echo stop [channel name]' stops transmission." +
                	" While you are subscribed to a group, any message" +
                	"you send to the bot will be relayed to the group chat.";
        }

        public void GroupRun (SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
        }

        public void FriendRun (SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string> (callback.Message.Split (' '));
            if (strings.Count == 2 && strings [1] == "channels")
            {
                //Just display channels
                StringBuilder sb = new StringBuilder ();

                List<string> groupNames = bot.AllChatrooms.Values.ToList ();
                for(int i=0; i<bot.AllChatrooms.Count; i++)
                {
                    sb.AppendLine (String.Format("{0}. {1}", i, groupNames[i]));
                }


                bot.FriendMessage (callback.Sender, sb.ToString());
            }
            else if (strings.Count >= 3)
            {
                if (strings [1] == "start")
                {
					if (!bot.SubscribingUsers.Keys.ToList().Contains (callback.Sender))
                    {
                        List<string> groupNames = bot.AllChatrooms.Values.ToList ();
                        string selectedGroupName = String.Join (" ", strings.Skip (2).Take (strings.Count - 2));
                        for (int i = 0; i < groupNames.Count; i++)
                        {
                            if (selectedGroupName == groupNames [i])
                            {
                                SteamID selectedGroupID = bot.AllChatrooms.Keys.ToList () [i];

                                List<SteamID> subscribingUsers;
                                bot.UserRelays.TryGetValue (selectedGroupID, out subscribingUsers);
                                if (subscribingUsers == null)
                                    bot.UserRelays [selectedGroupID] = new List<SteamID> ();
                                bot.UserRelays [selectedGroupID].Add (callback.Sender);
								bot.SubscribingUsers[callback.Sender] = selectedGroupID;

                                bot.FriendMessage (callback.Sender, String.Format ("You " +
                                "subscribed to group {0}. " +
                                "You will now start receiving messages from that group.", selectedGroupName));

								StringBuilder sb = new StringBuilder ("Users: \n");
								foreach (SteamUserInfo user in bot.mChattingUsers[selectedGroupID]) 
								{
									sb.AppendLine (user.username);
								}
								bot.FriendMessage (callback.Sender, sb.ToString ());
                            }
                        }
                    }
                    else
                    {
                        bot.FriendMessage (callback.Sender, "Unsubscribe first to subscribe to another group.");
                    }

                }
                else if (strings [1] == "stop")
                {
                    List<string> groupNames = bot.AllChatrooms.Values.ToList ();
                    string selectedGroupName = String.Join (" ", strings.Skip (2).Take (strings.Count - 2));
                    for (int i = 0; i < groupNames.Count; i++)
                    {
                        if (selectedGroupName == groupNames [i])
                        {
                            SteamID selectedGroupID = bot.AllChatrooms.Keys.ToList () [i];
                            List<SteamID> subscribingUsers;
                            bot.UserRelays.TryGetValue (selectedGroupID, out subscribingUsers);
                            if (subscribingUsers != null)
                            {
                                bot.UserRelays [selectedGroupID].Remove (callback.Sender);
                                bot.SubscribingUsers.Remove (callback.Sender);
                                bot.FriendMessage (callback.Sender, String.Format ("You " +
                                "unsubscribed from group {0}." +
                                " You will no longer receive messages from this group.",
                                                                                  selectedGroupName));
                            }
                        }
                    }
                }
                else
                {
                    bot.FriendMessage (callback.Sender, "Incorrect arguments.");
                }
            }
            else
            {
                bot.FriendMessage (callback.Sender, "Incorrect arguments.");
            }
        }
    }
}

