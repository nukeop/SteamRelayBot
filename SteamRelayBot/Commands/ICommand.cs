using SteamKit2;
using System;

namespace SteamRelayBot
{
    //Implement this to create new commands
    interface ICommand
    {
        //Returns the string used to invoke the command
        string GetCommandString();

        //Returns a description of the command
        string GetDescription();

        //Whether the command is available to individual users
        bool AvailableForFriends();

        //What to do when someone in group chat calls the command
        void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, Object[] args = null);

        //What to do when an individual user calls the command
        void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, Object[] args = null);
    }
}
