using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamRelayBot
{
    class Logger
    {
        private static Logger log;

        public static string filename;

        private Logger()
        {

        }

        public static Logger GetLogger()
        {
            if(log == null)
            {
                log = new Logger();
            }

            return log;
        }

        public void LogMessage(string message, string level)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true);

            message = DateTime.Now + " - " + level + " - " + message;

            Console.WriteLine(message);

            file.WriteLine(message);

            file.Close();
        }

        public void Debug(string message)
        {
            LogMessage(message, "DEBUG");
        }

        public void Info(string message)
        {
            LogMessage(message, "INFO");
        }

        public void Warning(string message)
        {
            LogMessage(message, "WARNING");
        }

        public void Error(string message)
        {
            LogMessage(message, "ERROR");
        }

    }
}
