﻿/* Copyright © 2015-2016 Noesys Software Pvt.Ltd. - All Rights Reserved
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
using System.ComponentModel.DataAnnotations;

namespace Infoveave.Models
{
    public class AnalysisScenario
    {
        public long Id { get; set; }
        public long AnalysisId { get; set; }
        public string Name { get; set; }
        public string AnalysisData { get; set; }
        public virtual Analysis Analysis { get; set; }
        public bool IsPublic { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}