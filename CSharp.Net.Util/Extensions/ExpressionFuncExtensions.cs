using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// ExpressionFunc扩展
    /// </summary>
    public static class ExpressionFuncExtensions
    {
        /// <summary>
        /// 以特定的条件运行组合两个Expression表达式
        /// </summary>
        /// <typeparam name="T">表达式的主实体类型</typeparam>
        /// <param name="first">第一个Expression表达式</param>
        /// <param name="second">要组合的Expression表达式</param>
        /// <param name="merge">组合条件运算方式</param>
        /// <returns>组合后的表达式</returns>
        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second,
            Func<Expression, Expression, Expression> merge)
        {
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);
            var secondBody = ParameterReplace.Replace(second.Body, map);
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        /// <summary>
        /// 以 Expression.AndAlso 组合两个Expression表达式
        /// </summary>
        /// <typeparam name="T">表达式的主实体类型</typeparam>
        /// <param name="first">第一个Expression表达式</param>
        /// <param name="second">要组合的Expression表达式</param>
        /// <returns>组合后的表达式</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        /// <summary>
        ///     以 Expression.OrElse 组合两个Expression表达式
        /// </summary>
        /// <typeparam name="T">表达式的主实体类型</typeparam>
        /// <param name="first">第一个Expression表达式</param>
        /// <param name="second">要组合的Expression表达式</param>
        /// <returns>组合后的表达式</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        /// <summary>
        /// Where+If（条件过滤）And拼接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="isWhere"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereIf<T>(this Expression<Func<T, bool>> source, bool isWhere, Expression<Func<T, bool>> predicate)
        {
            if (isWhere)
            {
                source = source.Compose(predicate, Expression.AndAlso);
            }
            return source;
        }

        //private class ParameterRebinder : ExpressionVisitor
        //{
        //    private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        //    private ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        //    {
        //        _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        //    }

        //    public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map,                Expression exp)
        //    {
        //        return new ParameterRebinder(map).Visit(exp);
        //    }

        //    protected override Expression VisitParameter(ParameterExpression node)
        //    {
        //        ParameterExpression replacement;
        //        if (_map.TryGetValue(node, out replacement))
        //            node = replacement;
        //        return base.VisitParameter(node);
        //    }
        //}
              
    }
}
