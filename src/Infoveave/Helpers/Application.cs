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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Infoveave.Helpers
{

    public interface IApplicationConfiguration
    {
        ApplicationData Application { get; set; }
    }

    public class ApplicationConfiguration
    {
        public ApplicationData Application { get; set; }

    }

    public class ApplicationData
    {
        public string TenantDatabaseLocation { get; set; }

        public Dictionary<string, string> Paths { get; set; }

        public AnalyticsData AnalyticsData { get; set; }
        public CachingInformation Caching { get; set; }
        public MondrianService MondrianService { get; set; }

        public Mailer Mailer { get; set; }
    }

    public class CachingInformation
    {
        public string Endpoint { get; set; }
        public int Port { get; set; }
        public bool Enabled { get; set; }
    }

    public class AnalyticsData
    {
        public string AnalyticsAdapter { get; set; }
        public string SQLAdapter { get; set; }
        public string ConnectionString { get; set; }
    }

    public class MondrianService
    {
        public string Endpoint { get; set; }
        public int Port { get; set; }
    }

    public class Mailer
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public string FromName { get; set; }
        public string FromAddress { get; set; }
        public string CopyAll { get; set; }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member