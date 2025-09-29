using DataStore;
using DataStore.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Sam530Core;
using System.Runtime.InteropServices;


namespace Sam530.Services
{
    public class GenIOBridge : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        TelnetServer? _server=null;

        public GenIOBridge(ILogger<GenIOBridge> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            LinuxManager.SetLogger(_logger);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Starting Sam530 Service");

            DbStore.logger = _logger;
            TelnetClient.logger = _logger;
            TelnetServer.logger = _logger;
            SerialDevice.logger = _logger;
            StateManager.logger = _logger;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SEContext.SqliteConnectionString = _config["DbConnectionString:windows"]!;
                Settings.FirmwarePath = _config["FirmwarePath:windows"]!;
            }
            else
            {
                SEContext.SqliteConnectionString = _config["DbConnectionString:linux"]!;
                Settings.FirmwarePath = _config["FirmwarePath:linux"]!;
            }
            


            //Start Telnet Server
            _server = new TelnetServer();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                _server.EndPoint = "192.168.1.39";
            else
                _server.EndPoint = LinuxManager.GetIPAddress()[0].ToString(); //"192.168.1.39";
            _server.Port = 22000;
                       

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _server.COMPort = _config["SerialServer:windows"];
            }
            else
            {
                _server.COMPort = _config["SerialServer:linux"];
            }
            _server.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //Close Telnet Server
            _logger.LogWarning("Sam530 Service Finished");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            //Unmanaged to clear
        }
    }
}
