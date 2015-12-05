using SteamKit2;
using System;

namespace SteamRelayBot.Commands
{
    class EightBall : ICommand
    {
        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "8ball";
        }

        public string GetDescription()
        {
            return "answers a yes/no question";
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            Random rnd = new Random();

            string result = Util.RandomChoice<string>(Util.eightballAnswers);

            bot.FriendMessage(callback.Sender, result);
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            Random rnd = new Random();

            string result = Util.RandomChoice<string>(Util.eightballAnswers);

            bot.ChatroomMessage(bot.chatRoomID, result);
        }
    }
}
