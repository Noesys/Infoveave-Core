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
    public enum FormulaElementType
    {
        Measure = 1,
        Value = 2,
        Expression = 3
    }

    public class FormulaElement
    {
        public long Id { get; set; }
        public string Name { get; set; }


        private int Type { get; set; }

        public virtual FormulaElementType ElementType
        {
            get
            {
                return (FormulaElementType)this.Type;
            }
            set
            {
                this.Type = (int)value;
            }
        }
        public string key { get; set; }

        public long? MeasureId { get; set; }

        public bool UserManaged { get; set; }

        public int ValueType { get; set; }

        public float MaxRange { get; set; }
        public float MinRange { get; set; }

        public float Step { get; set; }

        public string Expression { get; set; }

        public string ExtendedExpression { get; set; }
        public float Value { get; set; }

        public string LinkTo { get; set; }

        public long FormulaId { get; set; }
        public long DataSourceId { get; set; }
        public virtual ICollection<FormulaElementDimension> FormulaElementDimensions { get; set; }

    }
}
