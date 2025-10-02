using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace core.logic.mapping_woo_to_biro.other
{
    class GmailSender
    {

        string senderMail;
        string senderPassword;
        public GmailSender(string senderMail, string senderPassword) {
            this.senderMail = senderMail;
            this.senderPassword = senderPassword;
        }

        public void SendMail(string recipientMail, string subject, string body) {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderMail, senderPassword),
                EnableSsl = true,
            };
            smtpClient.Send(senderMail, recipientMail, subject, body);
        }
    }
}
