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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

namespace Infoveave.Mailer
{
    public interface IMailer
    {
        void Configure(IHostingEnvironment appEnv, ILoggerFactory logger, string smtpServer, int smtpPort, string smtpUser, string smtpPassword, string[] copyBCC, string fromName, string fromAddress);
        Task<bool> SendMail(List<Recipient> to, string subject, string htmlBody, List<Recipient> cc = null, List<Recipient> bcc = null, List<System.IO.FileInfo> attachments = null);
        string MergeTemplate(string language, string templateName, Dictionary<string, string> data);
    }

    public class Recipient
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
    }
}

