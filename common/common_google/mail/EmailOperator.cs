using common_google.inbox_state;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Google.Apis.Gmail.v1.UsersResource.MessagesResource;

namespace common_google {
    public class EmailOperator {

        public static string[] Scopes = { GmailService.Scope.GmailCompose, GmailService.Scope.GmailSend, GmailService.Scope.MailGoogleCom, GmailService.Scope.GmailReadonly };
        GmailService service;
        string user_email;

        public EmailOperator(UserCredential cred, string user_email) {
            this.user_email = user_email;
            service = new GmailService(new BaseClientService.Initializer() {
                HttpClientInitializer = cred,
                ApplicationName = "birokrat-next-mobile"
            });
        }

        public void SendEmail(string to, string subject, string body) {

            var msg = new System.Net.Mail.MailMessage();
            msg.From = new System.Net.Mail.MailAddress(user_email);
            msg.To.Add(new System.Net.Mail.MailAddress(to));
            msg.ReplyToList.Add(new System.Net.Mail.MailAddress(to));
            msg.Subject = subject;
            msg.Body = body;
            msg.IsBodyHtml = false;

            object mimeMessage = MimeKit.MimeMessage.CreateFromMailMessage(msg);

            Message gmailMessage = new Message {
                Raw = Encode(mimeMessage.ToString())
            };

            SendRequest sr = service.Users.Messages.Send(gmailMessage, user_email);
            Message back = sr.Execute();

        }

        public List<string> GetNewMailIds(IInboxState state) {

            var req = service.Users.Messages.List(user_email);
            IList<Message> messages = req.Execute().Messages;

            return messages.Where(x => !state.isProcessed(x.Id))
                           .Select(x => x.Id).ToList();

        }

        public string GetMailSender(string mailId) {
            Message ms = service.Users.Messages.Get(user_email, mailId).Execute();

            string ret = "";
            try {
                ret = ms.Payload.Headers.Where(x => x.Name == "Return-Path").Select(x => x.Value).First();
                ret = ret.Replace("<", "").Replace(">", "").Trim();
            } catch (Exception ex) {
                ret = ms.Payload.Headers.Where(x => x.Name == "From").Select(x => x.Value).First();
                int left = ret.IndexOf("<");
                int right = ret.IndexOf(">");
                ret = ret.Substring(left + 1, right - left);
                ret = ret.Replace("<", "").Replace(">", "").Trim();
            }

            return ret;
        }

        public List<KeyValuePair<string, string>> GetAttachmentDescriptions(string mailId) {
            Message ms = service.Users.Messages.Get(user_email, mailId).Execute();
            return ms.Payload.Parts
                .Where(x => !string.IsNullOrEmpty(x.Filename))
                .Select(x => new KeyValuePair<string, string>(x.Body.AttachmentId, x.Filename)).ToList();
        }

        public SMessage ToSMessage(string mailId) {

            SMessage msg = new SMessage();
            Message ms = service.Users.Messages.Get(user_email, mailId).Execute();

            msg.id = mailId;
            msg.from = GetMailSender(mailId);
            msg.subject = ms.Payload.Headers.Where(x => x.Name == "Subject").Select(x => x.Value).First();
            msg.content = ms.Snippet;
            List<KeyValuePair<string, string>> all = GetAttachmentDescriptions(mailId);
            msg.attachmentIds = all.Select(x => x.Key).ToArray();
            msg.attachmentFiles = all.Select(x => x.Value).ToArray();

            return msg;
        }

        public string DownloadAttachment(string mailId, string attachmentId) {
            MessagePartBody attachPart = service.Users.Messages.Attachments.Get(user_email, mailId, attachmentId).Execute();
            // Converting from RFC 4648 base64 to base64url encoding
            // see http://en.wikipedia.org/wiki/Base64#Implementations_and_history
            String attachData = attachPart.Data.Replace('-', '+');
            attachData = attachData.Replace('_', '/');

            return attachData;
        }

        //MessagePartBody attachPart = service.Users.Messages.Attachments.Get(user_email, ms.Id, attId).Execute();
        //string attachData = attachPart.Data.Replace('-', '+').Replace('_', '/');

        #region // auxiliary //
        private static string Encode(string text) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);

            return System.Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
        #endregion

    }
}
