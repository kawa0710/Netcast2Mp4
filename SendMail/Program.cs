using RayITUtilityNet472;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace MySendMail
{
    class Program
    {
        static void Main(string[] args)
        {
            //利用IsRunning.txt供後續判斷本程式的執行狀態
            var path_IsRunning = "IsRunning.txt";
            bool flag_IsRunning = false;
            if (File.Exists(path_IsRunning))
            {
                string IsRunning = File.ReadAllText(path_IsRunning);
                if (IsRunning.Trim() != "0")
                    flag_IsRunning = true;
            }
            else
                File.WriteAllLines("IsRunning.txt", new string[] { "0" });

            if (!flag_IsRunning)
            { //檢查 Config有無設定資料暫存的磁碟機&&本程式是不是【執行中】
                File.WriteAllLines("IsRunning.txt", new string[] { "1" });

                Netcast2Mp4Entities db = null;
                try
                {
                    db = new Netcast2Mp4Entities(Conn);

                    SendMail s = GetAMail(db);
                    while (s != null && s.SML_SN > 0)
                    {
                        if (SendAMail(s))
                        {
                            s.SML_SendAt = DateTime.Now;
                            db.Entry(db.SendMails.Find(s.SML_SN)).CurrentValues.SetValues(s);
                            db.SaveChanges();
                        }

                        s = GetAMail(db);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (db != null)
                    {
                        db.Dispose();
                        db = null;
                    }
                }
            }

            File.WriteAllLines("IsRunning.txt", new string[] { "0" });
        }

        static SendMail GetAMail(Netcast2Mp4Entities db)
        {
            SendMail s = null;
            if (db == null)
                return s;

            var date1900 = new DateTime(1900, 1, 1);
            var q = from a in db.SendMails
                    where a.SML_SendAt == date1900
                    select a;
            s = q.FirstOrDefault();

            return s;
        }

        static string Conn
        {
            get
            {                
                var conn = Common.AESDecrypt(ConfigurationManager.ConnectionStrings["Netcast2Mp4Entities"].ToString(), Common.AES_KEY, Common.AES_IV);
                return conn;
            }
        }

        static string Host
        {
            get { return ConfigurationManager.AppSettings["mailHost"]; }
        }

        static string Port
        {
            get { return ConfigurationManager.AppSettings["mailPort"]; }
        }

        static string Token
        {
            get
            {
                var token = ConfigurationManager.AppSettings["mailToken"];
                return token;
            }
        }

        static string Sender
        {
            get { return ConfigurationManager.AppSettings["mailSender"]; }
        }

        static string SenderName
        {
            get 
            {
                string result = "Raycast客服";
                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["mailSenderName"]))
                    result = ConfigurationManager.AppSettings["mailSenderName"];
                return result; 
            }
        }

        static int SendDelayMS
        {
            get
            {
                if (int.TryParse(ConfigurationManager.AppSettings["mailSendDelayMS"], out int result))
                    return result;
                else
                    return 1000;
            }
        }

        static bool SendAMail(SendMail s)
        {
            bool result = false;
            int port = 0;

            if (string.IsNullOrWhiteSpace(Host) || string.IsNullOrWhiteSpace(Port)
                 || string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(Sender)
                 || !int.TryParse(Port, out port))
            {
                return result;
            }

            MailMessage mail = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            try
            {
                mail.To.Add(s.SML_To);
                var sender = Common.AESDecrypt(ConfigurationManager.AppSettings["mailSender"].ToString(), Common.AES_KEY, Common.AES_IV);
                mail.From = new MailAddress(sender, SenderName, Encoding.UTF8);
                mail.Subject = s.SML_Subject;
                mail.SubjectEncoding = Encoding.UTF8;
                mail.Body = s.SML_Body;
                mail.IsBodyHtml = true;
                mail.BodyEncoding = Encoding.UTF8;

                smtp.Host = Host;
                smtp.Port = port;
                smtp.UseDefaultCredentials = false;
                var token = Common.AESDecrypt(ConfigurationManager.AppSettings["mailToken"].ToString(), Common.AES_KEY, Common.AES_IV);
                smtp.Credentials = new NetworkCredential(sender, token);
                smtp.EnableSsl = true;
                smtp.Send(mail);
                Thread.Sleep(SendDelayMS); //限制N秒(預設1秒)發1封Email

                result = true;
            }
            catch
            {
                throw;
            }
            finally
            {
                smtp.Dispose();
                mail.Dispose();
            }

            return result;
        }
    }
}
