using System;
using Newtonsoft.Json;

namespace IOT.Model
{
  public enum Protocol
  {
    AMQP,
    MQTT,
    HTTP
  }
  public class DeviceConfig
  {
    public string ConnectionString { get; set; }
    public int Interval { get; set; }
    public string EdgeHost { get; set; }
    public string ProvisionHost { get; set; }
    public string IdScope { get; set; }
    public string RegistrationId { get; set; }
    public Protocol Protocol { get; set; }

    public DeviceConfig()
    {
      int _interval;

      ConnectionString = Environment.GetEnvironmentVariable("DEVICE_CONNECTION_STRING");
      int.TryParse(Environment.GetEnvironmentVariable("MESSAGE_INTERVAL"), out _interval);
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
