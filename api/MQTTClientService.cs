using System.Text.Json;
using Backend.DeviceEventHandlers;
using Backend.infrastructure.dataModels;
using Backend.infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;

namespace Backend;

public class MQTTClientService(DeviceRepository deviceRepository)
{
    public async Task CommunicateWithBroker()
    {
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();
        var mqttClient2 = mqttFactory.CreateMqttClient();

        var mqttClientOptions1 = new MqttClientOptionsBuilder()
            .WithTcpServer("mqtt.flespi.io", 1883)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithCredentials("FlespiToken "+ Environment.GetEnvironmentVariable("Flespitoken"), "")
            .Build();

        await mqttClient.ConnectAsync(mqttClientOptions1, CancellationToken.None);

        var mqttSubForCheck = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic("devices/registration/check"))
            .Build();

        await mqttClient.SubscribeAsync(mqttSubForCheck, CancellationToken.None);
        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                var message = e.ApplicationMessage.ConvertPayloadToString();
                Console.WriteLine("Received message: " + message);
                var messageObject = JsonSerializer.Deserialize<DeviceWantsToCheckRegistrationDto>(message);
                var isRegistered = await deviceRepository.IsDeviceRegisteredInDb(messageObject!.DeviceId);
                if (!isRegistered.IsNullOrEmpty())
                {
                    var pongMessage = new MqttApplicationMessageBuilder()
                        .WithTopic("devices/registration/response/" + isRegistered + "/regSuccess")
                        .WithQualityOfServiceLevel(e.ApplicationMessage.QualityOfServiceLevel)
                        .WithRetainFlag(e.ApplicationMessage.Retain)
                        .Build();
                    await mqttClient.PublishAsync(pongMessage, CancellationToken.None);
                }
                else
                {
                    var pongMessage = new MqttApplicationMessageBuilder()
                        .WithTopic("devices/registration/response/" + messageObject.DeviceId + "/regFail")
                        .WithQualityOfServiceLevel(e.ApplicationMessage.QualityOfServiceLevel)
                        .WithRetainFlag(e.ApplicationMessage.Retain)
                        .Build();
                    await mqttClient.PublishAsync(pongMessage, CancellationToken.None);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.InnerException);
                Console.WriteLine(exc.StackTrace);
            }
        };
        
        var mqttClientOptions2 = new MqttClientOptionsBuilder()
            .WithTcpServer("mqtt.flespi.io", 1883)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithCredentials("FlespiToken "+ Environment.GetEnvironmentVariable("Flespitoken"), "")
            .Build();
        
        await mqttClient2.ConnectAsync(mqttClientOptions2, CancellationToken.None);
        var mqttSubForGps = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic("gps/data"))
            .Build();
        await mqttClient2.SubscribeAsync(mqttSubForGps, CancellationToken.None);
        mqttClient2.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                var message = e.ApplicationMessage.ConvertPayloadToString();
                var messageObject = JsonSerializer.Deserialize<DeviceWantsToLogCordsDto>(message);

                var userId = await deviceRepository.GetUserIdByDevice(messageObject!.DeviceId);

                var runStartTime = messageObject.gpsCordsList[0].TimeStamp;

                var runEndTime = messageObject.gpsCordsList[^1].TimeStamp;

                var formattedRunStartTime = runStartTime.ToString("s");

                var timeOfRun = runEndTime - runStartTime;

                string runId = $"{userId}_{formattedRunStartTime.Replace("/", "").Replace(":", "").Replace(" ", "")}";

                await deviceRepository.LogCoordinates(runId, userId, runStartTime, runEndTime, timeOfRun,
                    messageObject.gpsCordsList);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.InnerException);
                Console.WriteLine(exc.StackTrace);
            }
        };
    }
}