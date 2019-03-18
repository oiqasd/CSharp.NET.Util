using System;
using System.Collections.Generic;

namespace CSharp.Net.Standard.RedisManager
{
    public interface IRedisService
    { 
        void Set(string key, object obj, TimeSpan expiresIn);
        void Set(string key, object obj, DateTime expiresAt);
        void PrependItemToList(string key, string obj);
        void PrependRangeToList(string key, List<string> values);
        string PopItemFromList(string listId);
        long GetListCount(string listId);
        void Remove(string key);
        void RemoveAll(IEnumerable<string> key);
        T Get<T>(string key);
    }
}
