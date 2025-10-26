// Services/AuthService.cs
using ChartSightAI.MVVM.Models;
using ChartSightAI.Services.Interfaces;
using Supabase;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace ChartSightAI.Services
{
    public class AuthService
    {
        private readonly Client _supabase;
        private bool _initialized;

        public AuthService(Client supabase) => _supabase = supabase;

        public async Task InitializeAsync()
        {
            if (_initialized) return;
            await _supabase.InitializeAsync();
            _initialized = true;
            await RestoreCachedSessionIntoClient();
        }

        public async Task<UserSession> GetSession()
        {
            var sessionJson = await SecureStorage.GetAsync("user_session");
            if (string.IsNullOrEmpty(sessionJson)) return null;
            return JsonSerializer.Deserialize<UserSession>(sessionJson);
        }

        public async Task<UserSession> Login(string email, string password)
        {
            await InitializeAsync();
            var session = await _supabase.Auth.SignIn(email, password);
            if (session == null) return null;

            var userSession = new UserSession
            {
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken,
                ExpiresAt = session.ExpiresAt(),
                UserId = session.User.Id,
                Email = session.User.Email
            };

            await SecureStorage.SetAsync("user_session", JsonSerializer.Serialize(userSession));
            return userSession;
        }

        public async Task Logout()
        {
            await InitializeAsync();
            await _supabase.Auth.SignOut();
            SecureStorage.Remove("user_session");
        }

        public async Task<UserSession> SignUp(string email, string password)
        {
            await InitializeAsync();
            var session = await _supabase.Auth.SignUp(email, password);
            if (session == null) return null;

            var userSession = new UserSession
            {
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken,
                ExpiresAt = session.ExpiresAt(),
                UserId = session.User.Id,
                Email = session.User.Email
            };

            await SecureStorage.SetAsync("user_session", JsonSerializer.Serialize(userSession));
            return userSession;
        }

        private async Task RestoreCachedSessionIntoClient()
        {
            var cached = await GetSession();
            if (cached == null) return;

            if (!string.IsNullOrWhiteSpace(cached.AccessToken) && !string.IsNullOrWhiteSpace(cached.RefreshToken))
            {
                _supabase.Auth.SetSession(cached.AccessToken, cached.RefreshToken);
                var refreshed = await _supabase.Auth.RefreshSession();
                if (refreshed != null)
                {
                    var userSession = new UserSession
                    {
                        AccessToken = refreshed.AccessToken,
                        RefreshToken = refreshed.RefreshToken,
                        ExpiresAt = refreshed.ExpiresAt(),
                        UserId = refreshed.User.Id,
                        Email = refreshed.User.Email
                    };
                    await SecureStorage.SetAsync("user_session", JsonSerializer.Serialize(userSession));
                }
            }
        }
    }
}
