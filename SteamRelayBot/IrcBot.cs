using IrcDotNet;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SteamRelayBot
{
    class IrcBot
    {
        //Logger
        private Logger log;

        // Internal and exposable collection of all clients that communicate individually with servers.
        private List<IrcClient> allClients;

        public IrcBot()
        {
            log = Logger.GetLogger();
            allClients = new List<IrcClient>();
        }

        #region IRC Client Event Handlers

        private void IrcClient_Connected(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;

            OnClientConnect(client);
        }

        private void IrcClient_Disconnected(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;

            OnClientDisconnect(client);
        }

        private void IrcClient_Registered(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;

            //client.LocalUser.NoticeReceived += IrcClient_LocalUser_NoticeReceived;
            //client.LocalUser.MessageReceived += IrcClient_LocalUser_MessageReceived;
            //client.LocalUser.JoinedChannel += IrcClient_LocalUser_JoinedChannel;
            // client.LocalUser.LeftChannel += IrcClient_LocalUser_LeftChannel;

            Console.Beep();

            OnClientRegistered(client);
        }
        #endregion

        public void Connect(string server, IrcRegistrationInfo registrationInfo)
        {
            // Create new IRC client and connect to given server.
            var client = new StandardIrcClient();
            client.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
            client.Connected += IrcClient_Connected;
            client.Disconnected += IrcClient_Disconnected;
            client.Registered += IrcClient_Registered;

            // Wait until connection has succeeded or timed out.
            using (var connectedEvent = new ManualResetEventSlim(false))
            {
                client.Connected += (sender2, e2) => connectedEvent.Set();
                client.Connect(server, false, registrationInfo);
                if (!connectedEvent.Wait(10000))
                {
                    client.Dispose();
                    log.Error(String.Format("Connection to '{0}' timed out.", server));
                    return;
                }
            }

            // Add new client to collection.
            this.allClients.Add(client);

            Console.Out.WriteLine("Now connected to '{0}'.", server);

            allClients[0].Channels.Join("#cfc");
        }

        protected virtual void OnClientConnect(IrcClient client)
        {
            
        }

        protected virtual void OnClientDisconnect(IrcClient client) { }

        protected virtual void OnClientRegistered(IrcClient client) { }
    }
}
