﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using System.Transactions;
using PayrollBureau.Data.Entities;
using PayrollBureau.Data.Interfaces;
using PayrollBureau.Data.Models;
using PayrollBureau.Data.Extensions;

namespace PayrollBureau.Data.Services
{
    public class PayrollBureauDataService : IPayrollBureauDataService
    {
        private IDatabaseFactory<PayrollBureauDatabase> _databaseFactory;

        public PayrollBureauDataService(IDatabaseFactory<PayrollBureauDatabase> databaseFactory)
        {
            _databaseFactory = databaseFactory;
        }

        private TransactionScope ReadUncommitedTransactionScope => new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted });

        public T Create<T>(T t) where T : class
        {
            using (var context = _databaseFactory.CreateContext())
            {
                t = context.Set<T>().Add(t);
                context.SaveChanges();
                return t;
            }
        }

        public IEnumerable<T> Create<T>(IEnumerable<T> t) where T : class
        {
            using (var context = _databaseFactory.CreateContext())
            {
                context.Set<T>().AddRange(t);
                context.SaveChanges();

            }
            return t.ToList();
        }

        //generic type use only when no other tables are needed
        public T Retrieve<T>(int Id) where T : class
        {
            using (ReadUncommitedTransactionScope)
            using (var context = _databaseFactory.CreateContext())
            {
                var returnItems = context.Set<T>().Find(Id);
                return returnItems;
            }
        }

        public List<T> Retrieve<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeExpressions) where T : class
        {
            using (ReadUncommitedTransactionScope)
            using (var context = _databaseFactory.CreateContext())
            {
                var query = context.Set<T>().AsQueryable<T>();
                if (includeExpressions?.Any() ?? false)
                    query = includeExpressions.Aggregate(query, (current, expression) => current.Include(expression));
                if (predicate != null)
                    query = query.Where(predicate);
                return query.ToList();
            }
        }

        //protected virtual IQueryable<T> RetrieveQueryable<T>(Expression<Func<T, bool>> predicate,Expression<Func<T, TResult>> selectExpression, params Expression<Func<T, object>>[] includeExpressions) where T : class
        //{
        //    using (ReadUncommitedTransactionScope)
        //    using (var context = _databaseFactory.CreateContext())
        //    {
        //        var query = context.Set<T>().AsQueryable<T>();
        //        if (includeExpressions?.Any() ?? false)
        //            query = includeExpressions.Aggregate(query, (current, expression) => current.Include(expression));
        //        if (predicate != null)
        //            query = query.Where(predicate);
        //        return query.Select(selectExpression);
        //    }
        //}


        //generic type use only when no other tables are needed
        public List<T> Retrieve<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            using (ReadUncommitedTransactionScope)
            using (var context = _databaseFactory.CreateContext())
            {
                var returnItems = context.Set<T>().Where(predicate).ToList();
                return returnItems;
            }
        }

        //generic type use only when no other tables are needed
        public PagedResult<T> RetrievePagedResult<T>(Expression<Func<T, bool>> predicate, List<OrderBy> orderBy = null, Paging paging = null) where T : class
        {
            using (ReadUncommitedTransactionScope)
            using (var context = _databaseFactory.CreateContext())
            {
                return context
                    .Set<T>()
                    .AsNoTracking()
                    .Where(predicate)
                    //if there is no name column on the table this will generate an error
                    .OrderBy(orderBy ?? new List<OrderBy>
                    {
                        new OrderBy
                        {
                            Property = "Name",
                            Direction = System.ComponentModel.ListSortDirection.Ascending
                        }
                    })
                    .Paginate(paging);
            }
        }

        public PagedResult<T> RetrievePagedResult<T>(Expression<Func<T, bool>> predicate, List<OrderBy> orderBy = null, Paging paging = null, params Expression<Func<T, object>>[] includeExpressions) where T : class
        {
           return RetrieveQueryable(predicate,orderBy,includeExpressions).Paginate(paging);
        }

        public PagedResult<AspNetUser> RetrievePagedResultBureauUsers<T>(int bureauId,string searchTerm, List<OrderBy> orderBy = null, Paging paging = null) 
        {
            using (ReadUncommitedTransactionScope)
            using (var context = _databaseFactory.CreateContext())
            {
                var users = context
                    .AspNetUsers
                    .Include(u => u.AspNetUsersBureau)
                    .AsNoTracking()
                    .Where(u => u.AspNetUsersBureau.Any(s=>s.BureauId==bureauId));
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    users = users.Where(u => u.UserName.ToUpper().Contains(searchTerm.ToUpper()) && u.Email.ToUpper().Contains(searchTerm.ToUpper()));

                return  users
                    .OrderBy(orderBy)
                    .Paginate(paging);
            }
        }
        protected virtual IQueryable<T> RetrieveQueryable<T>(Expression<Func<T, bool>> predicate, List<OrderBy> orderBy = null, params Expression<Func<T, object>>[] includeExpressions) where T : class
        {
            using (ReadUncommitedTransactionScope)
            using (var context = _databaseFactory.CreateContext())
            {
                var query = context.Set<T>().AsQueryable<T>();

                if (includeExpressions?.Any() ?? false)
                    query = includeExpressions.Aggregate(query, (current, expression) => current.Include(expression));

                if (predicate != null)
                    query = query.Where(predicate);

                if (orderBy?.Any() ?? false)
                    query = query.OrderBy(orderBy);

                return query;
            }
        }

        public T UpdateEntityEntry<T>(T t) where T : class
        {
            using (var context = _databaseFactory.CreateContext())
            {
                context.Entry(t).State = EntityState.Modified;
                context.SaveChanges();

                return t;
            }
        }

        public void Delete<T>(int Id) where T : class
        {
            using (var context = _databaseFactory.CreateContext())
            {
                var item = context.Set<T>().Find(Id);
                context.Set<T>().Remove(item);
                context.SaveChanges();
            }
        }

        public void Delete<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            using (var context = _databaseFactory.CreateContext())
            {
                var items = context.Set<T>().Where(predicate).FirstOrDefault();
                if (items != null)
                {
                    context.Set<T>().Remove(items);
                    context.SaveChanges();
                }
            }
        }

        public void DeleteRange<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            using (var context = _databaseFactory.CreateContext())
            {
                var items = context.Set<T>().Where(predicate);
                context.Set<T>().RemoveRange(items);
                context.SaveChanges();
            }
        }
    }
}
