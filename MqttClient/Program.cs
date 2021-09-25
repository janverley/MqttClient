using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;

namespace MqttClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Test MQTT Client");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false);

            IConfiguration config = builder.Build();

            var defaults = config.GetSection(nameof(Defaults)).Get<Defaults>();

            var tcpServer = defaults.TcpServer;

            Console.WriteLine($"Enter TcPServer: (default = {tcpServer})");
            var overwriteTcpServer = Console.ReadLine();
            if (!String.IsNullOrEmpty(overwriteTcpServer))
            {
                tcpServer = overwriteTcpServer;
            }

            var factory    = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId("Client1")
                .WithTcpServer(tcpServer)
                .Build();

            Console.WriteLine($"Connecting to {tcpServer}...");

            var result = await mqttClient.ConnectAsync(options, CancellationToken.None);

            if (result.ResultCode != MqttClientConnectResultCode.Success)
            {
                Console.WriteLine(result.ResponseInformation);
                Console.WriteLine(result.ReasonString);
            }
            else
            {
                var topic          = defaults.Topic;
                var defaultPayload = DateTime.UtcNow.ToString("U");
                var payload        = defaultPayload;

                while (true)
                {
                    Console.WriteLine($"Enter topic: (default = {topic})");
                    var overwriteTopic = Console.ReadLine();
                    if (!String.IsNullOrEmpty(overwriteTopic))
                    {
                        topic = overwriteTopic;
                    }

                    Console.WriteLine($"Enter payload: (default = {payload})");
                    var overwritePayload = Console.ReadLine();
                    if (!String.IsNullOrEmpty(overwritePayload))
                    {
                        payload = overwritePayload;
                    }

                    var message = new MqttApplicationMessageBuilder()
                        .WithPayload(payload)
                        .WithTopic(topic)
                        .Build();
                    var x = await mqttClient.PublishAsync(message, CancellationToken.None);
                    if (x.ReasonCode != MqttClientPublishReasonCode.Success)
                    {
                        Console.WriteLine("Failed to publish!");
                        Console.WriteLine($"{x.ReasonCode}/n{x.ReasonString}");
                    }

                    Console.WriteLine("Another? (type a letter to stop)");
                    var another = Console.ReadLine();
                    if (!String.IsNullOrEmpty(another))
                    {
                        break;
                    }
                }
            }

            Console.WriteLine("Test MQTT Client - Done!");
        }
    }

    internal class Defaults
    {
        public string TcpServer { get; set; }
        public string Topic { get; set; }
    }
}