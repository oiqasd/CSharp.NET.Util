using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharp.Net.Cache
{
    public interface ICache
    {

        /// <summary>
        /// ����Key����ֵ����ȡ������
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object this[string key] { get; set; }

        /// <summary>
        /// ��ȡ������������ĸ���
        /// </summary>
        int Count { get; }

        /// <summary>
        /// ���浥��key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheSeconds"></param>
        /// <param name="isSliding"></param>
        bool StringSet(string key, string value, int cacheSeconds = 0, bool isSliding = true);

        ///// <summary>
        ///// ����һ������
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <param name="obj"></param>
        ///// <param name="cacheSeconds"></param>
        ///// <param name="isSliding"></param>
        ///// <returns></returns>
        //bool StringSet<T>(string key, T obj, int cacheSeconds = 0, bool isSliding = true) where T : new();

        /// <summary>
        /// ����һ������
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="cacheSeconds">Ĭ��0��������</param>
        /// <returns></returns>
        bool StringSet(string key, object obj, int cacheSeconds = 0);
        /// <summary>
        /// ��ȡһ��key�Ķ���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T StringGet<T>(string key);

        /// <summary>
        /// ��ȡkeyֵ
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string StringGet(string key);

        /// <summary>
        /// ɾ������key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>�Ƿ�ɾ���ɹ�</returns>
        bool KeyDelete(string key);
        /// <summary>
        /// ��ȡ�����key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        T GetOrSet<T>(string key, Func<Task<T>> func, TimeSpan? expiry = null);// where T : new();
        /// <summary>
        /// ��ȡ�򴴽�һ��key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="func">��������ʹ�ø�ֵ����</param>
        /// <param name="seconds">���Թ���ʱ�� Ĭ��0�벻����</param>
        /// <returns></returns>
        T GetOrSet<T>(string key, Func<T> func, int seconds = 0);
        /// <summary>
        /// ɾ����<paramref name="pattern"/>��ͷ��key
        /// </summary>
        /// <param name="pattern"></param>
        Task KeyDeleteStartWith(string pattern);
        /// <summary>
        /// ��ѯ<paramref name="pattern"/>��ͷ��keys
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="removePrefix">Ĭ���Ƴ�ʵ��ǰ׺</param>
        /// <returns></returns>
        Task<string[]> QueryStartWith(string pattern, bool removePrefix = true);
        /// <summary>
        /// �ж�key�Ƿ�洢
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns></returns>
        bool KeyExists(string key);

        /// <summary>
        /// ��ջ���
        /// </summary>
        void FlushDb();

        /// <summary>
        /// ����ֵ��ȡkey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        List<string> GetKeys<T>(T value);

        /// <summary>
        /// ��Ӽ���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        bool SetAdd<T>(string key, T value, TimeSpan? timeSpan = null);
        /// <summary>
        /// ��Ӽ���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        bool SetAdd<T>(string key, T[] value, TimeSpan? timeSpan = null);
        /// <summary>
        /// �Ƴ����϶���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        long SetRemove<T>(string key, params T[] value);
        /// <summary>
        /// ��ȡ�������ж���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> SetMembers<T>(string key);

        /// <summary>
        /// ������ؼ���һ������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T SetRandomMember<T>(string key);
        /// <summary>
        /// ������ؼ��϶������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> SetRandomMembers<T>(string key, long count = 1);
        /// <summary>
        /// �Ƿ���ڶ���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetContains<T>(string key, T value);

    }
}

