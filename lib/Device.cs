using Iot;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Iot
{
  public class Device
  {
    private bool is509 = false;
    private bool isLeaf = false;

    private readonly ILog Log;

    private Model.TelemetryType _telemetryType;
    private DeviceClient deviceClient;
    private Model.Configuration config;

    public Device(ILog log,
    Model.Configuration configuration, Model.TelemetryType telemetryType)
    {
      Log = log;
      config = configuration;
      _telemetryType = telemetryType;

      if (config.Protocol == TransportType.Amqp) Log.Info("Protocol: AMQP");
      else Log.Info("Protocol: MQTT");

      if (config.ConnectionString.Contains("509")) {
        is509 = true;
        log.Info("Authentication: x509");
      }
      else log.Info("Authentication: SymmetricKey");

      if (config.ConnectionString != null && config.EdgeHost != null) {
        config.ConnectionString = config.ConnectionString + ";GatewayHostName=" + config.EdgeHost;
        log.Info("Gateway: " + config.EdgeHost);
        isLeaf = true;
      }

    }

    private Task<MethodResponse> SetInterval(MethodRequest methodRequest, object userContext)
    {
      int interval = config.Interval;
      var data = Encoding.UTF8.GetString(methodRequest.Data);
      if (int.TryParse(data, out interval))
      {
        config.Interval = interval;
        Log.Info($"Telemetry interval set to {data} seconds");

        string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
        return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
      }
      else
      {
        string result = "{\"result\":\"Invalid parameter\"}";
        return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
      }
    }

    private async void SendTelemetry()
    {
      Random rand = new Random();

      while (true)
      {
        var telemetry = new Model.Climate();
        var message = new Message(Encoding.ASCII.GetBytes(telemetry.toJson()));
        message.Properties.Add("TelemetryType", _telemetryType.ToString());

        Log.Info("Sending message: " + Encoding.ASCII.GetString(message.GetBytes()));
        await deviceClient.SendEventAsync(message);
        await Task.Delay(config.Interval * 1000);
      }
    }

    public void Start()
    {
      var deviceType = "IOT DEVICE";
      var authType = "SYMMETRICKEY";
      if (isLeaf) deviceType = "LEAF DEVICE";
      if (is509) authType = "X509";

      if (config.ConnectionString == null) {
        Log.Debug("Provision Host: " + config.ProvisionHost);
        Log.Debug("Id Scope: " + config.IdScope);

        // TODO: Initialize DPS SDK
      } else {
        Log.Info($"-----------------{deviceType} {authType}------------------");
        Log.Info("Connection String: " + config.ConnectionString);
      }

      deviceClient = DeviceClient.CreateFromConnectionString(config.ConnectionString, config.Protocol);
      deviceClient.SetMethodHandlerAsync("SetInterval", SetInterval, null).Wait();
      SendTelemetry();
      while (true) { }
    }
  }
}
