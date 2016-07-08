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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infoveave.Models
{
    public class User
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [NotMapped]
        public virtual string DisplayName { get { return FirstName + " " + LastName; } }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }

        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }

        public long UserRoleId { get; set; }
        public virtual UserRole UserRole { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? LastLoginDateTime { get; set; }

        public bool IsArchived { get; set; }
        public bool IsLockedOut { get; set; }

        public DateTime? LockoutDate { get; set; }

        public int LoginAttempts { get; set; }
        public string Language { get; set; }
        public string ImagePath { get; set; }


        [Column(TypeName = "text")]
        [MaxLength]
        public string ContextInfo { get; set; }


        [NotMapped]
        public virtual List<UserContext> Context
        {
            get
            {
                return (string.IsNullOrEmpty(ContextInfo)) ? new List<UserContext>() : JsonConvert.DeserializeObject<List<UserContext>>(ContextInfo);
            }
            set
            {
                ContextInfo = JsonConvert.SerializeObject(value);
            }
        }

        public void SetPassword(string password)
        {
            PasswordSalt = BCrypt.GenerateSalt();
            PasswordHash = BCrypt.HashPassword(password, PasswordSalt);
        }
        public bool ValidatePassword(string password)
        {
            return BCrypt.Verify(password, PasswordHash);
        }
        public void UnlockUser()
        {
            IsLockedOut = false;
            LoginAttempts = 0;
            LockoutDate = DateTime.Now;
        }
    }


    public class UserContext
    {
        public long DataSourceId { get; set; }
        public long DimensionId { get; set; }
        public string Query { get; set; }
        public string[] Items { get; set; }

    }

}
