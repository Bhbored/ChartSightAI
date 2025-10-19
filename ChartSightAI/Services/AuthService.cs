using ChartSightAI.MVVM.Models;
using ChartSightAI.Services.Interfaces;
using Supabase;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace ChartSightAI.Services
{
    public class AuthService : IAuthService
    {
        private readonly Client _supabase;

        public AuthService()
        {
            var supabaseUrl = "YOUR_SUPABASE_URL";
            var supabaseKey = "YOUR_SUPABASE_KEY";

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            _supabase = new Client(supabaseUrl, supabaseKey, options);
        }

        public async Task<UserSession> GetSession()
        {
            var sessionJson = await SecureStorage.GetAsync("user_session");
            if (string.IsNullOrEmpty(sessionJson))
            {
                return null;
            }

            return JsonSerializer.Deserialize<UserSession>(sessionJson);
        }

        public async Task<UserSession> Login(string email, string password)
        {
            var session = await _supabase.Auth.SignIn(email, password);
            if (session != null)
            {
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

            return null;
        }

        public async Task Logout()
        {
            await _supabase.Auth.SignOut();
            SecureStorage.Remove("user_session");
        }

        public async Task<UserSession> SignUp(string email, string password)
        {
            var session = await _supabase.Auth.SignUp(email, password);
            if (session != null)
            {
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

            return null;
        }

        //public async Task<UserSession> GoogleLogin()
        //{
        //    var session = await _supabase.Auth.SignInWithOAuth(Supabase.Gotrue.Constants.Provider.Google, new Supabase.Gotrue.Client.SignInWithOAuthOptions
        //    {
        //        RedirectTo = "chartsightai://callback"
        //    });

        //    if (session != null)
        //    {
        //        var userSession = new UserSession
        //        {
        //            AccessToken = session.AccessToken,
        //            RefreshToken = session.RefreshToken,
        //            ExpiresAt = session.ExpiresAt(),
        //            UserId = session.User.Id,
        //            Email = session.User.Email
        //        };

        //        await SecureStorage.SetAsync("user_session", JsonSerializer.Serialize(userSession));
        //        return userSession;
        //    }

        //    return null;
        //}
    }
}
