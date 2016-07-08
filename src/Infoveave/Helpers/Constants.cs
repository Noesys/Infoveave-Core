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
using System.Security.Claims;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Infoveave.Helpers
{
    public static class Adapters {
        public static Dictionary<string, AdapterFramework.IOlapAdapter> OlapAdapters { get; set; }
        public static Dictionary<string, AdapterFramework.ISQLAdapter> SQLAdapters { get; set; }
    }
    public class Constants
    {
        public static string Id { get { return "id"; } }
        public static string Tenant { get { return "tenant"; } }
        public static string UserName { get { return "userName"; } }
        public static string Email { get { return "userEmail"; } }
        public static string Role { get { return "RoleId"; } }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member