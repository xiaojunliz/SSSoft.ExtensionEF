// ***********************************************************************
// Assembly         : HySoft.Core.Framework
// Author           : 李小军
// Created          : 08-04-2014
//
// Last Modified By : 李小军
// Last Modified On : 08-04-2014
// ***********************************************************************
// <copyright file="DynamicTypeBuilder.cs" company="赛思网络科技">
//     Copyright (c) 赛思网络科技. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace HySoft.Core.Framework
{

    //Thanks to Ethan J. Brown:
    //  http://stackoverflow.com/questions/606104/how-to-create-linq-expression-tree-with-anonymous-type-in-it/723018#723018

    /// <summary>
    /// Class DynamicTypeBuilder
    /// </summary>
    public static class DynamicTypeBuilder
    {
        /// <summary>
        /// The assembly name
        /// </summary>
        private static AssemblyName assemblyName = new AssemblyName() { Name = "DynamicLinqTypes" };
        /// <summary>
        /// The module builder
        /// </summary>
        private static ModuleBuilder moduleBuilder = null;
        /// <summary>
        /// The built types
        /// </summary>
        private static Dictionary<string, Tuple<string, Type>> builtTypes = new Dictionary<string, Tuple<string, Type>>();

        /// <summary>
        /// Initializes static members of the <see cref="DynamicTypeBuilder" /> class.
        /// </summary>
        static DynamicTypeBuilder()
        {
            moduleBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(assemblyName.Name);
        }

        /// <summary>
        /// Gets the type key.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>System.String.</returns>
        private static string GetTypeKey(Dictionary<string, Type> fields)
        {
            string key = string.Empty;
            foreach (var field in fields.OrderBy(v => v.Key).ThenBy(v => v.Value.Name))
                key += field.Key + ";" + field.Value.Name + ";";
            return key;
        }

        /// <summary>
        /// Gets the type of the dynamic.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="basetype">The basetype.</param>
        /// <param name="interfaces">The interfaces.</param>
        /// <returns>Type.</returns>
        /// <exception cref="System.ArgumentNullException">fields</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">fields;fields must have at least 1 field definition</exception>
        public static Type GetDynamicType(Dictionary<string, Type> fields, Type basetype, Type[] interfaces)
        {
            if (null == fields)
                throw new ArgumentNullException("fields");
            if (0 == fields.Count)
                throw new ArgumentOutOfRangeException("fields", "fields must have at least 1 field definition");

            try
            {
                Monitor.Enter(builtTypes);
                string typeKey = GetTypeKey(fields);

                if (builtTypes.ContainsKey(typeKey))
                    return builtTypes[typeKey].Item2;

                string typeName = "DynamicLinqType" + builtTypes.Count.ToString();
                TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable, null, Type.EmptyTypes);

                foreach (var field in fields)
                    typeBuilder.DefineField(field.Key, field.Value, FieldAttributes.Public);

                builtTypes[typeKey] = new Tuple<string,Type>(typeName, typeBuilder.CreateType());

                return builtTypes[typeKey].Item2;
            }
            catch
            {
                throw;
            }
            finally
            {
                Monitor.Exit(builtTypes);
            }

        }

    }
}
