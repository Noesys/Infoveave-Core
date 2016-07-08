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
using FluentDate;
using FluentDateTime;

#pragma warning disable CS1591
namespace Infoveave.Helpers
{
    public class StartEndDate
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class DateHelpers
    {
        public static StartEndDate GetStateEndDate(int progression)
        {
            var startDate = DateTime.Now;
            var endDate = DateTime.Now;
            switch (progression)
            {
                case 101:
                    startDate = startDate.FirstDayOfWeek();
                    endDate = endDate.LastDayOfWeek();
                    break;
                case 102:
                    startDate = 7.Days().Ago().FirstDayOfWeek();
                    endDate = endDate.LastDayOfWeek();
                    break;
                case 103:
                    startDate = 21.Days().Ago().FirstDayOfWeek();
                    endDate = endDate.LastDayOfWeek();
                    break;
                case 201:
                    startDate = startDate.FirstDayOfMonth();
                    endDate = endDate.LastDayOfMonth();
                    break;
                case 202:
                    startDate = 1.Months().Ago().FirstDayOfMonth();
                    endDate = endDate.LastDayOfMonth();
                    break;
                case 203:
                    startDate = 2.Months().Ago().FirstDayOfMonth();
                    endDate = endDate.LastDayOfMonth();
                    break;
                case 301:
                    startDate = startDate.FirstDayOfQuarter();
                    endDate = endDate.LastDayOfQuarter();
                    break;
                case 302:
                    startDate = 1.Quarters().Ago().FirstDayOfQuarter();
                    endDate = endDate.LastDayOfQuarter();
                    break;
                case 401:
                    startDate = startDate.FirstDayOfYear();
                    endDate = endDate.LastDayOfYear();
                    break;
                case 402:
                    startDate = 1.Years().Ago().FirstDayOfYear();
                    endDate = endDate.LastDayOfYear();
                    break;
                default:
                    startDate = startDate.FirstDayOfMonth();
                    endDate = endDate.LastDayOfMonth();
                    break;
            }
            return new StartEndDate { StartDate = startDate, EndDate = endDate };

        }
    }
}
#pragma warning restore CS1591