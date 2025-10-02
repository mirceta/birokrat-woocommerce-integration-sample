using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace infrastructure_pinger
{

    public class SmtpEmailSenderFactory {
        public static SmtpEmailSender Birokrat(MailAddress sender, string senderPassword, string senderUsername, List<MailAddress> recipients) {
            var smtp = new SmtpClient {
                Host = "mail.birokrat.si",
                Port = 587,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderUsername, senderPassword)
            };
            return new SmtpEmailSender(smtp, sender, senderPassword, senderUsername, recipients);
        }
    }


    public class SmtpEmailSender
    {

        MailAddress sender;
        string senderPassword;
        string senderUsername;
        SmtpClient client;

        List<MailAddress> recipients;

        public SmtpEmailSender(SmtpClient client, MailAddress sender, string senderPassword, string senderUsername, List<MailAddress> recipients) {
            this.client = client;
            this.recipients = recipients;
            this.sender = sender;
            this.senderPassword = senderPassword;
            this.senderUsername = senderUsername;
        }

        public void SendEmail(string subject, string body)
        {
            var fromAddress = sender;
            foreach (var toAddress in recipients)
            {

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    client.Send(message);
                }
            }
        }
    }
}
