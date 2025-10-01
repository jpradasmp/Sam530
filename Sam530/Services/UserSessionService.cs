using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Sam530.Services
{
    public class UserSessionService
    {
        private readonly ProtectedSessionStorage _storage;
        private const string StorageKey = "user";

        public bool IsLoggedIn { get; private set; } = false;
        public string? UserName { get; private set; }

        public UserSessionService(ProtectedSessionStorage storage)
        {
            _storage = storage;
        }

        public async Task LoginAsync(string username)
        {
            IsLoggedIn = true;
            UserName = username;
            await _storage.SetAsync(StorageKey, username);
        }

        public async Task RestoreAsync()
        {
            var result = await _storage.GetAsync<string>(StorageKey);
            if (result.Success && result.Value is not null)
            {
                IsLoggedIn = true;
                UserName = result.Value;
            }
            else
            {
                IsLoggedIn = false;
                UserName = null;
            }
        }

        public async Task LogoutAsync()
        {
            IsLoggedIn = false;
            UserName = null;
            await _storage.DeleteAsync(StorageKey);
        }
    }
}

