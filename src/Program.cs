using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_http_server.src
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Logs from your program will appear here!");

            string directory = GetDirectoryFromArgs(args);
            var server = new HttpServer(4221, directory);
            server.Start();
        }

        private static string GetDirectoryFromArgs(string[] args)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--directory")
                {
                    return args[i + 1];
                }
            }
            return Directory.GetCurrentDirectory();
        }
    }
}
