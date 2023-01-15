using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 动态获取对象属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyValueHelper<T>
    {
        delegate object MemberGetDelegate(T obj);

        private static ConcurrentDictionary<string, MemberGetDelegate> _memberGetDelegate = new ConcurrentDictionary<string, MemberGetDelegate>();

        public PropertyValueHelper(T obj)
        {
            Target = obj;
            type = typeof(T);
        }
        T Target { get; }
        Type type { get; }

        public object Get(string name)
        {
            MemberGetDelegate memberGet = _memberGetDelegate.GetOrAdd(name, BuildDelegate);

            if (memberGet == null)
                return null;

            return memberGet(Target);
        }
        private MemberGetDelegate BuildDelegate(string name)
        {
            PropertyInfo property = type.GetProperty(name);
            if (property == null)
                return null;
            return (MemberGetDelegate)Delegate.CreateDelegate(typeof(MemberGetDelegate), property.GetGetMethod());
        }
    }

    public class PropertyValueLambda<T>
    {
        delegate object MemberGetDelegate(T obj);

        private static ConcurrentDictionary<string, Func<T, object>> _propertyDelegate = new ConcurrentDictionary<string, Func<T, object>>();

        public PropertyValueLambda(T obj)
        {
            Target = obj;
            type = typeof(T);
            parameterExpression = Expression.Parameter(type);
        }
        Type type { get; }
        T Target { get; }
        ParameterExpression parameterExpression { get; }
        public object Get(string name)
        {
            var func = _propertyDelegate.GetOrAdd(name, BuildDelegate);
            if (func == null)
                return null;
            return func.Invoke(Target);
        }
        private Func<T, object> BuildDelegate(string name)
        {
            //Type type = typeof(T);
            //ParameterExpression parameterExpression = Expression.Parameter(type);
            PropertyInfo property = type.GetProperty(name);
            if (property == null)
                return null;
            Expression expProperty = Expression.Property(parameterExpression, property.Name);//取参数的属性m.Name
            var propertyDelegateExpression = Expression.Lambda(expProperty, parameterExpression);//变成表达式 m => m.Name
            return (Func<T, object>)propertyDelegateExpression.Compile();//编译成委托
        }
    }
}
