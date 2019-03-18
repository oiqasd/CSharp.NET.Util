using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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

    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int, bool>> predicate)
    {
        throw new NotSupportedException("未实现");
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }
        if (predicate == null)
        {
            throw new ArgumentNullException("predicate");
        }
        Expression[] arguments = new Expression[] { source.Expression, Expression.Quote(predicate) };

        //return source.Provider.CreateQuery<TSource>(Expression.Call(null, GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int, bool>>, IQueryable<TSource>>(new Func<IQueryable<TSource>, Expression<Func<TSource, int, bool>>, IQueryable<TSource>>(Queryable.Where<TSource>), source, predicate), arguments));
    }
}
