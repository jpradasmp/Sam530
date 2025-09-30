namespace Sam530.Services
{
    public class ShutdownService
    {
        private readonly IHostApplicationLifetime _appLifetime;

        public ShutdownService(IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
        }

        public void Stop()
        {
            _appLifetime.StopApplication();
        }
    }
}
