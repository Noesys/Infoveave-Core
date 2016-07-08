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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infoveave.Data.Interfaces
{
    public interface ITenantContext
    {
        void SetDatabaseConnection(bool useMultiTenancy, string baseConnectionString);
        IDataUow CreateTenantRepository(string tenant);
        IDataUow GetTenantRepository(string tenant);

        bool TenantExists(string tenant);
    }
}
