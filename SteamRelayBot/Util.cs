using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;
using System.Xml;

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
									"{0} is a total loser", "{0} is pure scum", "{0} is a low altitude flyer",
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
			string infoString = client.DownloadString (String.Format ("http://youtube.com/get_video_info?video_id={0}&el=vevo&el=embedded", id));

			if (!string.IsNullOrEmpty (infoString)) {
				string title = GetArgs (infoString, "title", '&');
				string rating = GetArgs (infoString, "avg_rating", '&');
				string length = GetArgs (infoString, "length_seconds", '&');

				float fRating = float.Parse (rating);
				int iLength = int.Parse (length);

				length = string.Format ("{0}:{1}", iLength/60, iLength%60);

				return String.Format ("{0} | {1} | {2}", title, length, RatingToStars(fRating));
			}
			else
			{
				return "Could not retrieve info about the video.";
			}
        }

		public static string RatingToStars(float rating)
		{
			string estar = "☆";
			string fstar = "★";
			string[] ratings = { 
				"☆☆☆☆☆", 
				"★☆☆☆☆", 
				"★★☆☆☆", 
				"★★★☆☆", 
				"★★★★☆", 
				"★★★★★", 
			};

			return string.Format (ratings[(int)Math.Round(rating)]);
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

        public static string GetDDGTopicSummary(string term)
        {
            WebClient client = new WebClient();
            term.Replace(' ', '+');
            string query = String.Format("https://api.duckduckgo.com/?q={0}&format=xml", term);
            string site = client.DownloadString(query);
            if (!String.IsNullOrEmpty(site))
            {
                XmlDocument doc = new XmlDocument();
                //Remove invalid characters
                site = new string(site.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray());
                doc.LoadXml(site);

                string _abstract="", abstractUrl="";

                XmlNode node = doc.SelectSingleNode("//DuckDuckGoResponse/Abstract/text()");
                if(node != null)
                _abstract = node.Value;
                node = doc.SelectSingleNode("//DuckDuckGoResponse/AbstractURL/text()");
                if (node != null)
                    abstractUrl = node.Value;

                if (!String.IsNullOrEmpty(_abstract) && !String.IsNullOrEmpty(abstractUrl))
                {
                    return _abstract + "\n\n" + abstractUrl;
                }

                if (!String.IsNullOrEmpty(_abstract))
                {
                    return _abstract;
                }

                StringBuilder sb = new StringBuilder();
                XmlNodeList relatedTopics = doc.SelectNodes("//DuckDuckGoResponse/RelatedTopics/RelatedTopic");
                int counter = 0;
                foreach(XmlNode xn in relatedTopics)
                {
                    sb.Append("("+counter+") "+xn["Text"].InnerText);
                    sb.Append("\n");
                    counter++;

                    if (counter >= 5)
                        break;
                }

                string related = sb.ToString();
                if(!String.IsNullOrEmpty(related))
                {
                    return String.Format("No information about topic {0}, here are some related terms:\n\n{1}", term, related);
                }

                if (!String.IsNullOrEmpty(abstractUrl))
                {
                    return abstractUrl;
                }

            }

            return String.Format("No information about topic {0}.", term);

        }

        public static string GetUrbanDictionaryDefiniton(string term)
        {
            WebClient client = new WebClient();
            term.Replace(' ', '+');

            string query = String.Format("http://api.urbandictionary.com/v0/define?term={0}", term);
            string site = client.DownloadString(query);

            XmlDocument doc = JsonConvert.DeserializeXmlNode(site, "root");
            XmlNode node = doc.SelectSingleNode("//root/list/definition/text()");

            if (node != null && !String.IsNullOrEmpty(node.InnerText))
                return node.InnerText;
            else
                return String.Format("No definition for word {0} in Urban Dictionary.", term);
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
