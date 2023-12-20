﻿using System;
using System.Text;

namespace CSharp.Net.Util.Stack
{
    /// <summary>
    /// from Ben.Demystifier
    /// </summary>
    public class ResolvedParameter
    {

        public string? Name { get; set; }

        public Type ResolvedType { get; set; }

        public string? Prefix { get; set; }

        public bool IsDynamicType { get; set; }

        public ResolvedParameter(Type resolvedType)
        {
            ResolvedType = resolvedType;
        }

        public override string ToString()
        {
            return Append(new StringBuilder()).ToString();
        }

        public StringBuilder Append(StringBuilder sb)
        {
            if (ResolvedType.Assembly.ManifestModule.Name == "FSharp.Core.dll" && ResolvedType.Name == "Unit")
            {
                return sb;
            }

            if (!string.IsNullOrEmpty(Prefix))
            {
                sb.Append(Prefix).Append(" ");
            }

            if (IsDynamicType)
            {
                sb.Append("dynamic");
            }
            else if (ResolvedType != null)
            {
                AppendTypeName(sb);
            }
            else
            {
                sb.Append("?");
            }

            if (!string.IsNullOrEmpty(Name))
            {
                sb.Append(" ").Append(Name);
            }

            return sb;
        }

        protected virtual void AppendTypeName(StringBuilder sb)
        {
            sb.AppendTypeDisplayName(ResolvedType, fullName: false, includeGenericParameterNames: true);
        }
    }
}
