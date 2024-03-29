﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

/// <summary>
/// PredicateBuilder
/// </summary>
public static class QueryableExtension
{
    /// <summary>
    /// 执行排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="sortModels"></param>
    /// <returns></returns>
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<SortModel> sortModels)
    {
        if (sortModels == null) return source;

        //SortModel[] models = new SortModel[] { new SortModel { ColumnName = nameof(T.id), SortAsc = false } };
        var expression = source.Expression;
        bool orderby = false;

        foreach (var item in sortModels)
        {
            var paramter = Expression.Parameter(typeof(T), "u");
            var selector = Expression.PropertyOrField(paramter, item.ColumnName);
            var method = item.SortAsc ? !orderby ? "OrderBy" : "ThenBy" : !orderby ? "OrderByDescending" : "ThenByDescending";
            expression = Expression.Call(typeof(Queryable), method, new Type[] { source.ElementType, selector.Type }, expression, Expression.Quote(Expression.Lambda(selector, paramter)));
            orderby = true;
        }

        return orderby ? source.Provider.CreateQuery<T>(expression) : source;
    }

    #region Expression Joiner
    /// <summary>
    /// 创建一个值恒为 <c>true</c> 的表达式。
    /// </summary>
    /// <typeparam name="T">表达式方法类型</typeparam>
    /// <returns>一个值恒为 <c>true</c> 的表达式。</returns>
    public static Expression<Func<T, bool>> True<T>() { return p => true; }

    /// <summary>
    /// 创建一个值恒为 <c>false</c> 的表达式。
    /// </summary>
    /// <typeparam name="T">表达式方法类型</typeparam>
    /// <returns>一个值恒为 <c>false</c> 的表达式。</returns>
    public static Expression<Func<T, bool>> False<T>() { return f => false; }

    /// <summary>
    /// 使用 Expression.OrElse 的方式拼接两个 System.Linq.Expression。
    /// </summary>
    /// <typeparam name="T">表达式方法类型</typeparam>
    /// <param name="left">左边的 System.Linq.Expression 。</param>
    /// <param name="right">右边的 System.Linq.Expression。</param>
    /// <returns>拼接完成的 System.Linq.Expression。</returns>
    public static Expression<T> Or<T>(this Expression<T> left, Expression<T> right)
    {
        return MakeBinary(left, right, Expression.OrElse);
    }

    /// <summary>
    /// 使用 Expression.AndAlso 的方式拼接两个 System.Linq.Expression。
    /// </summary>
    /// <typeparam name="T">表达式方法类型</typeparam>
    /// <param name="left">左边的 System.Linq.Expression 。</param>
    /// <param name="right">右边的 System.Linq.Expression。</param>
    /// <returns>拼接完成的 System.Linq.Expression。</returns>
    public static Expression<T> And<T>(this Expression<T> left, Expression<T> right)
    {
        return MakeBinary(left, right, Expression.AndAlso);
    }

    /// <summary>
    /// 条件判断拼接
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="condition"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }

    /// <summary>
    /// 使用自定义的方式拼接两个 System.Linq.Expression。
    /// </summary>
    /// <typeparam name="T">表达式中的元素类型</typeparam>
    /// <param name="left">左边的 System.Linq.Expression 。</param>
    /// <param name="right">右边的 System.Linq.Expression。</param>
    /// <param name="func"></param>
    /// <returns>拼接完成的 System.Linq.Expression。</returns>
    public static Expression<T> MakeBinary<T>(this Expression<T> left, Expression<T> right, Func<Expression, Expression, Expression> func)
    {
        return MakeBinary((LambdaExpression)left, right, func) as Expression<T>;
    }

    /// <summary>
    /// 拼接两个 <paramref name="System.Linq.Expression"/> ，两个 <paramref name="System.Linq.Expression"/> 的参数必须完全相同。
    /// </summary>
    /// <param name="left">左边的 <paramref name="System.Linq.Expression"/></param>
    /// <param name="right">右边的 <paramref name="System.Linq.Expression"/></param>
    /// <param name="func">表达式拼接的具体逻辑</param>
    /// <returns>拼接完成的 <paramref name="System.Linq.Expression"/></returns>
    public static LambdaExpression MakeBinary(this LambdaExpression left, LambdaExpression right, Func<Expression, Expression, Expression> func)
    {
        var data = Combinate(right.Parameters, left.Parameters).ToArray();
        right = ParameterReplace.Replace(right, data) as LambdaExpression;
        return Expression.Lambda(func(left.Body, right.Body), left.Parameters.ToArray());
    }
    #endregion

    #region Private Methods
    private static IEnumerable<KeyValuePair<T, T>> Combinate<T>(IEnumerable<T> left, IEnumerable<T> right)
    {
        var a = left.GetEnumerator();
        var b = right.GetEnumerator();
        while (a.MoveNext() && b.MoveNext())
            yield return new KeyValuePair<T, T>(a.Current, b.Current);
    }
    #endregion
}

internal sealed class ParameterReplace : ExpressionVisitor
{
    public static Expression Replace(Expression e, IEnumerable<KeyValuePair<ParameterExpression, ParameterExpression>> paramList)
    {
        var item = new ParameterReplace(paramList);
        return item.Visit(e);
    }

    private Dictionary<ParameterExpression, ParameterExpression> parameters = null;

    public ParameterReplace(IEnumerable<KeyValuePair<ParameterExpression, ParameterExpression>> paramList)
    {
        parameters = paramList.ToDictionary(p => p.Key, p => p.Value, new ParameterEquality());
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
        ParameterExpression result;
        if (parameters.TryGetValue(p, out result))
            return result;
        else
            return base.VisitParameter(p);
    }

    private class ParameterEquality : IEqualityComparer<ParameterExpression>
    {
        public bool Equals(ParameterExpression x, ParameterExpression y)
        {
            if (x == null || y == null)
                return false;

            return x.Type == y.Type;
        }

        public int GetHashCode(ParameterExpression obj)
        {
            if (obj == null)
                return 0;

            return obj.Type.GetHashCode();
        }
    }
}
