using System;
using System.Text;
using MQTTnet;
using MQTTnet.Server;
using Serilog;

namespace MQTTServer
{
    class Program
    {
        public static int MessageCounter { get; private set; }

        static void Main(string[] args)
        {
            // Create the options for our MQTT Broker
            MqttServerOptionsBuilder options = new MqttServerOptionsBuilder()
                                                 // set endpoint to localhost
                                                 .WithDefaultEndpoint()
                                                 // port used will be 707
                                                 .WithDefaultEndpointPort(707)
                                                 // handler for new connections
                                                 .WithConnectionValidator(OnNewConnection)
                                                 // handler for new messages
                                                 .WithApplicationMessageInterceptor(OnNewMessage);
            // creates a new mqtt server     
            IMqttServer mqttServer = new MqttFactory().CreateMqttServer();

            // start the server with options  
            mqttServer.StartAsync(options.Build()).GetAwaiter().GetResult();

            Console.ReadLine();
        }
        public static void OnNewConnection(MqttConnectionValidatorContext context)
        {
            Console.WriteLine(string.Format("New connection: ClientId = {0}, Endpoint = {1}", context.ClientId, context.Endpoint));
        }

        public static void OnNewMessage(MqttApplicationMessageInterceptorContext context)
        {
            var payload = context.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(context.ApplicationMessage?.Payload);

            MessageCounter++;

            Console.WriteLine(string.Format(
                "MessageId: {0} - TimeStamp: {1} -- Message: ClientId = {2}, Topic = {3}, Payload = {4}, QoS = {5}, Retain-Flag = {6}",
                MessageCounter,
                DateTime.Now,
                context.ClientId,
                context.ApplicationMessage?.Topic,
                payload,
                context.ApplicationMessage?.QualityOfServiceLevel,
                context.ApplicationMessage?.Retain));
        }
    }
}
