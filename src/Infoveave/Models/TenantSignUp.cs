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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Infoveave.ViewModels
{
    /// <summary>
    /// Basic Information required for creation of a tenant
    /// </summary>
    public class TenantSignup
    {
        /// <summary>
        /// Organisation Name
        /// </summary>
        [Required]
        [RegularExpression("^[A-Za-z]{1}[A-Za-z0-9]*$")]
        [MinLength(3)]
        [MaxLength(50)]
        public string OrganisationName { get; set; }

        /// <summary>
        /// Domain Name will be used as {xxx}.infoveave.cloud
        /// </summary>
        [Required]
        [RegularExpression("^[a-z]{1}[a-z0-9]*$")]
        [MinLength(3)]
        [MaxLength(50)]
        public string DomainName { get; set; }

        /// <summary>
        /// Username of the Administrator
        /// </summary>
        [Required]
        [RegularExpression("^[A-Za-z]{1}[A-Za-z0-9]*$")]
        [MinLength(3)]
        [MaxLength(50)]
        public string UserName { get; set; }


        /// <summary>
        /// Password of the Administrator
        /// </summary>
        [Required]
        [MinLength(6)]
        [MaxLength(40)]
        public string Password { get; set; }

        /// <summary>
        /// E-Mail Address of the Administrator
        /// </summary>
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

    }
}
