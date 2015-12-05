using SteamKit2;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamRelayBot.Commands
{
    class ListCommands : ICommand
    {
        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "commands";
        }

        public string GetDescription()
        {
            return "shows a list of available commands";
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, Object[] args = null)
        {
            if (args == null)
            {
                bot.ChatroomMessage(bot.chatRoomID, "List of commands not passed, bot performed an illegal operation.");
            }
            else
            {
                Dictionary<string, ICommand> commands = args[0] as Dictionary<string, ICommand>;
                StringBuilder sb = new StringBuilder();
                sb.Append("\n");
                foreach (ICommand com in commands.Values)
                {
                    sb.AppendFormat("\t\t\t\t!{0} - {1}\n",
                        com.GetCommandString(),
                        com.GetDescription());
                }
                bot.ChatroomMessage(bot.chatRoomID, sb.ToString());
            }
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, Object[] args = null)
        {
            if (args == null)
            {
                bot.FriendMessage(callback.Sender, "List of commands not passed, bot performed an illegal operation.");
            }
            else
            {
                Dictionary<string, ICommand> commands = args[0] as Dictionary<string, ICommand>;
                StringBuilder sb = new StringBuilder();
                sb.Append("\n");
                foreach (ICommand com in commands.Values)
                {
                    if (com.AvailableForFriends())
                    {
                        sb.AppendFormat("\t\t\t\t!{0} - {1}\n",
                            com.GetCommandString(),
                            com.GetDescription());
                    }
                }
                bot.FriendMessage(callback.Sender, sb.ToString());
            }
        }
    }
}
