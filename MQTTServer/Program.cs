using System;
using System.Text.Json;
using System.Threading.Tasks;
using Common;
using MQTTnet.Server;

namespace MQTTServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new MQTTService();
            var mqttServer = service.GetMQTTBroker();
            do
            {
                Task.Delay(1000).GetAwaiter().GetResult();
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("Select client Id to publish to: 1. Client 1, 2. Cleint 2");
                var val = Console.ReadLine();
                if (val != "1" && val != "2")
                    Console.WriteLine("Invalid Input!");
                else
                {
                    string json = JsonSerializer.Serialize(new { message = "Hi :)", sent = DateTimeOffset.UtcNow });
                    mqttServer.PublishAsync($"client{val}/topic/json", json);
                    
                }
            } while (true);
            Console.ReadLine();
        }
    }
}
