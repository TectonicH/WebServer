using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Fclp;


namespace myOwnWebServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var p = new FluentCommandLineParser<CommandLineArguments>();

            p.Setup(arg => arg.Port)
                .As("webPort")
                .Required();

            p.Setup(arg => arg.Root)
                .As("webRoot")
                .Required();

            p.Setup(arg => arg.IPAddress)
                .As("webIP")
                .Required();

            p.Parse(args);

            ServeController server = new ServeController(p.Object.Root, IPAddress.Parse(p.Object.IPAddress), p.Object.Port);
            server.StartListen();
        }

    }
}