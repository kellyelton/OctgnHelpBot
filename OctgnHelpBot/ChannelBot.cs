namespace TrollBot
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Web;

    using ChatterBotAPI;

    using IrcDotNet;

    public class ChannelBot
    {
        internal IrcChannel Channel { get; set; }
        internal bool Silence { get; set; }
        internal bool dboResponded { get; set; }
        internal ChatterBotSession Bot { get; set; }
        internal string Who = "Db0";
        private Random random = new Random();
        internal int noDb0Counter = 10;

        public ChannelBot(IrcChannel channel)
        {
            Channel = channel;

            Channel.UsersListReceived += ChannelOnUsersListReceived;
            Channel.UserJoined += ChannelOnUserJoined;
            Channel.UserLeft += ChannelOnUserLeft;
            Channel.MessageReceived += ChannelOnMessageReceived;
            var b = new ChatterBotAPI.ChatterBotFactory().Create(ChatterBotType.CLEVERBOT);
            Bot = b.CreateSession();
            //HelloTimerOnElapsed(null, null);
        }

        internal void Message(string message)
        {
            const string template = "PRIVMSG {0} :{1}";
            Channel.Client.SendRawMessage(String.Format(template, Channel.Name, message));
        }

        private void ChannelOnUsersListReceived(object sender, EventArgs eventArgs)
        {
            //Message("Hello!");
        }

        private void ChannelOnMessageReceived(object sender, IrcMessageEventArgs ircMessageEventArgs)
        {
            if (ircMessageEventArgs.Source.Name == "GaryBot") return;
            //if (ircMessageEventArgs.Text.Trim().EndsWith("?"))
            //{
            //    this.Message("http://lmgtfy.com/?q=" + HttpUtility.UrlEncode(ircMessageEventArgs.Text));
            //}
            return;
            var from = ircMessageEventArgs.Source.Name.ToLower();
            var num = random.Next(0, 100);
            if (from.Contains(Who.ToLower()) || from.Contains("brine") || num > 55)
            {
                noDb0Counter = 0;
                dboResponded = true;
                var reply = "";

                while (String.IsNullOrWhiteSpace(reply))
                {
                    try
                    {
                        reply = Bot.Think(ircMessageEventArgs.Text);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            Bot = new ChatterBotAPI.ChatterBotFactory().Create(ChatterBotType.CLEVERBOT).CreateSession();
                        }
                        catch
                        {

                        }
                    }
                }

                this.Message(reply);
            }

        }

        private void ChannelOnUserLeft(object sender, IrcChannelUserEventArgs ircChannelUserEventArgs)
        {
            //Message(ircChannelUserEventArgs.ChannelUser.User.NickName + " was a real dick that year...");
        }

        private object handleJoinLocker = new object();
        private bool HandleUserJoin(string username)
        {
            lock (handleJoinLocker)
            {
                var ret = false;
                username = username.ToLower();
                if (!File.Exists("users.txt")) File.Create("users.txt").Close();
                var userListRaw = File.ReadAllLines("users.txt");
                var userList = userListRaw.ToDictionary(x => x.Split(' ')[0].ToLower(), y => y.Split(' ')[1]);
                if (userList.ContainsKey(username))
                {
                    var timeSpan = new TimeSpan(DateTime.Now.Ticks - DateTime.Parse(userList[username]).Ticks);
                    if (timeSpan.Days > 10) ret = true;
                    userList[username] = DateTime.Now.ToString();
                    File.WriteAllLines("users.txt", userList.Select(x => x.Key + " " + x.Value));
                }
                else
                {
                    ret = true;
                    userList.Add(username,DateTime.Now.ToString());
                    File.WriteAllLines("users.txt", userList.Select(x => x.Key + " " + x.Value));
                }
                return ret;
            }
        }

        private void ChannelOnUserJoined(object sender, IrcChannelUserEventArgs ircChannelUserEventArgs)
        {
            if (this.HandleUserJoin(ircChannelUserEventArgs.ChannelUser.User.NickName))
            {
                this.Message(
                    "Hello, " + ircChannelUserEventArgs.ChannelUser.User.NickName
                    + ". If you need help may I recommend http://www.octgn.net/help ?");
                this.Message(
                    "If you need to report an issue you can always do that here https://github.com/kellyelton/OCTGN/issues .");
                this.Message("If you are feeling patient, please, stick around and someone will be with you shortly.");
            }
            else
            {
                
            }
            //this.Message("Hello " + ircChannelUserEventArgs.ChannelUser.User.NickName);
        }
    }
}