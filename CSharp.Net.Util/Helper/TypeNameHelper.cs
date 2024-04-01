using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    internal static class TypeNameHelper
    {
        private struct DisplayNameOptions
        {
            public bool FullName { get; }

            public bool IncludeGenericParameterNames { get; }

            public DisplayNameOptions(bool fullName, bool includeGenericParameterNames)
            {
                FullName = fullName;
                IncludeGenericParameterNames = includeGenericParameterNames;
            }
        }

        public static readonly Dictionary<Type, string> BuiltInTypeNames = new Dictionary<Type, string>
        {
            {
                typeof(void),
                "void"
            },
            {
                typeof(bool),
                "bool"
            },
            {
                typeof(byte),
                "byte"
            },
            {
                typeof(char),
                "char"
            },
            {
                typeof(decimal),
                "decimal"
            },
            {
                typeof(double),
                "double"
            },
            {
                typeof(float),
                "float"
            },
            {
                typeof(int),
                "int"
            },
            {
                typeof(long),
                "long"
            },
            {
                typeof(object),
                "object"
            },
            {
                typeof(sbyte),
                "sbyte"
            },
            {
                typeof(short),
                "short"
            },
            {
                typeof(string),
                "string"
            },
            {
                typeof(uint),
                "uint"
            },
            {
                typeof(ulong),
                "ulong"
            },
            {
                typeof(ushort),
                "ushort"
            }
        };

        public static readonly Dictionary<string, string> FSharpTypeNames = new Dictionary<string, string>
        {
            { "Unit", "void" },
            { "FSharpOption", "Option" },
            { "FSharpAsync", "Async" },
            { "FSharpOption`1", "Option" },
            { "FSharpAsync`1", "Async" }
        };

        public static string GetTypeDisplayName(Type type, bool fullName = true, bool includeGenericParameterNames = false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            ProcessType(stringBuilder, type, new DisplayNameOptions(fullName, includeGenericParameterNames));
            return stringBuilder.ToString();
        }

        public static StringBuilder AppendTypeDisplayName(this StringBuilder builder, Type type, bool fullName = true, bool includeGenericParameterNames = false)
        {
            ProcessType(builder, type, new DisplayNameOptions(fullName, includeGenericParameterNames));
            return builder;
        }

        public static string GetTypeNameForGenericType(Type type)
        {
            if (!type.IsGenericType)
            {
                throw new ArgumentException("The given type should be generic", "type");
            }

            int num = type.Name.IndexOf('`');
            if (num < 0)
            {
                return type.Name;
            }

            return type.Name.Substring(0, num);
        }

        private static void ProcessType(StringBuilder builder, Type type, DisplayNameOptions options)
        {
            string value;
            if (type.IsGenericType)
            {
                Type underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType != null)
                {
                    ProcessType(builder, underlyingType, options);
                    builder.Append('?');
                }
                else
                {
                    Type[] genericArguments = type.GetGenericArguments();
                    ProcessGenericType(builder, type, genericArguments, genericArguments.Length, options);
                }
            }
            else if (type.IsArray)
            {
                ProcessArrayType(builder, type, options);
            }
            else if (BuiltInTypeNames.TryGetValue(type, out value))
            {
                builder.Append(value);
            }
            else if (type.Namespace == "System")
            {
                builder.Append(type.Name);
            }
            else if (type.Assembly.ManifestModule.Name == "FSharp.Core.dll" && FSharpTypeNames.TryGetValue(type.Name, out value))
            {
                builder.Append(value);
            }
            else if (type.IsGenericParameter)
            {
                if (options.IncludeGenericParameterNames)
                {
                    builder.Append(type.Name);
                }
            }
            else
            {
                builder.Append(options.FullName ? (type.FullName ?? type.Name) : type.Name);
            }
        }

        private static void ProcessArrayType(StringBuilder builder, Type type, DisplayNameOptions options)
        {
            Type type2 = type;
            while (type2.IsArray)
            {
                Type elementType = type2.GetElementType();
                if ((object)elementType != null)
                {
                    type2 = elementType;
                }
            }

            ProcessType(builder, type2, options);
            while (type.IsArray)
            {
                builder.Append('[');
                builder.Append(',', type.GetArrayRank() - 1);
                builder.Append(']');
                Type elementType2 = type.GetElementType();
                if ((object)elementType2 != null)
                {
                    type = elementType2;
                    continue;
                }

                break;
            }
        }

        private static void ProcessGenericType(StringBuilder builder, Type type, Type[] genericArguments, int length, DisplayNameOptions options)
        {
            int num = 0;
            if (type.IsNested && (object)type.DeclaringType != null)
            {
                num = type.DeclaringType.GetGenericArguments().Length;
            }

            if (options.FullName)
            {
                if (type.IsNested && (object)type.DeclaringType != null)
                {
                    ProcessGenericType(builder, type.DeclaringType, genericArguments, num, options);
                    builder.Append('+');
                }
                else if (!string.IsNullOrEmpty(type.Namespace))
                {
                    builder.Append(type.Namespace);
                    builder.Append('.');
                }
            }

            int num2 = type.Name.IndexOf('`');
            if (num2 <= 0)
            {
                builder.Append(type.Name);
                return;
            }

            if (type.Assembly.ManifestModule.Name == "FSharp.Core.dll" && FSharpTypeNames.TryGetValue(type.Name, out var value))
            {
                builder.Append(value);
            }
            else
            {
                builder.Append(type.Name, 0, num2);
            }

            builder.Append('<');
            for (int i = num; i < length; i++)
            {
                ProcessType(builder, genericArguments[i], options);
                if (i + 1 != length)
                {
                    builder.Append(',');
                    if (options.IncludeGenericParameterNames || !genericArguments[i + 1].IsGenericParameter)
                    {
                        builder.Append(' ');
                    }
                }
            }

            builder.Append('>');
        }
    }
}
