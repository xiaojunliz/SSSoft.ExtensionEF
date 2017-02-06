// ***********************************************************************
// Assembly         : HySoft.Core.Framework
// Author           : 李小军
// Created          : 08-04-2014
//
// Last Modified By : 李小军
// Last Modified On : 08-04-2014
// ***********************************************************************
// <copyright file="ExtEFFunction.cs" company="赛思网络科技">
//     Copyright (c) 赛思网络科技. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Objects;
using System.Data;
using System.Data.EntityClient;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Objects.DataClasses;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using EntityState = System.Data.Entity.EntityState;

// <summary>
// =================================================================================================
//  系统名         : 
//  子系统名       : 针对EF6.0扩展
//  文件名         : ExtEFFunction.cs
//  概要           : 
//  版本           : 1.0.0
//  Rev -  Date------- Name --------- Note -------------------------------
//  1.0.0  2016.12.05  李小军        创建
//  ================================================================================================
//</summary>
namespace HySoft.Core.Framework
{
    /// <summary>
    /// Extension EF Function
    /// </summary>
    public static class ExtEFFunction
    {
        /// <summary>
        /// The obj lock
        /// </summary>
        private static readonly Object ObjLock = new Object();

        /// <summary>
        /// The split chart
        /// </summary>
        const Char SplitChart = ',';

        #region EF base Function

        /// <summary>
        /// Inserts the model.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <returns>Int32.</returns>
        public static Int32 InsertModel<TEntity>(this DbContext context, TEntity entity, Action<DbContext, TEntity> action = null) where TEntity : class
        {
            lock (ObjLock)
            {
                InsertModelWithNotSave(context, entity);
                if (action != null)
                {
                    action(context,entity);
                }
                return context.SaveChanges();
            }
        }

        public static int DeleteModel<TEntity>(this DbContext context, TEntity entity, Action<DbContext, TEntity> action = null) where TEntity : class
        {
            lock (ObjLock)
            {
                //context.Entry<TEntity>(entity).State = EntityState.Deleted;
                //var entry = context.Entry(entity).State = EntityState.Deleted;
                //if (entry.State == System.Data.Entity.EntityState.Detached)
                //{
                //    entry.State = System.Data.Entity.EntityState.Deleted;
                //}

                var entry = context.Entry(entity);
                if (entry.State == System.Data.Entity.EntityState.Detached)
                {
                    context.Set<TEntity>().Remove(entity);
                    if (action != null)
                    {
                        action(context, entity);
                    }
                }
                return context.SaveChanges();
            }
        }

        /// <summary>
        /// Inserts the model with not save.
        /// </summary>
        /// <typeparam name="TEntity">The type of the T entity.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        internal static void InsertModelWithNotSave<TEntity>(this DbContext context, TEntity entity, Action<DbContext, TEntity> action = null) where TEntity : class
        {
            lock (ObjLock)
            {
                var entry = context.Entry(entity);
                if (entry.State == System.Data.Entity.EntityState.Detached)
                {
                    context.Set<TEntity>().Add(entity);
                    if (action != null)
                    {
                        action(context, entity);
                    }
                }
            }
        }

        /// <summary>
        /// Inserts the model.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="listEntity">The list entity.</param>
        /// <param name="action">The action.</param>
        /// <returns>Int32.</returns>
        public static Int32 InsertModelList<TEntity>(this DbContext context, IEnumerable<TEntity> listEntity, Action<DbContext, IEnumerable<TEntity>> action = null) where TEntity : class
        {
            lock (ObjLock)
            {
                foreach (var entity in listEntity)
                {
                    InsertModelWithNotSave(context, entity);

                }
                if (action != null)
                {
                    action(context,listEntity);
                }
                return context.SaveChanges();
            }
        }
        
        /// <summary>
        /// Update the model by condition.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="updateColumns">Name of the update property.</param>
        /// <param name="action">The action.</param>
        /// <returns>Int32.</returns>
        public static Int32 UpdateModelByCondition<TEntity>(this DbContext context, TEntity entity, Expression<Func<TEntity, bool>> condition, String updateColumns, Action<DbContext, IEnumerable<TEntity>> action = null) where TEntity : class
        {
            lock (ObjLock)
            {
                var listEntity = (context.Set<TEntity>()).Where(condition).ToList();
                if (listEntity.Any())
                {
                    for (int i = 0; i < listEntity.Count; i++)
                    {
                        PropertyInfo[] pis = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        if (!String.IsNullOrWhiteSpace(updateColumns))
                        {
                            foreach (var updateItem in updateColumns.Split(SplitChart))
                            {
                                if (pis.Any(p => p.Name.Contains(updateItem)) && !string.IsNullOrWhiteSpace(updateItem))
                                {
                                    PropertyInfo info = listEntity[i].GetType().GetProperty(updateItem);
                                    PropertyInfo infoEntity = entity.GetType().GetProperty(updateItem);
                                    if (info != null)
                                    {
                                        object value = infoEntity.GetValue(entity, null);
                                        info.SetValue(listEntity[i], value, null);
                                    }
                                    context.Entry<TEntity>(listEntity[i]).Property(updateItem).IsModified = true;
                                }
                            }
                        }
                    }
                }

                if (action != null)
                {
                    action(context, listEntity);
                }
                return context.SaveChanges();
            }
        }

        ///// <summary>
        ///// Deletes the model.
        ///// </summary>
        ///// <typeparam name="TEntity">The type of the entity.</typeparam>
        ///// <param name="context">The context.</param>
        ///// <param name="entity">The entity.</param>
        ///// <param name="action">The action.</param>
        ///// <returns>Int32.</returns>
        //public static Int32 DeleteModel<TEntity>(this ObjectContext context, TEntity entity, Action<ObjectContext, TEntity> action = null) where TEntity : class
        //{
        //    lock (ObjLock)
        //    {
        //        DeleteModelWithNotSave(context, entity);
        //        if (action != null)
        //        {
        //            action(context, entity);
        //        }
        //        return context.SaveChanges();
        //    }
        //}

        ///// <summary>
        ///// Deletes the model with not save.
        ///// </summary>
        ///// <typeparam name="TEntity">The type of the T entity.</typeparam>
        ///// <param name="context">The context.</param>
        ///// <param name="entity">The entity.</param>
        ///// <param name="action">The action.</param>
        //internal static void DeleteModelWithNotSave<TEntity>(this ObjectContext context, TEntity entity, Action<ObjectContext, TEntity> action = null) where TEntity : class
        //{
        //    lock (ObjLock)
        //    {
        //        string entitySetName = context.GetEntityName<TEntity>();
        //        context.AttachExistedEntity(entity);
        //        context.ApplyOriginalValues(entitySetName, entity);
        //        if (action != null)
        //        {
        //            action(context, entity);
        //        }
        //        context.ObjectStateManager.ChangeObjectState(entity, EntityState.Deleted);
        //    }
        //}

        ///// <summary>
        ///// Deletes the model list.
        ///// </summary>
        ///// <typeparam name="TEntity">The type of the entity.</typeparam>
        ///// <param name="context">The context.</param>
        ///// <param name="listEntity">The list entity.</param>
        ///// <param name="action">The action.</param>
        ///// <returns>Int32.</returns>
        //public static Int32 DeleteModelList<TEntity>(this DbContext context, IEnumerable<TEntity> listEntity, Action<DbContext, IEnumerable<TEntity>> action = null) where TEntity : class
        //{
        //    lock (ObjLock)
        //    {
        //        string entitySetName = context.GetEntityName<TEntity>();
        //        foreach (var entity in listEntity)
        //        {
        //            context.AttachExistedEntity(entity);
        //            context.ApplyOriginalValues(entitySetName, entity);
        //            context.Configuration.ObjectStateManager.ChangeObjectState(entity, EntityState.Deleted);
        //        }
        //        if (action != null)
        //        {
        //            action(context, listEntity);
        //        }
        //        return context.SaveChanges();
        //    }
        //}

        ///// <summary>
        ///// Deletes the model by condition.
        ///// </summary>
        ///// <typeparam name="TEntity">The type of the entity.</typeparam>
        ///// <param name="context">The context.</param>
        ///// <param name="condition">The condition.</param>
        ///// <param name="action">The action.</param>
        ///// <returns>Int32.</returns>
        //public static Int32 DeleteModelByCondition<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> condition, Action<DbContext, IEnumerable<TEntity>> action = null) where TEntity : class
        //{
        //    lock (ObjLock)
        //    {
        //        var query = (context.Set<TEntity>()).Where(condition);
        //        if (query.Any())
        //        {
        //            query.ToList().ForEach((item) =>
        //            {
        //                context.DeleteModel(item);
        //            });
                    
        //        }
        //        if (action != null)
        //        {
        //            action(context, query);
        //        }
        //        return context.SaveChanges();
        //    }
        //}
        #endregion

        #region EF Find by condition

        /// <summary>
        /// Finds all by page.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="includeChild">The include child.</param>
        /// <param name="orderByKeys">The order bykeys.</param>
        /// <returns>PageData{``0}.</returns>
        /// <exception cref="System.ArgumentException">orderByKey不存在...</exception>
        public static PageData<T> FindAllByPage<T>(this DbContext context, int pageIndex, int pageSize, Expression<Func<T, bool>> condition = null, IList<String> includeChild = null, IList<OrderKeyAndValue> orderByKeys = null) where T : class, new()
        {

            PageData<T> pageData = new PageData<T>();
            System.Data.Entity.Infrastructure.DbQuery<T> query = context.Set<T>();

            if (includeChild == null)
            {
                includeChild = new List<String>() { };
            }
            foreach (var child in includeChild)
            {
                query = query.Include(child);
            }

            if (condition != null)
            {
                query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.Where(condition);
            }
            pageData.totalCount = query.Count();

            if (orderByKeys != null && orderByKeys.Count > 0)
            {
                Boolean isFirstOrderBy = false;
                Type sourceTtype = typeof(T);

                foreach (var item in orderByKeys)
                {
                    PropertyInfo keyProperty = sourceTtype.GetProperties().FirstOrDefault(p => p.Name.ToLower().Equals(item.key.Trim().ToLower()));
                    if (keyProperty == null)
                    {
                        throw new ArgumentException("orderByKey不存在...");
                    }

                    if (!isFirstOrderBy)
                    {
                        if (item.value == true)
                        {
                            //query = (ObjectQuery<T>)query.OrderByDescending(orderByExpression);
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.OrderByDescendingExt(item.key);
                        }
                        else
                        {
                            //query = (ObjectQuery<T>)query.OrderBy(orderByExpression);
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.OrderByExt(item.key);
                        }
                        isFirstOrderBy = true;
                    }
                    else
                    {
                        if (item.value == true)
                        {
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.ThenByDescendingExt(item.key);
                        }
                        else
                        {
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.ThenByExt(item.key);
                        }
                    }
                }
            }
            else
            {
                string primarykey = "";
                PropertyInfo[] prop = typeof(T).GetProperties();
                foreach (PropertyInfo item in prop)
                {
                    var temp = item.GetCustomAttributes(true).FirstOrDefault(t => t.ToString() == typeof(EdmScalarPropertyAttribute).FullName);
                    if (temp != null && ((temp as EdmScalarPropertyAttribute).EntityKeyProperty))
                    {
                        primarykey = item.Name;
                        break;
                    }
                }
                if (primarykey != "")
                {
                    query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.OrderByDescendingExt(primarykey);
                }
            }

            if (context.Configuration.LazyLoadingEnabled)
            {
                pageData.dataList = query
                .Skip(pageIndex * pageSize).Take(pageSize).AsNoTracking().ToList();
            }
            else
            {
                pageData.dataList = query
                .AsQueryable()
                .Skip(pageIndex * pageSize).Take(pageSize).AsNoTracking().ToList();
            }
            
            return pageData;
        }

        /// <summary>
        /// Finds all list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="includeChild">The include child.</param>
        /// <param name="orderByKeys">The order bykeys.</param>
        /// <returns>PageData&lt;T&gt;.</returns>
        /// <exception cref="System.ArgumentException">orderByKey不存在...</exception>
        public static PageData<T> FindAllList<T>(this DbContext context, Expression<Func<T, bool>> condition = null, IList<String> includeChild = null, IList<OrderKeyAndValue> orderByKeys = null) where T : class, new()
        {
            PageData<T> pageData = new PageData<T>();
            System.Data.Entity.Infrastructure.DbQuery<T> query = context.Set<T>();

            if (includeChild == null)
            {
                includeChild = new List<String>() { };
            }
            foreach (var child in includeChild)
            {
                query = query.Include(child);
            }

            if (condition != null)
            {
                query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.Where(condition);
            }

            if (orderByKeys != null && orderByKeys.Count > 0)
            {
                Boolean isFirstOrderBy = false;
                Type sourceTtype = typeof(T);
                foreach (var item in orderByKeys)
                {
                    PropertyInfo keyProperty = sourceTtype.GetProperties().FirstOrDefault(p => p.Name.ToLower().Equals(item.key.Trim().ToLower()));
                    if (keyProperty == null)
                    {
                        throw new ArgumentException("orderByKey不存在...");
                    }

                    if (!isFirstOrderBy)
                    {
                        if (item.value == true)
                        {
                            //query = (ObjectQuery<T>)query.OrderByDescending(orderByExpression);
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.OrderByDescendingExt(item.key);
                        }
                        else
                        {
                            //query = (ObjectQuery<T>)query.OrderBy(orderByExpression);
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.OrderByExt(item.key);
                        }
                        isFirstOrderBy = true;
                    }
                    else
                    {
                        if (item.value == true)
                        {
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.ThenByDescendingExt(item.key);
                        }
                        else
                        {
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.ThenByExt(item.key);
                        }
                    }
                }
            }
            if (context.Configuration.LazyLoadingEnabled)
            {
                pageData.dataList = query.ToList();
                pageData.totalCount = pageData.dataList.Count();
                return pageData;
            }
            else
            {
                pageData.dataList = query.AsQueryable().ToList();//.Execute(MergeOption.NoTracking).ToList();
                pageData.totalCount = pageData.dataList.Count();
                return pageData;

            }
        }

        /// <summary>
        /// Distincts the specified context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="fieldNames">The field names.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="orderByKeys">The order bykeys.</param>
        /// <returns>List{dynamic}.</returns>
        /// <exception cref="System.ArgumentException">orderByKey不存在...</exception>
        public static List<dynamic> Distinct<T>(this DbContext context, IEnumerable<String> fieldNames, Expression<Func<T, bool>> condition = null, IList<OrderKeyAndValue> orderByKeys = null) where T : class, new()
        {
            System.Data.Entity.Infrastructure.DbQuery<T> query = context.Set<T>();

            if (condition != null)
            {
                query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.Where(condition);
            }

            if (orderByKeys != null && orderByKeys.Count > 0)
            {
                Boolean isFirstOrderBy = false;
                Type sourceTtype = typeof(T);
                foreach (var item in orderByKeys)
                {
                    PropertyInfo keyProperty = sourceTtype.GetProperties().FirstOrDefault(p => p.Name.ToLower().Equals(item.key.Trim().ToLower()));
                    if (keyProperty == null)
                    {
                        throw new ArgumentException("orderByKey不存在...");
                    }

                    if (!isFirstOrderBy)
                    {
                        if (item.value == true)
                        {
                            //query = (ObjectQuery<T>)query.OrderByDescending(orderByExpression);
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.OrderByDescendingExt(item.key);
                        }
                        else
                        {
                            //query = (ObjectQuery<T>)query.OrderBy(orderByExpression);
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.OrderByExt(item.key);
                        }
                        isFirstOrderBy = true;
                    }
                    else
                    {
                        if (item.value == true)
                        {
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.ThenByDescendingExt(item.key);
                        }
                        else
                        {
                            query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.ThenByExt(item.key);
                        }
                    }
                }
            }
            return query.SelectPartially(fieldNames).Distinct().ToList();
        }

        /// <summary>
        /// Finds Entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="includeChild">The include child.</param>
        /// <returns>``0.</returns>
        public static T FindEntity<T>(this DbContext context, Expression<Func<T, bool>> condition = null, List<String> includeChild = null) where T : class, new()
        {
            System.Data.Entity.Infrastructure.DbQuery<T> query = context.Set<T>();

            if (includeChild == null)
            {
                includeChild = new List<String>() { };
            }
            foreach (var child in includeChild)
            {
                query = query.Include(child);
            }

            if (condition != null)
            {
                query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.Where(condition);
            }
            if (context.Configuration.LazyLoadingEnabled)
            {
                return query.AsNoTracking().FirstOrDefault();
            }
            else
            {
                return query
                    .AsNoTracking()
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Finds Entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="includeChild">The include child.</param>
        /// <returns>``0.</returns>
        public static int FindEntityCount<T>(this DbContext context, Expression<Func<T, bool>> condition = null) where T : class, new()
        {
            System.Data.Entity.Infrastructure.DbQuery<T> query = context.Set<T>();
            
            if (condition != null)
            {
                query = (System.Data.Entity.Infrastructure.DbQuery<T>)query.Where(condition);
            }
            return query.Count();
        }

        #endregion

        #region EF Tran Function

        /// <summary>
        /// Inserts the model with transaction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="obj">The obj.</param>
        /// <returns>Int32.</returns>
        public static Int32 InsertModelWithTransaction<T>(this DbContext context, T obj) where T : class
        {
            return context.ActionOperation(obj, OpterotionState.Added);
        }

        /// <summary>
        /// Updates the model with transaction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="obj">The obj.</param>
        /// <param name="updateColumns">The update columns.</param>
        /// <returns>Int32.</returns>
        public static Int32 UpdateModelWithTransaction<T>(this DbContext context, T obj, String updateColumns) where T : class
        {
            return context.ActionOperation(obj, OpterotionState.Modified, updateColumns);
        }

        /// <summary>
        /// Deletes the model with transaction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="obj">The obj.</param>
        /// <returns>Int32.</returns>
        public static Int32 DeleteModelWithTransaction<T>(this DbContext context, T obj) where T : class
        {
            return context.ActionOperation(obj, OpterotionState.Deleted);
        }

        ///// <summary>
        ///// Deletes the model with transaction.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="context">The context.</param>
        ///// <param name="obj">The obj.</param>
        ///// <returns>Int32.</returns>
        //public static Int32 DeleteModelWithTransaction<T>(this DbContext context, IEnumerable<T> obj) where T : class
        //{
        //    return context.DeleteModelList(obj);
        //}


        /// <summary>
        /// Actions the operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="obj">The obj.</param>
        /// <param name="pType">The pType.</param>
        /// <param name="updateColumns">The update columns.</param>
        /// <param name="action">The action.</param>
        /// <returns>Int32.</returns>
        internal static Int32 ActionOperation<T>(this DbContext context, T obj, OpterotionState pType, String updateColumns = null, Action<DbContext, T> action = null) where T : class
        {
            Int32 rValue = 0;
            var cnn = context.Database.Connection;

            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }

            System.Data.Common.DbTransaction transaction = cnn.BeginTransaction();

            try
            {
                switch (pType)
                {
                    case OpterotionState.Added:
                        rValue = context.InsertModel(obj, action);
                        break;
                    case OpterotionState.Modified:
                        rValue = context.UpdateModel(obj, updateColumns, action);
                        break;
                    case OpterotionState.Deleted:
                        //rValue = context.DeleteModel(obj, action);
                        break;
                }
                transaction.Commit();
            }
            catch (Exception f)
            {
                transaction.Rollback();
                throw f;
            }
            finally
            {
                cnn.Close();
            }
            return rValue;
        }

        /// <summary>
        /// Updates the model.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="updateColumns">Name of the update property.</param>
        /// <param name="action">The action.</param>
        /// <returns>Int32.</returns>
        public static Int32 UpdateModel<TEntity>(this DbContext context, TEntity entity, String updateColumns = null, Action<DbContext, TEntity> action = null) where TEntity : class
        {
            lock (ObjLock)
            {
                UpdateModelWithNotSave(context, entity, updateColumns);
                if (action != null)
                {
                    action(context, entity);
                }
                return context.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the model with not save.
        /// </summary>
        /// <typeparam name="TEntity">The type of the T entity.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="updateColumns">Name of the update property.</param>
        /// <param name="action">The action.</param>
        /// <returns>Int32.</returns>
        internal static void UpdateModelWithNotSave<TEntity>(this DbContext context, TEntity entity, String updateColumns = null, Action<DbContext, TEntity> action = null) where TEntity : class
        {
            lock (ObjLock)
            {
                var set = context.Set<TEntity>();
                set.Attach(entity);
                foreach (var updateItem in updateColumns.Split(SplitChart))
                {
                    context.Entry<TEntity>(entity).Property(updateItem).IsModified = true;
                }
                if (action != null)
                {
                    action(context, entity);
                }
            }
        }

        /// <summary>
        /// Executes the EF with transaction.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="efPrams">The ef prams.</param>
        /// <returns>List{``1}.</returns>
        /// <exception cref="System.ArgumentNullException">efParams is null</exception>
        public static List<TResult> ExecuteEFWithTransaction<T1, TResult>(this DbContext context, params EfTranPrams<T1, TResult>[] efPrams)
        {
            List<TResult> listObj = new List<TResult>();
            var cnn = context.Database.Connection;

            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }

            DbTransaction transaction = cnn.BeginTransaction();

            try
            {
                if (efPrams == null)
                {
                    throw new ArgumentNullException("efPrams is null");
                }

                Array
                    .ForEach<EfTranPrams<T1, TResult>>
                    (efPrams, t => listObj.Add(t.TuplePrams.Item2(context, t.TuplePrams.Item1)));
                transaction.Commit();
            }
            catch (Exception f)
            {
                transaction.Rollback();
                throw f;
            }
            finally
            {
                if (context.Database.Connection.State == System.Data.ConnectionState.Open)
                {
                    context.Database.Connection.Close();
                }
            }
            return listObj;
        }

        /// <summary>
        /// Executes the EF with transaction.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="efPrams">The ef prams.</param>
        /// <returns>List{``2}.</returns>
        /// <exception cref="System.ArgumentNullException">efParams is null</exception>
        public static List<TResult> ExecuteEFWithTransaction<T1, T2, TResult>(this DbContext context, params EfTranPrams<T1, T2, TResult>[] efPrams)
        {
            List<TResult> listObj = new List<TResult>();
            var cnn = context.Database.Connection;

            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }

            DbTransaction transaction = cnn.BeginTransaction();

            try
            {
                if (efPrams == null)
                {
                    throw new ArgumentNullException("efPrams is null");
                }

                Array.ForEach<EfTranPrams<T1, T2, TResult>>(efPrams, t => listObj.Add(t.TuplePrams.Item3(context, t.TuplePrams.Item1, t.TuplePrams.Item2)));
                transaction.Commit();
            }
            catch (Exception f)
            {
                transaction.Rollback();
                throw f;
            }
            finally
            {
                if (context.Database.Connection.State == System.Data.ConnectionState.Open)
                {
                    context.Database.Connection.Close();
                }
            }
            return listObj;
        }

        /// <summary>
        /// Executes the EF with transaction.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <typeparam name="T3">The type of the 3.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="efPrams">The ef prams.</param>
        /// <returns>List{``3}.</returns>
        /// <exception cref="System.ArgumentNullException">efParams is null</exception>
        public static List<TResult> ExecuteEFWithTransaction<T1, T2, T3, TResult>(this DbContext context, params EfTranPrams<T1, T2, T3, TResult>[] efPrams)
        {
            List<TResult> listObj = new List<TResult>();
            var cnn = context.Database.Connection;

            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }

            DbTransaction transaction = cnn.BeginTransaction();

            try
            {
                if (efPrams == null)
                {
                    throw new ArgumentNullException("efPrams is null");
                }

                Array.ForEach<EfTranPrams<T1, T2, T3, TResult>>(efPrams, t => listObj.Add(t.TuplePrams.Item4(context, t.TuplePrams.Item1, t.TuplePrams.Item2, t.TuplePrams.Item3)));
                transaction.Commit();
            }
            catch (Exception f)
            {
                transaction.Rollback();
                throw f;
            }
            finally
            {
                if (context.Database.Connection.State == System.Data.ConnectionState.Open)
                {
                    context.Database.Connection.Close();
                }
            }
            return listObj;
        }

        /// <summary>
        /// Executes the EF with transaction.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="action">The action.</param>
        public static void ExecuteEFWithTransaction(this DbContext context, Action<DbContext> action)
        {
            var cnn = context.Database.Connection;
            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }

            DbTransaction transaction = cnn.BeginTransaction();

            try
            {
                action(context);
                transaction.Commit();
            }
            catch (Exception f)
            {
                transaction.Rollback();
                throw f;
            }
            finally
            {
                if (context.Database.Connection.State == System.Data.ConnectionState.Open)
                {
                    context.Database.Connection.Close();
                }
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Gets the SQL connection.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>System.String.</returns>
        public static string GetConnectionString(this DbContext context)
        {
            return context.Database.Connection.ConnectionString;
        }

        /// <summary>
        /// Detache the Entity
        /// </summary>
        /// <typeparam name="TEntity">The type of the t entity.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="express">The express.</param>
        public static int DetacheEntity<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> express) where TEntity : class
        {
            var modelList = (context.Set<TEntity>()).Where(express).ToList();
            if (modelList.Count > 0)
            {
                for (int i = 0; i < modelList.Count; i++)
                {
                    var templateModel = modelList[i];
                    context.Entry(templateModel).State = System.Data.Entity.EntityState.Deleted;
                }
                return context.SaveChanges();
            }
            return 0;
        }
        #endregion

    }
}