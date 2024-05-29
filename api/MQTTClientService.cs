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

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithCredentials("Flespitoken "+ Environment.GetEnvironmentVariable("Flespitoken"))
            .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var mqttSubForCheck = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic("device/registration/check/"))
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
                if (isRegistered.IsNullOrEmpty())
                {
                    var pongMessage = new MqttApplicationMessageBuilder()
                        .WithTopic("device/registration/response/" + isRegistered + "/regSuccess")
                        .WithQualityOfServiceLevel(e.ApplicationMessage.QualityOfServiceLevel)
                        .WithRetainFlag(e.ApplicationMessage.Retain)
                        .Build();
                    await mqttClient.PublishAsync(pongMessage, CancellationToken.None);
                }
                else
                {
                    var pongMessage = new MqttApplicationMessageBuilder()
                        .WithTopic("device/registration/response/" + messageObject.DeviceId + "/regFail")
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
        var mqttSubForGps = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic("gps/data/"))
            .Build();
        await mqttClient.SubscribeAsync(mqttSubForGps, CancellationToken.None);
        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                var message = e.ApplicationMessage.ConvertPayloadToString();
                var messageObject = JsonSerializer.Deserialize<DeviceWantsToLogCordsDto>(message);

                var userId = await deviceRepository.GetUserIdByDevice(messageObject!.DeviceId);

                var runStartTime = messageObject.Coordinates[0].TimeStamp;

                var runEndTime = messageObject.Coordinates[^1].TimeStamp;

                var formattedRunStartTime = runStartTime.ToString("s");

                var timeOfRun = runEndTime - runStartTime;

                string runId = $"{userId}_{formattedRunStartTime.Replace("/", "").Replace(":", "").Replace(" ", "")}";

                await deviceRepository.LogCoordinates(runId, userId, runStartTime, runEndTime, timeOfRun,
                    messageObject.Coordinates);
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