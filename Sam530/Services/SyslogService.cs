using Sam530Core;
using Serilog.Context;

namespace Sam530.Services
{
    public class SyslogService : IHostedService, IDisposable
    {
       public RSyslog? _rsyslog = null;

        private readonly ILogger _logger;

        public SyslogService(ILogger<SyslogService>logger) 
        { 
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _rsyslog = new RSyslog();
            _rsyslog.Start();
            

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            //Unmanaged to clear
        }
    }
}
