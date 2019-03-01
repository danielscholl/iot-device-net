using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace IOT
{
  class Program
  {
    private readonly static string _connectionString = Environment.GetEnvironmentVariable("DEVICE_CONNECTION_STRING");
    private static int interval = 2;
    private static DeviceClient client;

    private static Task<MethodResponse> SetInterval(MethodRequest methodRequest, object userContext)
    {
      var data = Encoding.UTF8.GetString(methodRequest.Data);
      if (Int32.TryParse(data, out interval))
      {
        Log(ConsoleColor.Blue, String.Format("Telemetry interval set to {0} seconds", data));

        string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
        return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
      }
      else
      {
        string result = "{\"result\":\"Invalid parameter\"}";
        return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
      }
    }

    private static async void SendTelemetry()
    {
      Random rand = new Random();

      while (true)
      {
        var telemetry = new Model.Climate();
        var message = new Message(Encoding.ASCII.GetBytes(telemetry.toJson()));
        message.Properties.Add("TelemetryType", "Climate");

        Log(ConsoleColor.Green, "Sending message: " + Encoding.ASCII.GetString(message.GetBytes()));
        await client.SendEventAsync(message);
        Log(ConsoleColor.DarkGray, "SendEvent: " + DateTime.UtcNow);

        await Task.Delay(interval * 1000);
      }
    }

    private static void Main(string[] args)
    {
      client = DeviceClient.CreateFromConnectionString(_connectionString);
      client.SetMethodHandlerAsync("SetInterval", SetInterval, null).Wait();
      SendTelemetry();
      while (true) { }
    }

    public static void Log(ConsoleColor color, string message)
    {
      Console.ForegroundColor = color;
      Console.WriteLine(message);
      Console.ResetColor();
    }
  }
}
