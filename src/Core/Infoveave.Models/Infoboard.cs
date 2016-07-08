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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infoveave.Models
{
    public class Infoboard
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public string ShortCode { get; set; }
        public virtual List<InfoboardLink> Items { get; set; }
        public string InfoboardOptions { get; set; }
        public bool IsPublic { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? SortOrder { get; set; }

        [Column(TypeName = "text")]
        [MaxLength]
        public string Layouts { get; set; }



    }

    public class Widget
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ItemType { get; set; }
        public string SavedData { get; set; }
        public long TimeProgression { get; set; }
        public string ItemOptions { get; set; }
        public string ShortCode { get; set; }
        public string DataSourceIds { get; set; }
        public string FullName { get; set; }
        public bool IsPublic { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }


    public class WidgetAnnotation
    {
        public long Id { get; set; }

        public long WidgetId { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        [Column(TypeName = "text")]
        [MaxLength]
        public string AnnotationData { get; set; }

        [Column(TypeName = "text")]
        [MaxLength]
        public string AnnotationContent { get; set; }

        public virtual Widget Widget { get; set; }

        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

    }

    public class InfoboardLink
    {
        public long Id { get; set; }
        public long InfoboardId { get; set; }
        public long WidgetId { get; set; }
        public virtual Infoboard Infoboard { get; set; }
        public virtual Widget Widget { get; set; }
        public string SavedViewShortCode { get; set; }
        public int HorizontalSize { get; set; }
        public int VerticalSize { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
    }
}
