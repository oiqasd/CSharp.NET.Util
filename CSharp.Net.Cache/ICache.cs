using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        /// <summary>
        /// ����һ������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="cacheSeconds"></param>
        /// <param name="isSliding"></param>
        /// <returns></returns>
        bool StringSet<T>(string key, T obj, int cacheSeconds = 0, bool isSliding = true) where T : new();

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
        /// ��ȡ�򴴽�һ��key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue">��������ʹ�ø�ֵ����</param>
        /// <param name="seconds">���Թ���ʱ�� Ĭ��30��</param>
        /// <returns></returns>
        string GetOrCreate(string key, string defaultValue, int seconds = 30);
        /// <summary>
        /// ɾ����<paramref name="pattern"/>��ͷ��key
        /// </summary>
        /// <param name="pattern"></param>
        void KeyDeleteStartWith(string pattern);
        /// <summary>
        /// ��ѯ<paramref name="pattern"/>��ͷ��keys
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="removePrefix">Ĭ���Ƴ�ʵ��ǰ׺</param>
        /// <returns></returns>
        string[] QueryStartWith(string pattern, bool removePrefix = true);
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

        #region Set ���򼯺�

        #region ͬ������

        /// <summary>
        /// ��Ӷ���
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        bool SetAdd<T>(string key, T value, TimeSpan? timeSpan = null);

        /// <summary>
        /// ��set�������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        bool SetAdd<T>(string key, T[] value, TimeSpan? timeSpan = null);
        /// <summary>
        /// ɾ��
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        long SetRemove<T>(string key, params T[] value);

        /// <summary>
        /// ��ȡȫ��
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> SetMembers<T>(string key);

        /// <summary>
        /// �����ȡһ��
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T SetRandomMember<T>(string key);

        /// <summary>
        /// �����ȡ���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> SetRandomMembers<T>(string key, long count = 1);

        /// <summary>
        /// �ж�key�������Ƿ����ָ��ֵ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetContains<T>(string key, T value);

        /// <summary>
        ///  ���ɾ��key�����е�һ��ֵ�������ظ�ֵ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param> 
        /// <returns></returns>
        T SetPop<T>(string key);

        /// <summary>
        /// ���ɾ��key�����е�count��ֵ��������count��ֵ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> SetPop<T>(string key, long count);

        /// <summary>
        /// ��ȡ�����е�����
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long SetLength(string key);
        /// <summary>
        /// ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> SetUnion<T>(params string[] key);
        /// <summary>
        /// ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> SetIntersect<T>(params string[] key);
        /// <summary>
        /// �
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> SetDifference<T>(params string[] key);
        #endregion ͬ������

        #region �첽����

        /// <summary>
        /// ���
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        Task<bool> SetAddAsync<T>(string key, T value, TimeSpan? timeSpan = null);
        /// <summary>
        /// ��set�������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        Task<bool> SetAddAsync<T>(string key, T[] value, TimeSpan? timeSpan = null);
        /// <summary>
        /// ɾ��
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        Task<long> SetRemoveAsync<T>(string key, params T[] value);

        /// <summary>
        /// ��ȡȫ��
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<List<T>> SetMembersAsync<T>(string key);

        /// <summary>
        /// �����ȡһ��
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> SetRandomMemberAsync<T>(string key);

        /// <summary>
        /// �����ȡ���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<T>> SetRandomMembersAsync<T>(string key, long count = 1);

        /// <summary>
        /// �ж�key�������Ƿ����ָ��ֵ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<bool> SetContainsAsync<T>(string key, T value);

        /// <summary>
        ///  ���ɾ��key�����е�һ��ֵ�������ظ�ֵ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param> 
        /// <returns></returns>
        Task<T> SetPopAsync<T>(string key);

        /// <summary>
        /// ���ɾ��key�����е�count��ֵ��������count��ֵ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<T>> SetPopAsync<T>(string key, long count);

        /// <summary>
        /// ��ȡ�����е�����
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<long> SetLengthAsync(string key);

        #endregion �첽����

        #endregion Set ���򼯺�


        #region ��������

        /// <summary>
        /// Redis��������  ����
        /// </summary>
        /// <param name="subChannel"></param>
        void Subscribe(string subChannel);

        /// <summary>
        /// �������� 
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="action"></param>
        void Subscribe(string subChannel, Action<string> action);
        Task SubscribeAsync(string subChannel, Func<string, Task> action);

        /// <summary>
        /// Redis��������  ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        long Publish<T>(string channel, T msg);

        /// <summary>
        /// Redis��������  ȡ������
        /// </summary>
        /// <param name="channel"></param>
        void Unsubscribe(string channel);

        /// <summary>
        /// [���ص���]Redis��������  ȡ��ȫ������
        /// </summary>
        void UnsubscribeAll();
        #endregion
    }
}

