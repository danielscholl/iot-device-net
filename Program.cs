using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using log4net;
using log4net.Config;

namespace Iot
{
  class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

    static void Main(string[] args)
    {
      // Setup the logger
      var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
      XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

      // Setup the configuration File
      var config = new Model.Configuration();

      var directory = Directory.GetCurrentDirectory();
      if (directory.Contains("bin"))
      {
        // Visual Studio Path Hack
        directory = Directory.GetParent(directory).Parent.Parent.FullName;
      }

      var device_cert = Path.Combine(directory, "cert/device.pfx");

      if (File.Exists(device_cert)) config.CertPath = device_cert;

      var root_ca = Path.Combine(directory, "cert/root.pfx");
      if (File.Exists(root_ca) && !String.IsNullOrEmpty(config.EdgeHost))
      {
        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        try
        {
          store.Open(OpenFlags.ReadWrite);
          //store.Add(new X509Certificate2(X509Certificate.CreateFromCertFile(certPath))); ** Crash on MacOSx **
          store.Add(new X509Certificate2(root_ca, "password", X509KeyStorageFlags.Exportable));
        }
        finally
        {
          store.Close();
        }
      }

      var device = new Device(Log, config, Model.TelemetryType.CLIMATE);
      device.Start();
    }
  }
}
