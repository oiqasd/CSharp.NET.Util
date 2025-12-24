using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace CSharp.Net.Util.LiteSockets
{
    public class LiteSocketsDemo
    {
        /// <summary>
        /// 服务端
        /// </summary>
        /// <returns></returns>
        public async Task testHub()
        {
            var hub = new SocketHub(9000);
            hub.OnClientConnected += (id, ep) => Console.WriteLine($"[+] {id} {ep}");
            hub.OnClientDisconnected += (id, ep) => Console.WriteLine($"[-] {id} {ep}");
            await hub.StartAsync();

            Console.ReadLine();
            await hub.DisposeAsync();
        }


        /// <summary>
        /// 客户端
        /// </summary>
        /// <returns></returns>
        public async Task testClient()
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync("127.0.0.1", 9000);
            var client = new SocketClient(new SocketWire(socket));

            client.OnMessage<object>("pong", async obj =>
            {
                Console.WriteLine("RECV: " + JsonSerializer.Serialize(obj));
                await Task.CompletedTask;
            });

           //  Parallel.ForAsync(0, 100, async (i,x) => {
             //   Console.WriteLine($"{i}");
                await client.SendAsync("ping", new { hello = "world" });
                await client.SendAsync("ping", new { hello = "world222" });
           // });
          
            //Console.ReadLine();
            await client.DisposeAsync();
        }
    }
}
