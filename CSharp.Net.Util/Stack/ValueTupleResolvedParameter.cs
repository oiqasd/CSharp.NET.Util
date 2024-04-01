using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CSharp.Net.Util.Stack
{
    /// <summary>
    /// from Ben.Demystifier
    /// </summary>
    public class ValueTupleResolvedParameter : ResolvedParameter
    {
        public IList<string> TupleNames { get; }

        public ValueTupleResolvedParameter(Type resolvedType, IList<string> tupleNames)
            : base(resolvedType)
        {
            TupleNames = tupleNames;
        }

        protected override void AppendTypeName(StringBuilder sb)
        {
            if ((object)base.ResolvedType != null)
            {
                if (base.ResolvedType.IsValueTuple())
                {
                    AppendValueTupleParameterName(sb, base.ResolvedType);
                    return;
                }

                sb.Append(TypeNameHelper.GetTypeNameForGenericType(base.ResolvedType));
                sb.Append("<");
                AppendValueTupleParameterName(sb, base.ResolvedType.GetGenericArguments()[0]);
                sb.Append(">");
            }
        }

        private void AppendValueTupleParameterName(StringBuilder sb, Type parameterType)
        {
            sb.Append("(");
            Type[] genericArguments = parameterType.GetGenericArguments();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.AppendTypeDisplayName(genericArguments[i], fullName: false, includeGenericParameterNames: true);
                if (i < TupleNames.Count)
                {
                    string text = TupleNames[i];
                    if (text != null)
                    {
                        sb.Append(" ");
                        sb.Append(text);
                    }
                }
            }

            sb.Append(")");
        }
    }

    internal static class ReflectionHelper
    {
        private static PropertyInfo tranformerNamesLazyPropertyInfo;
        public static bool IsValueTuple(this Type type)
        {
            if (type.Namespace == "System")
            {
                return type.Name.Contains("ValueTuple`");
            }

            return false;
        }

        public static bool IsTupleElementNameAttribue(this Attribute attribute)
        {
            Type type = attribute.GetType();
            if (type.Namespace == "System.Runtime.CompilerServices")
            {
                return type.Name == "TupleElementNamesAttribute";
            }

            return false;
        }
        public static IList<string> GetTransformerNames(this Attribute attribute)
        {
            return GetTransformNamesPropertyInfo(attribute.GetType())?.GetValue(attribute) as IList<string>;
        }

        private static PropertyInfo GetTransformNamesPropertyInfo(Type attributeType)
        {
            Type attributeType2 = attributeType;
            return LazyInitializer.EnsureInitialized(ref tranformerNamesLazyPropertyInfo, () => attributeType2.GetProperty("TransformNames", BindingFlags.Instance | BindingFlags.Public));
        }
    }
}
