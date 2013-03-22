using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;


namespace TrollBot
{
    using System.Threading;
    using System.Threading.Tasks;

    using IrcDotNet;

    internal class Program
    {
        private static bool closingTime;

        internal static IrcClient client;

        private static IrcClient octgnClient;
        private static IrcClient octgnDevClient;
        private static List<ChannelBot> channels = new List<ChannelBot>();
        private static bool gotChannels;
        static void Main(string[] args)
        {
            client = new IrcClient();
            var reg = new IrcUserRegistrationInfo();
            reg.NickName = "GaryBot";
            reg.UserName = "GaryBot";
            reg.RealName = "GaryBot";
            client.Disconnected += ClientOnDisconnected;
            client.Connected += ClientOnConnected;
            client.ErrorMessageReceived += ClientOnErrorMessageReceived;
            client.MotdReceived += ClientOnMotdReceived;
            client.RawMessageReceived += ClientOnRawMessageReceived;
            client.ChannelListReceived += ClientOnChannelListReceived;
            client.ProtocolError += ClientOnProtocolError;
            client.RawMessageSent += ClientOnRawMessageSent;
            client.Error += client_Error;
            client.Connect("irc.freenode.net",6667,false,reg);
            while (!closingTime)
            {
                Thread.Sleep(10);
            }
        }

        static void client_Error(object sender, IrcErrorEventArgs e)
        {
            Log("Client Error: " + e.Error.Message, ConsoleColor.Magenta);
        }

        private static void ClientOnRawMessageSent(object sender, IrcRawMessageEventArgs ircRawMessageEventArgs)
        {
            if(ircRawMessageEventArgs != null)
                Log("Message Sent: " + ircRawMessageEventArgs.RawContent, ConsoleColor.DarkGreen);
        }

        private static void Connect()
        {
            
        }

        private static void ClientOnProtocolError(object sender, IrcProtocolErrorEventArgs ircProtocolErrorEventArgs)
        {
            Log("Error: " + ircProtocolErrorEventArgs.Message, ConsoleColor.Magenta);
            if (ircProtocolErrorEventArgs.Code == 433)
            {
                
            }
        }

        private static void ClientOnChannelListReceived(object sender, IrcChannelListReceivedEventArgs ircChannelListReceivedEventArgs)
        {
            Log("Got those channels yo");
            gotChannels = true;
            foreach (var c in ircChannelListReceivedEventArgs.Channels)
            {
                Log("Trying to join channel " + c.Name);
                var tc = c;
                client.Channels.Join(c.Name);
                var task = new Task(
                    () =>
                        {
                            while (client.Channels.FirstOrDefault(x => x.Name == tc.Name) == null)
                            {
                                Thread.Sleep(10);
                            }
                            Log("WOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
;                            channels.Add(new ChannelBot(client.Channels.First(x => x.Name == tc.Name)));
                        });
                task.Start();
            }
        }

        private static void ClientOnRawMessageReceived(object sender, IrcRawMessageEventArgs ircRawMessageEventArgs)
        {
            Log(ircRawMessageEventArgs.RawContent, ConsoleColor.White);
            try
            {
                var mess = JsonConvert.SerializeObject(ircRawMessageEventArgs.Message);
                Log(mess);
            }
            catch
            {

            }
            if (ircRawMessageEventArgs.Message.Parameters[0] == "gary199567" && ircRawMessageEventArgs.Message.Source.Name == "kellyelton")
            {
                client.SendRawMessage("PRIVMSG #octgn :" + ircRawMessageEventArgs.Message.Parameters[1]);
            }
            
        }

        private static void ClientOnMotdReceived(object sender, EventArgs eventArgs)
        {
            Log("motd");
            new Task(() =>
                         {
                             while (!gotChannels)
                             {
                                 Log("Getting chanel list");
                                 //client.Channels.Join("#octgn");
                                 //client.SendRawMessage("LIST #octgn");
                                 client.ListChannels("#octgn");
                                 Thread.Sleep(20000);
                             }
                         }).Start();
            //channels.Add(new ChannelBot(client.Channels.First(x => x.Name == "#octgn")));
            //channels.Add(new ChannelBot(client.Channels.First(x => x.Name == "#octgn-dev")));
        }

        private static void ClientOnErrorMessageReceived(object sender, IrcErrorMessageEventArgs ircErrorMessageEventArgs)
        {
            Log("Error: " + ircErrorMessageEventArgs.Message);
        }

        private static void ClientOnConnected(object sender, EventArgs eventArgs)
        {
            Log("Connected");
            //client.ListChannels("octgn","octgn-dev");
            //client.Channels.Join("octgn", "octgn-dev");
        }

        private static void ClientOnDisconnected(object sender, EventArgs eventArgs)
        {
            Log("Disconnected");
            closingTime = true;
        }

        private static object logLock = new object();
        private static void Log(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            lock (logLock)
            {
                var o = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ForegroundColor = o;
            }
        }
    }
}
