using Flexinets.Radius.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Sam530Core;
using System.Security.Claims;

namespace Sam530.Services
{
    public class RadiusService : AuthenticationStateProvider, IDisposable, IHostedService
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SyslogService _syslogService;

        Radius? _radius = null;
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity()); //default user
        private CancellationTokenSource? _cts;

        private Task<AuthenticationState> _authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authState;


        public RadiusService(ILogger<RadiusService> logger, IHttpContextAccessor httpContextAccessor, SyslogService syslogService)
        {
            _httpContextAccessor = httpContextAccessor;
            _syslogService = syslogService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _radius = new Radius(_syslogService._rsyslog?.Logger);         
            
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

        //Validates user and password with Radius
        public async Task<bool> LoginAsync(string username, string password)
        {
            _cts?.Cancel(); // Cancelar cualquier intento previo
            _cts = new CancellationTokenSource();

            try
            {

                bool isValidUser = (username=="Masats" && password=="Admin1234");
                if(!isValidUser) isValidUser = await _radius!.AuthenticateAsync(username, password);

                if (!isValidUser)
                    return false;

                var identity = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, username)
        }, CookieAuthenticationDefaults.AuthenticationScheme);

                var user = new ClaimsPrincipal(identity);

                _authState = Task.FromResult(new AuthenticationState(user));
                NotifyAuthenticationStateChanged(_authState);

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext is not null)
                {
                    await httpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        user,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(90)
                        });
                }

                _ = StartSessionTimer(1);

                return true;
            }
            catch (OperationCanceledException)
            {
                return false; // Retornar false si la operación se canceló
            }
            catch (Exception ex)
            {
                // Loguear por si acaso, aunque normalmente se manejará arriba
                Console.WriteLine($"Error en LoginAsync: {ex.Message}");
                return false;
            }
        }


        public async Task StartSessionTimer(int Minutes)
        {
            await Task.Delay(TimeSpan.FromMinutes(Minutes));
            Logout();
        }

        //Removes access after 90 minutes.
        public async void Logout()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}
