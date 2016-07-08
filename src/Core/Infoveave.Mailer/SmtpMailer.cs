/* Copyright © 2015-2016 Noesys Software Pvt.Ltd. - All Rights Reserved
 * -------------
 * This file is part of Infoveave.
 * Infoveave is dual licensed under Infoveave Commercial License and AGPL v3  
 * -------------
 * You should have received a copy of the GNU Affero General Public License v3
 * along with this program (Infoveave)
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the Infoveave without
 * disclosing the source code of your own applications.
 * -------------
 * Authors: Naresh Jois <naresh@noesyssoftware.com>, et al.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using MimeKit;
using System.IO;
using MailKit.Net.Smtp;

namespace Infoveave.Mailer
{
    public class SmtpMailer : IMailer
    {
        protected ILogger Logger { get; set; }
     
        protected IHostingEnvironment ApplicationEnvironment { get; set; }

        protected string[] CopyBCC { get; set; }

        protected string FromName { get; set; }
        protected string FromAddress { get; set; }

        protected string Domain { get; set; }


        protected string SmtpServer { get; set; }
        protected int SmtpPort { get; set; }
        protected string SmtpUser { get; set; }
        protected string SmtpPassword { get; set; }


        public void Configure(IHostingEnvironment appEnv, ILoggerFactory logger,string smtpServer, int smtpPort, string smtpUser, string smtpPassword, string[] copyBCC, string fromName, string fromAddress)
        {
            ApplicationEnvironment = appEnv;
            Logger = logger.CreateLogger("Mailer");
            CopyBCC = copyBCC;
            FromName = FromName;
            FromAddress = fromAddress;
            SmtpServer = smtpServer;
            SmtpPort = smtpPort;
            SmtpUser = smtpUser;
            SmtpPassword = smtpPassword;
        }


        public async Task<bool> SendMail(List<Recipient> to, string subject, string htmlBody, List<Recipient> cc = null, List<Recipient> bcc = null, List<System.IO.FileInfo> attachments = null)
        {
            var message = new MimeMessage();
            to.ForEach((t) => { message.To.Add(new MailboxAddress(t.DisplayName, t.Email)); });
            if (cc != null)
            {
                cc.ForEach((t) => { message.Cc.Add(new MailboxAddress(t.DisplayName, t.Email)); });
            }
            if (bcc != null)
            {
                bcc.ForEach((t) => { message.Bcc.Add(new MailboxAddress(t.DisplayName, t.Email)); });
            }
            if (CopyBCC != null)
            {
                CopyBCC.ToList().ForEach((t) => { message.To.Add(new MailboxAddress("CopyAll", t)); });
            }
            message.From.Add(new MailboxAddress(FromName,FromAddress));
            message.Subject = subject;
            var body = new TextPart("html")
            {
                Text = htmlBody
            };
            var multipart = new Multipart("mixed");
            multipart.Add(body);
            if (attachments != null)
            {
                attachments.ForEach(a => {
                    var attachment = new MimePart()
                    {
                        ContentObject = new ContentObject(a.OpenRead(), ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = a.Name
                    };
                    multipart.Add(attachment);
                });
            }

            message.Body = multipart;
            Logger.LogInformation("Sending Email to {0} about {1}", to[0].Email, subject);
            using (var client = new SmtpClient())
            {
                client.Connect(SmtpServer, SmtpPort, false);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(SmtpUser, SmtpPassword);
                try
                {
                    client.Send(message);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Sending Email to {0} about {1} Falied : {2}",to[0].Email, subject,ex.Message);
                }
                finally
                {
                    client.Disconnect(true);
                }
               
                
            }
            await Task.Delay(0);
            return true;
        }

        public string MergeTemplate(string language, string templateName, Dictionary<string, string> data)
        {

            var template = System.IO.File.ReadAllText(System.IO.Path.Combine(ApplicationEnvironment.ContentRootPath, "Views", "Mails", language, templateName + ".hbs"));
            StringBuilder sb = new StringBuilder(template);
            foreach (var value in data)
            {
                sb.Replace("{" + value.Key + "}", value.Value);
            }
            return sb.ToString();
        }
    }

}
