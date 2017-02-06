// ***********************************************************************
// Assembly         : HySoft.Core.Framework
// Author           : 李小军
// Created          : 08-05-2014
//
// Last Modified By : 李小军
// Last Modified On : 08-05-2014
// ***********************************************************************
// <copyright file="ExpressionHelp.cs" company="赛思网络科技">
//     Copyright (c) 赛思网络科技. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace HySoft.Core.Framework
{
    /// <summary>
    /// Class ExpressionHelp
    /// </summary>
    public class ExpressionHelp
    {
        /// <summary>
        /// Gets the f expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ecmList">The ecm list.</param>
        /// <returns>Expression{Func{``0Boolean}}.</returns>
        public Expression<Func<T, Boolean>> GetFExpression<T>(IList<ExpConditionModel> ecmList)
        {
            if (ecmList == null)
            {
                return t => true;
            }
            else
            {
                ParameterExpression param = Expression.Parameter(typeof(T), "param");
                return GetFExpression<T>(param, ecmList);
            }
        }
        /// <summary>
        /// 获取条件表达式
        /// </summary>
        /// <typeparam name="T">表达式对象</typeparam>
        /// <param name="param1">The param1.</param>
        /// <param name="ecmList">The ecm list.</param>
        /// <returns>Expression{Func{``0Boolean}}.</returns>
        public Expression<Func<T, Boolean>> GetFExpression<T>(ParameterExpression param1, IList<ExpConditionModel> ecmList)
        {
            //定义子表达式
            Expression exp = Expression.Constant(true);
            Expression exp1 = null;
            Expression exp2 = null;
            //ParameterExpression param = Expression.Parameter(typeof(T), "c");
            if (ecmList != null && ecmList.Count > 0)
            {
                bool isincludeor = false;
                for (int i = 0; i < ecmList.Count; i++)
                {
                    ExpConditionModel ecm = ecmList[i];
                    try
                    {
                        Expression filter = SetoutExpression<T>(ecmList, param1, i);
                        exp = Expression.And(exp, filter);
                        if (ecm.Relation == OperationRelation.or)
                        {
                            exp1 = exp;
                            exp = Expression.Constant(true);
                            isincludeor = true;
                        }
                        if (i == ecmList.Count - 1 && isincludeor)
                        {
                            exp2 = Expression.Or(exp2, exp); ;
                        }
                        if (exp1 != null)
                        {
                            if (exp2 == null)
                            {
                                exp2 = exp1;

                            }
                            else
                            {
                                exp2 = Expression.Or(exp2, exp1);
                            }
                            exp1 = null;
                        }
                        if (i == ecmList.Count - 1 && exp2 == null)
                        {
                            exp2 = exp;
                        }

                    }
                    catch { }
                }
            }
            else
            {
                exp2 = Expression.Constant(true);
            }
            return (Expression<Func<T, Boolean>>)Expression.Lambda<Func<T, Boolean>>(exp2, new ParameterExpression[] { param1 });
        }

        /// <summary>
        /// Setouts the expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="EcmList">The ecm list.</param>
        /// <param name="param">The param.</param>
        /// <param name="i">The i.</param>
        /// <returns>Expression.</returns>
        private Expression SetoutExpression<T>(IList<ExpConditionModel> EcmList, ParameterExpression param, int i)
        {
            Expression filter = null;
            ExpConditionModel Ecm = EcmList[i];
            Expression left = null;

            if (typeof(T).GetProperties().Where(t => t.Name == Ecm.Column).First() != null)
            {
                left = Expression.Property(param, Ecm.Column);
            }

            Expression right = Expression.Constant(Ecm.OperationValue);
            //将值转为数字型
            switch (Ecm.ColumnType)
            {
                case ColumnType.str:
                    break;
                case ColumnType.number:
                    right = Expression.Constant(double.Parse(Ecm.OperationValue));
                    break;
                case ColumnType.date:
                    right = Expression.Constant(DateTime.Parse(Ecm.OperationValue));
                    if (left.Type.Name.IndexOf("Nullable") == 0)
                        left = Expression.Property(Expression.Property(param, typeof(T).GetProperty(Ecm.Column)), typeof(DateTime?).GetProperty("Value"));
                    break;
                case ColumnType.int16:
                    right = Expression.Constant(short.Parse(Ecm.OperationValue));
                    break;
                case ColumnType.int16s:
                    left = Expression.Constant(Ecm.OperationValue.ToInt16S(','), typeof(short[]));
                    right = Expression.PropertyOrField(param, Ecm.Column);
                    break;
                case ColumnType.int32:
                    right = Expression.Constant(int.Parse(Ecm.OperationValue));
                    break;
                case ColumnType.int32s:
                    left = Expression.Constant(Ecm.OperationValue.ToInt32S(','), typeof(int[]));
                    right = Expression.PropertyOrField(param, Ecm.Column);
                    break;
                default:
                    break;
            }

            //判断是否为字符型特殊条件形式
            if (Ecm.OperationType == OperationType.Contains || Ecm.OperationType == OperationType.StartsWith || Ecm.OperationType == OperationType.EndsWith)
            {
                Type[] containsTypes = new Type[1];
                MethodInfo mdInfo = null;
                MethodInfo[] mdInfos = null;
                switch (Ecm.ColumnType)
                {
                    case ColumnType.int16s:
                        containsTypes[0] = typeof(short);

                        mdInfos = typeof(Enumerable).GetMethods();
                        mdInfo = mdInfos.First(t => t.Name.Equals(((OperationType)Ecm.OperationType).ToString()) && t.IsGenericMethod && t.GetGenericArguments().Length == 1).MakeGenericMethod(typeof(short));
                        filter = Expression.Call(mdInfo, left, right);
                        break;
                    case ColumnType.int32s:
                        containsTypes[0] = typeof(int);

                        mdInfos = typeof(Enumerable).GetMethods();
                        mdInfo = mdInfos.First(t => t.Name.Equals(((OperationType)Ecm.OperationType).ToString()) && t.IsGenericMethod && t.GetGenericArguments().Length == 1).MakeGenericMethod(typeof(int));
                        filter = Expression.Call(mdInfo, left, right);
                        break;
                    default:
                        containsTypes[0] = typeof(string);
                        mdInfo = typeof(string).GetMethod(((OperationType)Ecm.OperationType).ToString(), containsTypes);
                        filter = Expression.Call(left, mdInfo, right);
                        break;
                }
            }
            else
            {
                switch (Ecm.OperationType)
                {
                    case OperationType.Equal: filter = Expression.Equal(left, right); break;
                    case OperationType.NotEqual: filter = Expression.NotEqual(left, right); break;
                    case OperationType.Greater: filter = Expression.GreaterThan(left, right); break;
                    case OperationType.Less: filter = Expression.LessThan(left, right); break;
                    case OperationType.GreatEqual: filter = Expression.GreaterThanOrEqual(left, right); break;
                    case OperationType.LessEqual: filter = Expression.LessThanOrEqual(left, right); break;
                    default: filter = Expression.Equal(left, right); break;
                }
            }
            return filter;
        }
    }

    /// <summary>
    /// Class ExpDynamicModel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExpDynamicModel<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpDynamicModel{T}" /> class.
        /// </summary>
        /// <param name="ecmList">The ecm list.</param>
        public ExpDynamicModel(List<ExpConditionModel> ecmList)
        {
            EcmList = ecmList;
        }

        /// <summary>
        /// Gets or sets the ecm list.
        /// </summary>
        /// <value>The ecm list.</value>
        public List<ExpConditionModel> EcmList { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>The entity.</value>
        public T Entity { get; set; }
    }
    /// <summary>
    /// 构建条件表达式的参数对象
    /// </summary>
    public class ExpConditionModel
    {
        //字段,列对象名
        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value>The T column.</value>
        public string Column { get; set; }

        /// <summary>
        /// 操作类型
        /// Gets or sets the type of the column.
        /// </summary>
        /// <value>The type of the column.</value>
        public ColumnType ColumnType { get; set; }

        //条件类型
        /// <summary>
        /// Gets or sets the type of the T.
        /// </summary>
        /// <value>The type of the T.</value>
        public OperationType OperationType { get; set; }

        //比较的值
        /// <summary>
        /// Gets or sets the T value.
        /// </summary>
        /// <value>The T value.</value>
        public string OperationValue { get; set; }

        //操作关系
        /// <summary>
        /// Gets or sets the relation.
        /// </summary>
        /// <value>The relation.</value>
        public OperationRelation Relation { get; set; }
    }
    /// <summary>
    /// Enum OperationRelation
    /// </summary>
    public enum OperationRelation
    {
        /// <summary>
        /// The none
        /// </summary>
        [Description("None")]
        none = 0,

        /// <summary>
        /// The and
        /// </summary>
        [Description("And")]
        and = 1,

        /// <summary>
        /// The or
        /// </summary>
        [Description("Or")]
        or = 2
    }
    /// <summary>
    /// 数据类型枚举
    /// </summary>
    public enum ColumnType
    {
        /// <summary>
        /// The STR
        /// </summary>
        [Description("String 类型")]
        str = 0,

        /// <summary>
        /// The number
        /// </summary>
        [Description("数字类型")]
        number = 1,

        /// <summary>
        /// The date
        /// </summary>
        [Description("时间类型")]
        date = 2,

        /// <summary>
        /// The int16
        /// </summary>
        [Description("Int16 类型")]
        int16 = 3,

        /// <summary>
        /// The int16s
        /// </summary>
        [Description("Int16 类型数组")]
        int16s = 4,

        /// <summary>
        /// The int32
        /// </summary>
        [Description("Int32 类型")]
        int32 = 5,

        /// <summary>
        /// The int32s
        /// </summary>
        [Description("Int32 类型数组")]
        int32s = 6,

        /// <summary>
        /// The int64
        /// </summary>
        [Description("Int64 类型")]
        int64 = 7,

        /// <summary>
        /// The dataTable
        /// </summary>
        [Description("DataTable 类型")]
        dataTable = 8
    }
    /// <summary>
    /// 操作类型枚举
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// 等于
        /// </summary>
        [Description("等于")]
        Equal = 0,

        /// <summary>
        /// 不等于
        /// </summary>
        [Description("不等于")]
        NotEqual = 1,

        /// <summary>
        /// 大于
        /// </summary>
        [Description("大于")]
        Greater = 2,

        /// <summary>
        /// 小于
        /// </summary>
        [Description("小于")]
        Less = 3,

        /// <summary>
        /// 大于等于
        /// </summary>
        [Description("大于等于")]
        GreatEqual = 4,

        /// <summary>
        /// 小于等于
        /// </summary>
        [Description("小于等于")]
        LessEqual = 5,

        /// <summary>
        /// 开始
        /// </summary>
        [Description("开始")]
        StartsWith = 6,

        /// <summary>
        /// 结束
        /// </summary>
        [Description("结束")]
        EndsWith = 7,

        /// <summary>
        /// 包含
        /// </summary>
        [Description("包含")]
        Contains = 8
    }
    /// <summary>
    /// Class ExtString.
    /// </summary>
    internal static class ExtString
    {
        /// <summary>
        /// 将指定的 object 的值转换为整型数组
        /// </summary>
        /// <param name="str">任意字符串</param>
        /// <param name="separator">The separator.</param>
        /// <returns>Int16[][].</returns>
        internal static Int16[] ToInt16S(this String str, char separator)
        {
            if (!String.IsNullOrWhiteSpace(str))
            {
                string[] temp = str.Split(separator);
                Int16[] stemp = new Int16[temp.Length];

                int count = 0;
                foreach (string item in temp)
                {
                    if (item.IsNumeric())
                    {
                        stemp[count] = item.ToInt16();
                        count++;
                    }
                    else
                    {
                        return null;
                    }
                }
                return stemp;
            }
            return null;
        }
        /// <summary>
        /// 将指定的 object 的值转换为整型数组
        /// </summary>
        /// <param name="str">任意字符串</param>
        /// <param name="separator">The separator.</param>
        /// <returns>Int32[][].</returns>
        internal static Int32[] ToInt32S(this String str, char separator)
        {
            if (!String.IsNullOrWhiteSpace(str))
            {
                string[] temp = str.Split(separator);
                Int32[] stemp = new Int32[temp.Length];

                int count = 0;
                foreach (string item in temp)
                {
                    if (item.IsNumeric())
                    {
                        stemp[count] = item.ToInt32();
                        count++;
                    }
                    else
                    {
                        return null;
                    }
                }
                return stemp;
            }
            return null;
        }
        /// <summary>
        /// 验证内容是否为整数
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns><c>true</c> if the specified STR is numeric; otherwise, <c>false</c>.</returns>
        internal static bool IsNumeric(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return Regex.IsMatch(str, @"^-?[0-9]*$", RegexOptions.IgnoreCase);
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// To the int16.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>Int16.</returns>
        internal static Int16 ToInt16(this Object s)
        {
            return ToInt16(Convert.ToString(s), 0);
        }
        /// <summary>
        /// To the int16.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>Int16.</returns>
        internal static Int16 ToInt16(this Int16? s)
        {
            return ToInt16(Convert.ToString(s), 0);
        }
        /// <summary>
        /// To the int16.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>Int16.</returns>
        internal static Int16 ToInt16(this String s)
        {
            return ToInt16(s, 0);
        }
        /// <summary>
        /// To the int16.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Int16.</returns>
        internal static Int16 ToInt16(this String s, Int16 defaultValue)
        {
            Int16 r = defaultValue;
            if (s != null)
            {
                Int16.TryParse(s, out r);
            }
            return r;
        }

        /// <summary>
        /// To the int32.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>Int32.</returns>
        internal static Int32 ToInt32(this Object s)
        {
            return ToInt32(Convert.ToString(s), 0);
        }

        /// <summary>
        /// To the int32.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>Int32.</returns>
        internal static Int32 ToInt32(this Int32? s)
        {
            return ToInt32(Convert.ToString(s), 0);
        }
        /// <summary>
        /// To the int32.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>Int32.</returns>
        internal static Int32 ToInt32(this String s)
        {
            return ToInt32(s, 0);
        }
        /// <summary>
        /// To the int32.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>Int32.</returns>
        internal static Int32 ToInt32(this Char s)
        {
            return ToInt32(Convert.ToString(s));
        }
        /// <summary>
        /// To the int32.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Int32.</returns>
        internal static Int32 ToInt32(this String s, Int32 defaultValue)
        {
            int r = defaultValue;
            if (s != null)
            {
                Int32.TryParse(s, out r);
            }
            return r;
        }
    }
}