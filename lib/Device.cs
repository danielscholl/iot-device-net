using Iot;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.ApplicationInsights;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

namespace Iot
{
  public class Device
  {
    private bool isLeaf = false;
    private bool is509 = false;
    private bool insights = false;

    private readonly ILog log;

    private Model.TelemetryType telemetryType;
    private DeviceClient deviceClient;
    private Model.Configuration config;

    private TelemetryClient telemetryClient;

    public Device(ILog log, Model.Configuration config, Model.TelemetryType telemetryType)
    {
      this.log = log;
      this.config = config;
      this.telemetryType = telemetryType;

      // Setup App Insights
      if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY")))
      {
        insights = true;
        this.telemetryClient = new TelemetryClient();
        this.telemetryClient.Context.Device.Id = Environment.GetEnvironmentVariable("DEVICE");
        this.telemetryClient.TrackEvent("IoTDeviceSimulator started");
        this.telemetryClient.GetMetric("SimulatorCount").TrackValue(1);
      }


      // Setup the Device Protocol
      switch (config.Protocol)
      {
        case TransportType.Mqtt:
          log.Info("Protocol: MQTT");
          break;
        case TransportType.Amqp:
          log.Info("Protocol: AMQP");
          break;
        default:
          log.Info("Protocol: MQTT");
          break;
      }

      // Determine if Symmetric Key or x509
      if (!string.IsNullOrEmpty(config.ConnectionString) && config.ConnectionString.Contains("509"))
        is509 = true;

      if (is509) log.Info("Authentication: x509");
      else log.Info("Authentication: SymmetricKey");

      // Determine if Down Stream Device
      if (!string.IsNullOrEmpty(config.ConnectionString) && !string.IsNullOrEmpty(config.EdgeHost))
      {
        config.ConnectionString = config.ConnectionString + ";GatewayHostName=" + config.EdgeHost;
        isLeaf = true;
        log.Info("Gateway: " + config.EdgeHost);
      }
    }

    private void provisionDevice()
    {
      throw new NotImplementedException();
    }

    private void c2dMessage()
    {
      throw new NotImplementedException();
    }

    private void desiredProperties()
    {
      throw new NotImplementedException();
    }

    private async void sendMessage()
    {
      var sendCount = 1;

      while (true)
      {
        var telemetry = new Model.Climate();
        telemetry.Count = sendCount;
        var message = new Message(Encoding.ASCII.GetBytes(telemetry.toJson()));
        message.Properties.Add("TelemetryType", telemetryType.ToString());

        try
        {
          log.Info("Sending message: " + Encoding.ASCII.GetString(message.GetBytes()));

          await deviceClient.SendEventAsync(message).ConfigureAwait(false);
          if (insights) telemetryClient.GetMetric("DeviceMsgSent").TrackValue(1);
          sendCount++;

          await Task.Delay(config.Interval * 1000).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
          log.ErrorFormat("Error with deviceId {0} while sending a message: {1}", telemetry.DeviceId, ex.Message + "\n" + ex.StackTrace);

          if (insights) telemetryClient.TrackTrace(String.Format("Error with deviceId {0} while sending a message: {1}", telemetry.DeviceId, ex.Message));
          if (insights) telemetryClient.GetMetric("DeviceSendError").TrackValue(1);

          if (ex.InnerException != null)
            log.ErrorFormat("Inner exception for deviceId {0} when sending a message: {1}", telemetry.DeviceId, ex.InnerException);

          await Task.Delay(config.Interval * 1000).ConfigureAwait(false);
        }

      }
    }

    private Task<MethodResponse> receiveMessage(MethodRequest methodRequest, object userContext)
    {
      int interval = config.Interval;
      var data = Encoding.UTF8.GetString(methodRequest.Data);
      if (int.TryParse(data, out interval))
      {
        config.Interval = interval;
        log.Info($"Telemetry interval set to {data} seconds");

        string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
        if (insights) telemetryClient.GetMetric("DeviceDirectMethod").TrackValue(1);
        return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
      }
      else
      {
        string result = "{\"result\":\"Invalid parameter\"}";
        return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
      }
    }



    public void Start()
    {
      var deviceType = "IOT DEVICE";
      var authType = "SYMMETRICKEY";
      if (isLeaf) deviceType = "LEAF DEVICE";
      if (is509) authType = "X509";

      if (string.IsNullOrEmpty(config.ConnectionString))
      {
        log.Debug("Provision Host: " + config.ProvisionHost);
        log.Debug("Id Scope: " + config.IdScope);

        // TODO: Initialize DPS SDK
        log.Error("provision not implemented");
      }
      else
      {
        log.Info($"-----------------{deviceType} {authType}------------------");
        log.Info("Connection String: " + config.ConnectionString);
      }

      if (!String.IsNullOrEmpty(config.ConnectionString))
      {
        if (is509)
        {
          var dict = config.ConnectionString.Split(';')
                      .Select(x => x.Split('='))
                      .ToDictionary(x => x[0], x => x[1]);
          var cert = new X509Certificate2(config.CertPath, "password");
          var auth = new DeviceAuthenticationWithX509Certificate(dict["DeviceId"], cert);
          deviceClient = DeviceClient.Create(dict["HostName"], auth, config.Protocol);
        }
        else
        {
          deviceClient = DeviceClient.CreateFromConnectionString(config.ConnectionString, config.Protocol);
        }

        deviceClient.SetMethodHandlerAsync("SetInterval", receiveMessage, null).Wait();
        sendMessage();
        while (true) { }
      }
      else
      {
        log.Error("Connection String: ");
      }

    }
  }
}
