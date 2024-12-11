using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace WEBAPITESTPROJECT.Services
{
    public class Logging
    {
        public void logger(string message, int Found)
        {
            string log_Path = "C:/Log";
            string logPath = "C:/Log/";
            if (!Directory.Exists(log_Path))
            {
                Directory.CreateDirectory(log_Path);
            }
            logPath = logPath + System.DateTime.Today.ToString("yyyyMMdd") + "." + "txt";

            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine($"{ DateTime.Now } {message} {Found}");
            }

        }

    }
}
