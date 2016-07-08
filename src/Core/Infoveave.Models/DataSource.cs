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
using System.ComponentModel.DataAnnotations.Schema;

namespace Infoveave.Models
{
    public class DataSource
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public long TypeId { get; set; }

        public string ServerType { get; set; }

        public string Server { get; set; }

        public string AnalysisDataBase { get; set; }

        public string Cube { get; set; }

        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "text")]
        [MaxLength]
        public string ColumnMappings { get; set; }

        [Column(TypeName = "text")]
        [MaxLength]
        public string ValidationSchema { get; set; }
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public virtual ICollection<Measure> Measures { get; set; }
        public virtual ICollection<Dimension> Dimensions { get; set; }

        public bool IsPublic { get; set; }
        public long CreatedBy { get; set; }       
    }
}