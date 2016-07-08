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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Infoveave.Models
{
    public class Dimension
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string Query { get; set; }
        public int DrillDownLevel { get; set; }
        public long DataSourceId { get; set; }
        public bool IsDate { get; set; }
        public bool IsRangeDimension { get; set; }
        public string RangeDimension { get; set; }
        public string ColumnName { get; set; }

        public virtual DataSource DataSource { get; set; }
    }
}