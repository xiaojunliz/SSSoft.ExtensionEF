// ***********************************************************************
// Assembly         : HySoft.Core.Framework
// Author           : 李小军
// Created          : 08-04-2014
//
// Last Modified By : 李小军
// Last Modified On : 08-04-2014
// ***********************************************************************
// <copyright file="ExtOrderBy.cs" company="赛思网络科技">
//     Copyright (c) 赛思网络科技. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Linq;
using System.Linq.Expressions;

namespace HySoft.Core.Framework
{
    /// <summary>
    /// Class ExtOrderBy
    /// </summary>
    internal static class ExtOrderBy
    {
        #region 排序扩展
        /// <summary>
        /// The obj lock
        /// </summary>
        private static Object objLock = new Object();
        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns>IOrderedQueryable{``0}.</returns>
        internal static IOrderedQueryable<T> OrderByExt<T>(this IQueryable<T> query, string memberName)
        {
            return query.GenericOrderBy(memberName, "OrderBy");
        }

        /// <summary>
        /// Orders the by descending ext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns>IOrderedQueryable{``0}.</returns>
        internal static IOrderedQueryable<T> OrderByDescendingExt<T>(this IQueryable<T> query, string memberName)
        {
            return query.GenericOrderBy(memberName, "OrderByDescending");
        }
        /// <summary>
        /// Thens the by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns>IOrderedQueryable{``0}.</returns>
        internal static IOrderedQueryable<T> ThenByExt<T>(this IQueryable<T> query, string memberName)
        {
            return query.GenericOrderBy(memberName, "ThenBy");
        }
        /// <summary>
        /// Thens the by descending.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns>IOrderedQueryable{``0}.</returns>
        internal static IOrderedQueryable<T> ThenByDescendingExt<T>(this IQueryable<T> query, string memberName)
        {
            return query.GenericOrderBy(memberName, "ThenByDescending");
        }
        /// <summary>
        /// Generics the order by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="OrderType">Type of the order.</param>
        /// <returns>IOrderedQueryable{``0}.</returns>
        private static IOrderedQueryable<T> GenericOrderBy<T>(this IQueryable<T> query, string memberName,String OrderType)
        {
            lock (objLock)
            {
                ParameterExpression[] typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };

                System.Reflection.PropertyInfo pi = typeof(T).GetProperty(memberName);

                return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable),
                        OrderType,
                        new Type[] { typeof(T), pi.PropertyType },
                        query.Expression,
                        Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
                );
            }
        }
        #endregion
    }
}
