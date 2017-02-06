// ***********************************************************************
// Assembly         : HySoft.Core.Framework
// Author           : 李小军
// Created          : 08-04-2014
//
// Last Modified By : 李小军
// Last Modified On : 08-04-2014
// ***********************************************************************
// <copyright file="ExtensionEF.cs" company="赛思网络科技">
//     Copyright (c) 赛思网络科技. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Linq;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.Metadata.Edm;

namespace HySoft.Core.Framework
{
    /// <summary>
    /// Class ExtensionEf
    /// </summary>
    public static class ExtensionEf
    {

        /// <summary>
        /// Attaches the existed entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the t entity.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>
        internal static void AttachExistedEntity<TEntity>(this ObjectContext context, TEntity entity)
        where TEntity : class
        {
            var entitySetName = context.GetEntityName<TEntity>();
            ObjectStateEntry stateEntry = null;
            if (entity is EntityObject && (entity as EntityObject).EntityKey != null)
            {
                if (!context.ObjectStateManager.TryGetObjectStateEntry((entity as EntityObject).EntityKey, out stateEntry))
                {
                    context.Attach(entity as EntityObject);
                }
            }
            else
            {
                context.AttachTo(entitySetName, entity);
            }
        }

        ///// <summary>
        ///// Gets the name of the entity.
        ///// </summary>
        ///// <typeparam name="TEntity"></typeparam>
        ///// <returns></returns>
        ///// <remarks></remarks>
        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the t entity.</typeparam>
        /// <param name="context">The context.</param>
        /// <returns>System.String.</returns>
        public static string GetEntityName<TEntity>(this ObjectContext context)
        {
            var className = typeof(TEntity).Name;
            var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
            var entitySetName = (from meta in container.BaseEntitySets
                                    where meta.ElementType.Name == className
                                    select meta.Name).First();
            return entitySetName;
        }
    }
}