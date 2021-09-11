using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Server;
using System;
using System.Text;

namespace Common
{
    public class MQTTService
    {
        public string ClientId = "";
        public int MessageCounter { get; private set; }
        public IManagedMqttClient GetMQTTClient(string clientId)
        {
            ClientId = clientId;
            Console.WriteLine($"=============================================== CLIENT {clientId} STARTING ==================================================");
            // Creates a new client
            var builder = new MqttClientOptionsBuilder()
                                                    .WithClientId($"Client{clientId}")
                                                    .WithTcpServer("localhost", 707);

            // Create client options objects
            var options = new ManagedMqttClientOptionsBuilder()
                                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(60))
                                    .WithClientOptions(builder.Build())
                                    .Build();

            // Creates the client object
            var _mqttClient = new MqttFactory().CreateManagedMqttClient();

            // Set up handlers
            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            _mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(OnConnectingFailed);
            _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic($"client{clientId}/topic/json").Build());
            // Starts a connection with the Broker
            _mqttClient.StartAsync(options).GetAwaiter().GetResult();
            _mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
            });
            return _mqttClient;
        }

        public IMqttServer GetMQTTBroker()
        {
            Console.WriteLine("==================================== MQTT BROKER STARTING ==========================================");
            // Create the options for our MQTT Broker
            var options = new MqttServerOptionsBuilder()
                                                 // set endpoint to localhost
                                                 .WithDefaultEndpoint()
                                                 // port used will be 707
                                                 .WithDefaultEndpointPort(707)
                                                 // handler for new connections
                                                 .WithConnectionValidator(OnNewConnection)
                                                 // handler for new messages
                                                 .WithApplicationMessageInterceptor(OnNewMessage);
            // creates a new mqtt server     
            var mqttServer = new MqttFactory().CreateMqttServer();

            // start the server with options  
            mqttServer.StartAsync(options.Build()).GetAwaiter().GetResult();
            return mqttServer;
        }
        public void OnConnected(MqttClientConnectedEventArgs obj)
        {
            Console.WriteLine($"Client{ClientId} cuccessfully connected.");
        }

        public void OnConnectingFailed(ManagedProcessFailedEventArgs obj)
        {
            Console.WriteLine($"Client{ClientId} couldn't connect to broker.");
        }

        public void OnDisconnected(MqttClientDisconnectedEventArgs obj)
        {
            Console.WriteLine($"Client{ClientId} successfully disconnected.");
        }
        public void OnNewConnection(MqttConnectionValidatorContext context)
        {
            Console.WriteLine(string.Format("New connection: ClientId = {0}, Endpoint = {1}", context.ClientId, context.Endpoint));
        }

        public void OnNewMessage(MqttApplicationMessageInterceptorContext context)
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
