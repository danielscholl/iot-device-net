using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using log4net.Core;

namespace Iot
{
  class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

    private static void Main(string[] args)
    {
      // Setup the logger
      var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
      XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

      var config = new Model.Configuration();
      var device = new Device(Log, config, Model.TelemetryType.CLIMATE);
      device.Start();
    }
  }
}
