using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using Newtonsoft.Json;

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
                while (true)
                {
                  {
                      var messageCo2 = new MqttApplicationMessageBuilder()
                          .WithTopic("co2")
                          .WithPayload(JsonConvert.SerializeObject(new { Variable = "CO2", Value = 500, Unit = "ppm" }))
                          .Build();
                      var result1 = await mqttClient.PublishAsync(messageCo2, CancellationToken.None);
                      if (result1.ReasonCode != MqttClientPublishReasonCode.Success)
                      {
                          Console.WriteLine("Failed to publish!");
                          Console.WriteLine($"{result1.ReasonCode}/n{result1.ReasonString}");
                      }
                      var messageTemp = new MqttApplicationMessageBuilder()
                          .WithTopic("temp")
                          .WithPayload(JsonConvert.SerializeObject(new { Variable = "temperature", Value = 15, Unit = "C" }))
                          .Build();
                      var result2 = await mqttClient.PublishAsync(messageTemp, CancellationToken.None);
                      if (result2.ReasonCode != MqttClientPublishReasonCode.Success)
                      {
                          Console.WriteLine("Failed to publish!");
                          Console.WriteLine($"{result2.ReasonCode}/n{result2.ReasonString}");
                      }
                      var messageHum = new MqttApplicationMessageBuilder()
                          .WithTopic("hum")
                          .WithPayload(JsonConvert.SerializeObject(new { Variable = "humidity", Value = 42, Unit = "%R.H." }))
                          .Build();
                      var result3 = await mqttClient.PublishAsync(messageHum, CancellationToken.None);
                      if (result3.ReasonCode != MqttClientPublishReasonCode.Success)
                      {
                          Console.WriteLine("Failed to publish!");
                          Console.WriteLine($"{result3.ReasonCode}/n{result3.ReasonString}");
                      }
                  }
                  Console.WriteLine("Published!");
                  Console.ReadKey();
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