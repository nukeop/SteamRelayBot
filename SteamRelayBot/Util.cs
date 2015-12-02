using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamRelayBot
{
    class Util
    {

        public static string[] eightballAnswers = { "It is certain", "It is decidedly so", "Without a doubt", "Yes, definitely",
                                             "You may rely on it", "As I see it, yes", "Most likely", "Outlook good",
                                             "Yes", "Signs point to yes", "Reply hazy try again", "Ask again later",
                                             "Better not tell you now", "Cannot predict now", "Concentrate and ask again",
                                             "Don't count on it", "My reply is no", "My sources say no", "Outlook not so good",
                                             "Very doubtful",
                                            };
        public static string[] insults = { "Hey {0}, eat a dick", "Hey {0}, go fuck yourself", "Hey {0}, you're one ugly fag",
                                    "{0} is a dumb manboon", "{0} is a filthy liberal scum", "{0} buys Nvidia products",
                                    "{0} you sonnovabitch", "{0} chats at CDS",
                                    };

        public static T RandomChoice<T>(IEnumerable<T> source)
        {
            Random rnd = new Random();
            T result = default(T);
            int cnt = 0;
            foreach (T item in source)
            {
                cnt++;
                if (rnd.Next(cnt) == 0)
                {
                    result = item;
                }
            }
            return result;
        }
    }
}
