using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using log4net;
using log4net.Config;
using log4net.Core;

namespace Iot
{
  class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
    private static string ca = (String.IsNullOrEmpty(Environment.GetEnvironmentVariable("CA_PATH")))
                                      ? "./cert/root-ca.pem" : Environment.GetEnvironmentVariable("CA_PATH");

    static void Main(string[] args)
    {
      // Setup the logger
      var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
      XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

      var config = new Model.Configuration();
      if (!String.IsNullOrEmpty(config.EdgeHost) && File.Exists(ca))
      {
        var cert = new X509Certificate2("cert/riit.pfx", "password", X509KeyStorageFlags.Exportable);

        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        try
        {
          store.Open(OpenFlags.ReadWrite);
          store.Add(cert);
        }
        finally
        {
          store.Close();
        }
      }
      else
      {
        var cert = new X509Certificate2("cert/device.pfx", "password", X509KeyStorageFlags.Exportable);

        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        try
        {
          store.Open(OpenFlags.ReadWrite);
          store.Add(cert);
        }
        finally
        {
          store.Close();
        }
      }

      var device = new Device(Log, config, Model.TelemetryType.CLIMATE);
      device.Start();
    }

    //   private static void Main(string[] args)
    //   {
    // // Setup the logger
    // var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
    // XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

    //     var config = new Model.Configuration();
    //     if (!String.IsNullOrEmpty(config.EdgeHost) && File.Exists(ca))
    //     {
    //       var cert = new X509Certificate2("./cert/device.pfx", "password", X509KeyStorageFlags.Exportable);
    //       var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
    //       try
    //       {
    //         store.Open(OpenFlags.ReadWrite);
    //         // this line throws
    //         store.Add(cert);
    //       }
    //       finally
    //       {
    //         store.Close();
    //       }
    //     }
    //     else
    //     {
    //       X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
    //       store.Open(OpenFlags.ReadWrite);

    //       // How do I get a Cert from key and cert PEM files??
    //     }
    //     var device = new Device(Log, config, Model.TelemetryType.CLIMATE);
    //     device.Start();
    //   }
  }
}
