using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace Iot.Model
{

  public enum TelemetryType
  {
    CLIMATE
  }

  public class Configuration
  {
    public string ConnectionString { get; set; }
    public int Interval { get; set; } = 2;
    public string EdgeHost { get; set; }
    public string ProvisionHost { get; set; }
    public string IdScope { get; set; }
    public string RegistrationId { get; set; } = "device";
    public TransportType Protocol { get; set; } = TransportType.Amqp;

    public Configuration()
    {
      int _interval;
      int.TryParse(Environment.GetEnvironmentVariable("MESSAGE_INTERVAL"), out _interval);
      if (_interval > 0) Interval = _interval;

      if (Environment.GetEnvironmentVariable("PROTOCOL") == "MQTT") Protocol = TransportType.Mqtt;

      ConnectionString = Environment.GetEnvironmentVariable("DEVICE_CONNECTION_STRING");
      EdgeHost = Environment.GetEnvironmentVariable("EDGE_GATEWAY");
      ProvisionHost = Environment.GetEnvironmentVariable("DPS_HOST");
      IdScope = Environment.GetEnvironmentVariable("ID_SCOPE");
      RegistrationId = Environment.GetEnvironmentVariable("DEVICE");
    }
  }
  public class Climate
  {
    public string DeviceId { get; set; }
    public double WindSpeed { get; set; }
    public double Humidity { get; set; }
    public long TimeStamp { get; set; }

    public Climate()
    {
      Random rand = new Random();
      DeviceId = Environment.GetEnvironmentVariable("DEVICE");
      WindSpeed = 10 + rand.NextDouble() * 4;
      Humidity = 60 + rand.NextDouble() * 20;
      TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string toJson()
    {
      return JsonConvert.SerializeObject(this);
    }
  }
}
