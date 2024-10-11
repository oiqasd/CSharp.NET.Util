using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 枚举帮助类
    /// </summary>
    public sealed class EnumHelper
    {
        private static readonly Dictionary<string, FieldInfo> PropertyDictionary = new Dictionary<string, FieldInfo>();

        public static TEnum ParseEnumWithDefault<TEnum>(string value, bool ignoreCase, TEnum defaultEnum) where TEnum : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultEnum;
            }
            if (Enum.TryParse<TEnum>(value, ignoreCase, out TEnum result))
            {
                return result;
            }
            else
            {
                return defaultEnum;
            }
        }

        /// <summary>
        /// 由说明来解析枚举
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="description">说明</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns>枚举</returns>
        public static TEnum ParseByDescription<TEnum>(string description, bool ignoreCase)
        {
            Type enumType = typeof(TEnum);
            if (enumType.IsEnum != true)
            {
                throw new ArgumentException(string.Format("", enumType.FullName));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }

            List<EnumItem> list = EnumHelper.GetEnumItems(enumType);

            foreach (EnumItem item in list)
            {
                if (string.Compare(item.Description, description, ignoreCase) == 0)
                {
                    return (TEnum)Enum.Parse(enumType, item.Name);
                }
            }

            throw new ArgumentException(string.Format("", description, enumType.FullName));
        }

        /// <summary>
        /// 从名称或枚举值的字符串表示解析枚举值
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="nameOrValue">名称或值字符串表示</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns>枚举</returns>
        public static TEnum ParseByNameOrValue<TEnum>(string nameOrValue, bool ignoreCase = true)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), nameOrValue, ignoreCase);
        }

        #region 通过字符串获取枚举成员实例
        /// <summary>
        /// 通过字符串获取枚举成员实例
        /// </summary>
        /// <typeparam name="TEnum">枚举名,比如Enum1</typeparam>
        /// <param name="member">枚举成员的常量名或常量值,
        /// 范例:Enum1枚举有两个成员A=0,B=1,则传入"A"或"0"获取 Enum1.A 枚举类型</param>
        public static TEnum GetInstance<TEnum>(string member)
        {
            return ConvertHelper.ConvertTo<TEnum>(Enum.Parse(typeof(TEnum), member, true));
        }
        #endregion

        #region 获取枚举成员的名称
        /// <summary>
        /// 获取枚举成员的名称
        /// </summary>
        /// <typeparam name="T">枚举名,比如Enum1</typeparam>
        /// <param name="member">枚举成员实例或成员值,
        /// 范例:Enum1枚举有两个成员A=0,B=1,则传入Enum1.A或0,获取成员名称"A"</param>
        public static string GetName<T>(object member)
        {
            //转成基础类型的成员值
            Type underlyingType = GetUnderlyingType(typeof(T));
            object memberValue = ConvertHelper.ConvertTo(underlyingType, member);

            //获取枚举成员的名称
            return Enum.GetName(typeof(T), memberValue);
        }
        #endregion

        #region 获取枚举成员的值
        /// <summary>
        /// 获取枚举成员的值
        /// </summary>
        /// <typeparam name="T">枚举名,比如Enum1</typeparam>
        /// <param name="memberName">枚举成员的常量名,
        /// 范例:Enum1枚举有两个成员A=0,B=1,则传入"A"获取0</param>
        public static object GetValue<T>(string memberName)
        {
            //获取基础类型
            Type underlyingType = GetUnderlyingType(typeof(T));

            //获取枚举实例
            T instance = GetInstance<T>(memberName);
            var da = Convert.ChangeType(instance, underlyingType);
            return da;
        }

        #endregion

        #region 获取枚举的基础类型
        /// <summary>
        /// 获取枚举的基础类型
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        public static Type GetUnderlyingType(Type enumType)
        {
            //获取基础类型
            return Enum.GetUnderlyingType(enumType);
        }
        #endregion

        #region 检测枚举是否包含指定成员
        /// <summary>
        /// 检测枚举是否包含指定成员
        /// </summary>
        /// <typeparam name="T">枚举名,比如Enum1</typeparam>
        /// <param name="member">枚举成员名或成员值</param>
        public static bool IsDefined<T>(string member)
        {
            return Enum.IsDefined(typeof(T), member);
        }
        #endregion

        #region 获取Enum集合

        /// <summary>
        /// 获得Enum类型的绑定列表
        /// </summary>
        /// <param name="enumType">枚举的类型，例如：typeof(Sex)</param>
        /// <param name="preferDescriptionForKey">是否优先取Description描述作为key</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetEnumList(Type enumType, bool preferDescriptionForKey = false)
        {
            if (enumType.IsEnum != true)
            {    //不是枚举的要报错
                throw new InvalidOperationException();
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            //获得特性Description的类型信息
            Type typeDescription = typeof(DescriptionAttribute);

            //获得枚举的字段信息（因为枚举的值实际上是一个static的字段的值）
            System.Reflection.FieldInfo[] fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);

            //检索所有字段
            try
            {
                foreach (FieldInfo field in fields)
                {
                    //过滤掉一个不是枚举值的，记录的是枚举的源类型
                    if (field.FieldType.IsEnum == true)
                    {
                        if (Attribute.GetCustomAttribute(field, typeof(ObsoleteAttribute), false) == null) continue;
                        string key;
                        // 通过字段的名字得到枚举的值
                        object value = enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null);

                        //获得这个字段的所有自定义特性，这里只查找Description特性
                        object[] arr = field.GetCustomAttributes(typeDescription, true);
                        if (preferDescriptionForKey && arr.Length > 0)
                        {
                            //因为Description这个自定义特性是不允许重复的，所以我们只取第一个就可以了！
                            DescriptionAttribute aa = (DescriptionAttribute)arr[0];
                            //获得特性的描述值，也就是‘男’‘女’等中文描述
                            key = aa.Description;
                            if (data.ContainsKey(key))
                                key = field.Name;
                        }
                        else
                        {
                            //如果没有特性描述就显示英文的字段名
                            key = field.Name;
                        }
                        data.Add(key, value);
                    }
                }
            }
            catch
            {
            }

            return data;
        }

        /// <summary>
        /// 扩展方法，获得枚举的{Value,Description,EDescription}字典集
        /// 获取enum值对应多个的Description（需根据需求调整EnumItem）
        /// </summary>
        /// <param name="enumType">枚举本身</param> 
        /// <param name="IgnoreValues">忽略值集合，默认为空</param>
        /// <returns>枚举的Description</returns>
        public static List<EnumItem> GetDescriptionToList(Type enumType, int[] IgnoreValues = null)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("传入的参数必须是枚举类型！", nameof(enumType));
            }

            var list = new List<EnumItem>();
            var enumValues = Enum.GetValues(enumType);
            foreach (Enum enumValue in enumValues)
            {
                //对应的int值
                var valueNum = enumValue.GetHashCode();
                var name = Enum.GetName(enumType, enumValue);
                FieldInfo field = GetFieldInfo(enumType, name);
                //中文描述
                DescriptionAttribute attributeN = field.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
                //英文描述
                EDescriptionAttribute attributeE = field.GetCustomAttribute(typeof(EDescriptionAttribute)) as EDescriptionAttribute;
                //中描述文本
                var valueN = attributeN == null ? string.Empty : attributeN.Description;
                //英中描述文本
                var valueE = attributeE == null ? string.Empty : attributeE.EDescription;
                if (IgnoreValues != null && IgnoreValues.Any())
                {
                    if (IgnoreValues.Contains(valueNum))
                    {
                        continue;
                    }
                }

                list.Add(new EnumItem(valueNum, valueN, valueE));
            }
            return list;
        }

        /// <summary>
        /// 获取枚举条目信息列表
        /// key,value,description
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <returns>条目列表</returns>
        public static List<EnumItem> GetEnumItems(Type enumType)
        {
            if (enumType.IsEnum != true)
            {
                string message = string.Format("{0}", enumType.FullName);
                throw new ArgumentException(message);
            }
            var list = new List<EnumItem>();
            Type descAttributeType = typeof(DescriptionAttribute);
            FieldInfo[] fields = enumType.GetFields();

            foreach (FieldInfo field in fields)
            {
                int value;
                string description;

                if (!field.FieldType.IsEnum)
                    continue;

                value = (int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null);

                object[] descArr = field.GetCustomAttributes(descAttributeType, true);
                if (descArr.Length > 0)
                {
                    var desAttribute = (DescriptionAttribute)descArr[0];
                    description = desAttribute.Description;
                }
                else
                {
                    description = field.Name;
                }

                list.Add(new EnumItem(value, field.Name, description));
            }
            return list;
        }

        /// <summary>
        /// 扩展方法，获得枚举的{Value,Description}字典集
        /// </summary>
        /// <param name="enumType">枚举本身</param>        
        /// <returns>枚举的Description</returns>
        public static IDictionary<Enum, string> ToDescriptionDictionary(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("传入的参数必须是枚举类型！", nameof(enumType));
            }
            var dic = new Dictionary<Enum, string>();

            var enumValues = Enum.GetValues(enumType);
            foreach (Enum enumValue in enumValues)
            {
                var key = enumValue;
                var name = Enum.GetName(enumType, enumValue);
                FieldInfo field = GetFieldInfo(enumType, name);

                DescriptionAttribute attribute =
                    Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                var value = attribute == null ? name : attribute.Description;

                dic.Add(key, value);
            }
            return dic;
        }

        #region 获取枚举成员名称和成员值的键值对集合
        /// <summary>
        /// 获取枚举成员名称和成员值的键值对集合
        /// </summary>
        /// <typeparam name="T">枚举名,比如Enum1</typeparam>
        public static Hashtable GetKeyValue<T>()
        {
            //创建哈希表
            Hashtable ht = new Hashtable();

            //获取枚举所有成员名称
            string[] memberNames = GetNames<T>();

            //遍历枚举成员
            foreach (string memberName in memberNames)
            {
                ht.Add(memberName, GetValue<T>(memberName));
            }

            //返回哈希表
            return ht;
        }
        #endregion

        #region 获取枚举所有成员名称
        /// <summary>
        /// 获取枚举所有成员名称
        /// </summary>
        /// <typeparam name="T">枚举名,比如Enum1</typeparam>
        public static string[] GetNames<T>()
        {
            return Enum.GetNames(typeof(T));
        }
        #endregion

        #region 获取枚举所有成员值
        /// <summary>
        /// 获取枚举所有成员值
        /// </summary>
        /// <typeparam name="T">枚举名,比如Enum1</typeparam>
        public static Array GetValues<T>()
        {
            return Enum.GetValues(typeof(T));
        }
        #endregion

        #endregion

        /// <summary>
        /// 获取枚举值的详细文本
        /// </summary>
        /// <param name="e">object</param>
        /// <param name="enumType">枚举类型</param>
        /// <returns></returns>
        public static string GetEnumDescription(object e, Type enumType)
        {
            //获取字段信息
            System.Reflection.FieldInfo[] ms = enumType.GetFields();
            int value;
            Type t = enumType;
            foreach (System.Reflection.FieldInfo f in ms)
            {
                //判断名称是否相等
                try
                {
                    value = (int)enumType.InvokeMember(f.Name, BindingFlags.GetField, null, null, null);
                    if (value != (int)e)
                        continue;
                }
                catch
                {
                    continue;
                }
                //反射出自定义属性
                foreach (Attribute attr in f.GetCustomAttributes(true))
                {
                    //类型转换找到一个Description，用Description作为成员名称
                    System.ComponentModel.DescriptionAttribute dscript = attr as System.ComponentModel.DescriptionAttribute;
                    if (dscript != null)
                        return dscript.Description;
                }
            }

            //如果没有检测到合适的注释，则用默认名称
            return e.ToString();
        }

        /// <summary>
        /// 获取枚举值的详细文本
        /// </summary>
        /// <param name="e">object</param>
        /// <returns>枚举值的文本</returns>
        public static string GetEnumDescription(object e)
        {
            //获取字段信息
            System.Reflection.FieldInfo[] ms = e.GetType().GetFields();

            Type t = e.GetType();
            foreach (System.Reflection.FieldInfo f in ms)
            {
                //判断名称是否相等
                if (f.Name != e.ToString()) continue;

                //反射出自定义属性
                foreach (Attribute attr in f.GetCustomAttributes(true))
                {
                    //类型转换找到一个Description，用Description作为成员名称
                    System.ComponentModel.DescriptionAttribute dscript = attr as System.ComponentModel.DescriptionAttribute;
                    if (dscript != null)
                        return dscript.Description;
                }
            }

            //如果没有检测到合适的注释，则用默认名称
            return e.ToString();
        }


        /// <summary>
        /// 获得枚举的{Description,Value}字典集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Dictionary<string, T> GetDescription2ValueDictionary<T>(Func<T, bool> predicate = null)
        {
            var enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("传入的泛型参数必须是枚举类型！", nameof(enumType));
            }
            var dic = new Dictionary<string, T>();

            var enumValues = Enum.GetValues(enumType).Cast<T>();
            if (predicate != null)
            {
                enumValues = enumValues.Where(predicate);
            }
            foreach (T enumValue in enumValues)
            {
                var name = Enum.GetName(enumType, enumValue);
                FieldInfo field = GetFieldInfo(enumType, name);

                DescriptionAttribute attribute = field.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
                var key = attribute == null ? name : attribute.Description;
                dic.Add(key, enumValue);
            }
            return dic;
        }

#if NET
        /// <summary>
        /// 获取枚举名
        /// <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/>
        /// <param name="en"></param>
        /// <returns></returns>
        /// </summary>
        public static string GetEnumDisplayName(System.Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((System.ComponentModel.DataAnnotations.DisplayAttribute)attrs[0]).Name;
            }
            return en.ToString();
        }
#endif
        /// <summary>
        /// 获得枚举的{EDescription,Value}字典集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Dictionary<string, T> GetEDescription2ValueDictionary<T>(Func<T, bool> predicate = null)
        {
            var enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("传入的泛型参数必须是枚举类型！", nameof(enumType));
            }
            var dic = new Dictionary<string, T>();

            var enumValues = Enum.GetValues(enumType).Cast<T>();
            if (predicate != null)
            {
                enumValues = enumValues.Where(predicate);
            }
            foreach (T enumValue in enumValues)
            {
                var name = Enum.GetName(enumType, enumValue);
                FieldInfo field = GetFieldInfo(enumType, name);

                EDescriptionAttribute attribute = field.GetCustomAttribute(typeof(EDescriptionAttribute)) as EDescriptionAttribute;
                var key = attribute == null ? name : attribute.EDescription;
                dic.Add(key, enumValue);
            }
            return dic;
        }


        #region PrivateMethod

        /// <summary>
        /// 从缓存中获取FieldInfo,取不到去反射
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private static FieldInfo GetFieldInfo(Type modelType, string fieldName)
        {
            FieldInfo fieldInfo = null;
            var propertyFullName = $"{modelType.FullName}.{fieldName}";

            lock (PropertyDictionary)
            {
                if (PropertyDictionary.TryGetValue(propertyFullName, out fieldInfo))
                    return fieldInfo;
            }

            fieldInfo = modelType.GetField(fieldName);

            if (fieldInfo == null) return null;

            lock (PropertyDictionary)
            {
                PropertyDictionary.Add(propertyFullName, fieldInfo);
            }
            return fieldInfo;
        }

        #endregion
    }
}