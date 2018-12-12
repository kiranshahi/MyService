using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;

namespace MyService
{
    internal class SendMailService
    {
        // This function write log to LogFile.txt when some error occurs.
        public static void WriteErrorLog(Exception ex)
        {
            var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile.txt", true);
            sw.WriteLine(DateTime.Now.ToString() + ": " + ex.Source.ToString().Trim() + "; " + ex.Message.Trim());
            sw.Flush();
            sw.Close();
        }
        // This function write Message to log file.
        public static void WriteErrorLog(string message)
        {
            try
            {
                var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                WriteErrorLog(e);
            }
        }

        // This function contains the logic to send mail.
        public static void SendEmail(string toEmail, string subj, string message)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient { EnableSsl = true, Timeout = 200000 };
                MailMessage mailMsg = new MailMessage();
                ContentType htmlType = new ContentType("text/html");

                mailMsg.BodyEncoding = System.Text.Encoding.Default;
                mailMsg.To.Add(toEmail);
                mailMsg.Priority = MailPriority.High;
                mailMsg.Subject = subj;
                mailMsg.Body = message;
                mailMsg.IsBodyHtml = true;
                AlternateView.CreateAlternateViewFromString(message, htmlType);

                smtpClient.Send(mailMsg);
                WriteErrorLog("Mail sent successfully!");
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex.Message);
            }
        }
    }
}