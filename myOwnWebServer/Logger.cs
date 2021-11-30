/*
*FILE: Logger.cs
* PROGRAMMER: Kristian Biviens
* FIRST VERSION: 2021-11-29
* DESCRIPTION: This class will generate log entries into log file and will create a new log file if it does not exist in the current directory
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace myOwnWebServer
{
    static class Logger
    {
        //      METHOD: Log
        // DESCRIPTION: Processes log message into format and appends to log file. If log file doesn't exist, creates new log file with log message
        public static void Log(string entry)
        {
            FileStream file;
            StreamWriter sw;
            string logText = DateTime.Now.ToString() + " " + entry;
            string logPath = Directory.GetCurrentDirectory() + "\\myOwnWebServer.log";

            if (File.Exists(logPath))
            {
                using (file = File.Open(logPath, FileMode.Append))
                using (sw = new StreamWriter(file))
                {
                    sw.WriteLine(logText);
                }
            }
            else
            {
                using (file = File.Create(logPath))
                using (sw = new StreamWriter(file))
                {
                    sw.WriteLine(logText);
                }
            }

        
        }

    }
}
