using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Xml;
using SteamKit2;

namespace SteamRelayBot.Commands
{
    class SpillTheBeans : ICommand
    {
        public bool AvailableForFriends()
        {
            return true;
        }

        public string GetCommandString()
        {
            return "spillthebeans";
        }

        public string GetDescription()
        {
            return "shows news about a certain topic";
        }

        public void FriendRun(SteamFriends.FriendMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string term = String.Join(" ", strings.ToArray());

            bot.FriendMessage(callback.Sender, GetNews(term));
        }

        public void GroupRun(SteamFriends.ChatMsgCallback callback, Bot bot, object[] args = null)
        {
            List<string> strings = new List<string>(callback.Message.Split(' '));
            strings.RemoveAt(0);
            string term = String.Join(" ", strings.ToArray());

            bot.ChatroomMessage(bot.chatRoomID, GetNews(term));
        }

        public string GetNews(string term)
        {
            WebClient client = new WebClient();
            term.Replace(' ', '+');

            string query = String.Format("https://news.google.com/news?q={0}&output=rss", term);
            string site = client.DownloadString(query);

            if (!String.IsNullOrEmpty(site))
            {
                XmlDocument doc = new XmlDocument();
                //Remove invalid characters
                site = new string(site.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray());
                doc.LoadXml(site);

                string title = "", pubdate = "", desc = "";

                XmlNode node = doc.SelectSingleNode("//rss/channel/item/title");
                if (node != null)
                    title = node.FirstChild.Value;

                node = doc.SelectSingleNode("//rss/channel/item/pubDate");
                if (node != null)
                    pubdate = node.FirstChild.Value;

                node = doc.SelectSingleNode("//rss/channel/item/description");
                if (node != null)
                    desc = node.FirstChild.Value;

                desc = Regex.Replace(desc, "<[^>]*(>|$)", "");
                desc = desc.Replace("&#39;", "'");

                if(String.IsNullOrEmpty(title) || String.IsNullOrEmpty(pubdate) || String.IsNullOrEmpty(desc))
                {
                    return String.Format("No news about topic: {0}", term);
                }

                return String.Format("{0}\nPublished: {1}\n{2}", title, pubdate, desc);
            }

            return String.Format("No news about topic: {0}", term);
        }
    }
}
