using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web;

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
                                    "{0} you sonnovabitch", "{0} chats at CDS", "{0} sucks", "{0}, you should kill yourself",
                                    "{0} is a total loser", "{0} is pure scum",
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

        public static string GetYoutubeTitle(string id)
        {
            WebClient client = new WebClient();
            return GetArgs(client.DownloadString(String.Format("http://youtube.com/get_video_info?video_id={0}&el=vevo&el=embedded", id)), "title", '&');
        }

        public static string GetYahooStocks(string company)
        {
            WebClient client = new WebClient();
            string site = client.DownloadString(String.Format("http://finance.yahoo.com/q?s={0}", company));
            string tagstart = "yfs_l84";
            string tagend = "yfs_c63";

            int first = site.IndexOf(tagstart) + tagstart.Length + company.Length + 3;
            int last = site.IndexOf(tagend);
            try
            {
                return String.Format("Stock value for company {0}: {1}", company, site.Substring(first, last - first - 61).Trim(new Char[] { '<', '>', '/', '\\' }));
            }
            catch (Exception e)
            {
                return String.Format("Could not retrieve stock value for company {0}", company);
            }
        }

        private static string GetArgs(string args, string key, char query)
        {
            int iqs = args.IndexOf(query);
            string querystring = null;
            if (iqs != -1)
            {
                querystring = (iqs < args.Length - 1) ? args.Substring(iqs + 1) : String.Empty;
                NameValueCollection nvcArgs = HttpUtility.ParseQueryString(querystring);
                return nvcArgs[key];
            }
            return String.Empty; // or throw an error
        }
    }
}
