using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Timers;
using System.Xml.Linq;
using Timer = System.Timers.Timer;

namespace MyService
{
    public partial class Service : ServiceBase
    {
        private readonly Timer _timer1;
        private string _timeString;
        public int GetCallType;

        public Service()
        {
            InitializeComponent();
            int strTime = Convert.ToInt32(ConfigurationManager.AppSettings["CallDuration"]);
            GetCallType = Convert.ToInt32(ConfigurationManager.AppSettings["CallType"]);
            if (GetCallType == 1)
            {
                _timer1 = new Timer();
                double inter = GetNextInterval();
                _timer1.Interval = inter;
                _timer1.Elapsed += new ElapsedEventHandler(ServiceTimer_Tick);
            }
            else
            {
                _timer1 = new Timer
                {
                    Interval = strTime * 1000
                };
                _timer1.Elapsed += new ElapsedEventHandler(ServiceTimer_Tick);
            }
        }
        protected override void OnStart(string[] args)
        {
            _timer1.AutoReset = true;
            _timer1.Enabled = true;
            SendMailService.WriteErrorLog("Service started");
            SendMailService.SendEmail(ServiceProperty.Email, "Service Report", $"Service has been is started on {Environment.MachineName} on {DateTime.Now.ToLongTimeString()}.");
        }
        protected override void OnStop()
        {
            _timer1.AutoReset = false;
            _timer1.Enabled = false;
            SendMailService.WriteErrorLog("Service stopped");
            SendMailService.SendEmail(ServiceProperty.Email, "Service Report", $"Service on {Environment.MachineName} has been stopped on {DateTime.Now.ToLongTimeString()}.");
        }
        protected override void OnShutdown()
        {
            SendMailService.WriteErrorLog("System has been shutdown.");
            SendMailService.SendEmail(ServiceProperty.Email, "Service Report", $"{Environment.MachineName} went down on {DateTime.Now.ToLongTimeString()}");
        }

        // For Laptop
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            if (powerStatus.HasFlag(PowerBroadcastStatus.Suspend))
            {
                SendMailService.WriteErrorLog("System has been shutdown.");
                SendMailService.SendEmail(ServiceProperty.Email, "Service Report", "Your system has been down on " + DateTime.Now.ToLongTimeString());
            }

            return true;
        }
        private double GetNextInterval()
        {
            _timeString = ConfigurationManager.AppSettings["StartTime"];
            DateTime t = DateTime.Parse(_timeString);
            TimeSpan ts = new TimeSpan();
            ts = t - System.DateTime.Now;
            if (ts.TotalMilliseconds < 0)
            {
                ts = t.AddDays(1) - DateTime.Now; // Here we can increase the timer in
            }
            return ts.TotalMilliseconds;
        }
        private void SetTimer()
        {
            try
            {
                double inter = GetNextInterval();
                _timer1.Interval = inter;
                _timer1.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void ServiceTimer_Tick(object sender, ElapsedEventArgs e)
        {
            string msg = GetRssFeed();
            if (!string.IsNullOrEmpty(msg))
            {
                SendMailService.SendEmail(ServiceProperty.Email, "Today's Quote", msg);
            }
            if (GetCallType != 1)
            {
                return;
            }
            _timer1.Stop();
            System.Threading.Thread.Sleep(1000000);
            SetTimer();
        }
        public string GetRssFeed()
        {
            try
            {
                WebClient webClient = new WebClient();
                string rssData = webClient.DownloadString("https://www.brainyquote.com/link/quotebr.rss");

                XDocument xml = XDocument.Parse(rssData);
                IEnumerable<RssFeed> rssFeedData = (from x in xml.Descendants("item")
                                                    select new RssFeed
                                                    {
                                                        Title = ((string)x.Element("title")),
                                                        Description = ((string)x.Element("description"))
                                                    });
                string message = string.Empty;
                foreach (RssFeed item in rssFeedData)
                {
                    message += $"{item.Description} - {item.Title} \n";
                }
                return message;
            }
            catch (Exception e)
            {
                SendMailService.WriteErrorLog(e.Message);
                return null;
            }
        }
    }
}
