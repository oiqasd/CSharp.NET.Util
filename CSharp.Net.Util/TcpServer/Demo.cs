using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util.TcpServerDemo
{
    public class TcpServerDemo
    {
        public async Task Test(int port = 6000)
        {
            TcpServer socketServer = new TcpServer(port);
            socketServer.OnReceive += (sender, e) =>
            {
                Console.WriteLine($"Received message: {e.Message}");
                e.Response = "I received message:" + e.Message;
            };

            socketServer.Start();
            Console.WriteLine("Press Enter to stop the server...");
            Console.ReadKey();
            socketServer.Stop();
        }
    }

    public class TcpClientDemo
    {
        public async Task Test(string server = "127.0.0.1", int port = 6000)
        {
            using (TcpHelper client = new TcpHelper(server, port))
            {
                client.StartClient();

                while (true)
                {
                    Console.Write("请输入消息：");
                    string message = Console.ReadLine();
                    if (message == "exit") break;

                    await client.SendAsync(message);
                    Console.WriteLine($"Send: {message}");

                    // 接收服务器的响应  
                    string responseData = await client.ReadAsync();
                    Console.WriteLine($"Received: {responseData}");
                }
            }
        }
    }
}
