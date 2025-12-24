using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    class TestSub
    {

        void Exe()
        {
            //创建发布者
            AppPublish publish = new AppPublish();
            //创建订阅者
            AppSubscriber subscriber = new AppSubscriber();
            //订阅事件
            subscriber.Subscribe(publish);
            publish.Publish("this is message");

        }
    }


    public delegate void EventHandler(string message);

    public class AppPublish
    {
        /// <summary>
        /// 声明事件
        /// </summary>
        public event EventHandler OnPublish;
        public void Publish(string message)
        {
            OnPublish?.Invoke(message);
            //if (OnPublish != null)  
            //OnPublish(message); 
        }
    }
    public class AppSubscriber
    {
        public void Subscribe(AppPublish publish)
        {
            //订阅事件
            publish.OnPublish += OnEventHandler;
        }

        public void OnEventHandler(string message)
        {
            Console.WriteLine($"Event message:{message}");
        }

        public void UnSubscribe(AppPublish publish)
        {
            //取消订阅
            publish.OnPublish -= OnEventHandler;
        }
    }
}
