using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSharp.Net.Util.Json.NET
{
    /// <summary>
    /// 序列化时NULL初始化处理
    /// <code>
    /// Startup .AddNewtonsoftJson(options =>options.SerializerSettings.ContractResolver = new NullToInitContractResolver()); 
    /// </code>
    /// </summary>
    public class NullToInitContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var data = type.GetProperties().Select(c =>
            {
                var pro = base.CreateProperty(c, memberSerialization);
                pro.ValueProvider = new NullToInitValueProvider(c);
                return pro;
            });

            return data.ToList();
        }
    }

    public class NullToInitValueProvider : IValueProvider
    {
        private readonly PropertyInfo _property;
        public NullToInitValueProvider(PropertyInfo property)
        {
            _property = property;
        }

        public object GetValue(object target)
        {
            var result = _property.GetValue(target);

            if (_property.PropertyType == typeof(string) && result == null)
                return string.Empty;

            if (result == null)
            {
                try
                {
                    var instance = Activator.CreateInstance(_property.PropertyType);
                    result = instance;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetExcetionMessage());
                }
            }
            return result;
        }

        public void SetValue(object target, object value)
        {
            _property.SetValue(target, value);
        }
    }
}
