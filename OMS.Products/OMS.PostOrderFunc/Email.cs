    using System;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;

    namespace OMS.PostOrderFunc
    {
        public class EmailSettings
        {
            public String PrimaryDomain { get; set; }

            public int PrimaryPort { get; set; }

            public String SecondayDomain { get; set; }

            public int SecondaryPort { get; set; }

            public String UsernameEmail { get; set; }

            public String UsernamePassword { get; set; }

            public String FromEmail { get; set; }

            public String ToEmail { get; set; }

            public String CcEmail { get; set; }
        }

        public class OmsEmail
        {
            public static async Task SentEmail(string email, string subject, string message)
            {
                try
                {
                    EmailSettings _emailSettings = new EmailSettings()
                    {
                        PrimaryDomain = "smtp.gmail.com",
                        PrimaryPort = 587,
                        SecondayDomain = "smtp.live.com",
                        SecondaryPort = 587,
                        UsernameEmail = "vlbhaskarb@gmail.com",
                        UsernamePassword = "<PASSWORD>",
                        FromEmail = "vlbhaskarb@gmail.com",

                    };

                    string toEmail = string.IsNullOrEmpty(email)
                                     ? _emailSettings.ToEmail
                                     : email;
                    MailMessage mail = new MailMessage()
                    {
                        From = new MailAddress(_emailSettings.UsernameEmail, "Bhaskar OMS")
                    };
                    mail.To.Add(new MailAddress(toEmail));

                    if(!string.IsNullOrEmpty(_emailSettings.CcEmail))
                     mail.CC.Add(new MailAddress(_emailSettings.CcEmail));

                    mail.Subject = "Order Management System - " + subject;
                    mail.Body = message;
                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.High;

                    using (SmtpClient smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort))
                    {
                        smtp.Credentials = new NetworkCredential(_emailSettings.FromEmail, _emailSettings.UsernamePassword);
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(mail);
                    }
                }
                catch (Exception ex)
                {
                    //do something here
                }
            }
        }
    }
