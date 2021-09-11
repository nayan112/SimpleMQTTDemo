using System;
using Common;

namespace Client2
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new MQTTService();
            service.GetMQTTClient("2");
            Console.ReadLine();
        }
    }
}
