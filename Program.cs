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
    private static Model.TelemetryType model;
    private static Model.Configuration config;

    // We use log4net for our logger
    private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

    private static void Main(string[] args)
    {
      // Setup the logger
      var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
      XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
      Log.Info("Starting up");

      config = new Model.Configuration();

      switch (Environment.GetEnvironmentVariable("MODEL"))
      {
        default:
          model = Model.TelemetryType.CLIMATE;
          break;
      }

      //TODO:  If Edge Host setup Certificate

      var device = new Device(Log, config, model);
      device.Start();
    }
  }
}
