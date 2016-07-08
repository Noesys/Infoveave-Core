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
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infoveave.Data.Implementations
{
    public class DataRepository<T> : Interfaces.IRepository<T>  where T : class
    {
        protected DbContext Context { get; set; }
        protected DbSet<T> Data { get; set; }
        public DataRepository(DbContext context)
        {
            this.Context = context;
            this.Data = context.Set<T>();
        }
        public IQueryable<T> GetAll()
        {
            return Data;
        }
        public IQueryable<T> GetAll<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> path)
        {
            return Data.Include(path);
        }

        public void Add(T entity)
        {
            Data.Add(entity);
        }
        public void Add(IEnumerable<T> entities)
        {
            Data.AddRange(entities);
        }

        public void Delete(T entity)
        {
            Data.Remove(entity);
        }

        public void Delete(IEnumerable<T> entities)
        {
            Data.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            Data.Update(entity);
        }

    }
}
