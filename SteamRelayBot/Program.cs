using System;
using System.Collections.Generic;
using System.Threading;

using SteamKit2;
using IrcDotNet;

namespace SteamRelayBot
{
    class Program
    {
        static SteamClient steamClient;
        static CallbackManager manager;

        static SteamUser steamUser;
        static SteamFriends steamFriends;

        static Logger log;
        static Bot bot;

        static void Main(string[] args)
        {
            Logger.filename = "RelayBot.log";
            log = Logger.GetLogger();

            steamClient = new SteamClient(System.Net.Sockets.ProtocolType.Tcp);
            manager = new CallbackManager(steamClient);

            steamUser = steamClient.GetHandler<SteamUser>();
            steamFriends = steamClient.GetHandler<SteamFriends>();

            bot = new Bot(steamUser, steamFriends, steamClient);

            manager.Subscribe<SteamClient.ConnectedCallback>(bot.OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(bot.OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(bot.OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(bot.OnLoggedOff);

            manager.Subscribe<SteamUser.AccountInfoCallback>(bot.OnAccountInfo);
            manager.Subscribe<SteamFriends.FriendsListCallback>(bot.OnFriendsList);
            manager.Subscribe<SteamFriends.FriendAddedCallback>(bot.OnFriendAdded);

            manager.Subscribe<SteamFriends.ChatInviteCallback>(bot.OnChatInvite);
            manager.Subscribe<SteamFriends.ChatEnterCallback>(bot.OnChatEnter);
            manager.Subscribe<SteamFriends.FriendMsgCallback>(bot.OnFriendMessage);

            manager.Subscribe<SteamFriends.ChatMsgCallback>(bot.OnChatroomMessage);

            bot.isRunning = true;

            log.Info("Connecting to Steam...");

            steamClient.Connect();

            //callback loop
            while (bot.isRunning)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }
    }
}