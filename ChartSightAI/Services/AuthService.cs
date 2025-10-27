using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using ChartSightAI.MVVM.Models;
using Microsoft.Maui.Storage;
using Supabase;

public class AuthService
{
    private readonly Client _supabase;
    private bool _initialized;

    private const string SessionKey = "user_session";

    public AuthService(Client supabase) => _supabase = supabase;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        await _supabase.InitializeAsync();
        _initialized = true;
    }

    public async Task<UserSession?> GetSession()
    {
        await InitializeAsync();
        var ensured = await EnsureSessionAsync();
        if (ensured != null) return ensured;

        return await GetCachedSession();
    }

    public async Task<UserSession?> EnsureSessionAsync()
    {
        await InitializeAsync();

        var cached = await GetCachedSession();
        if (cached == null) return null;

        if (!string.IsNullOrWhiteSpace(cached.AccessToken) && !string.IsNullOrWhiteSpace(cached.RefreshToken))
            _supabase.Auth.SetSession(cached.AccessToken, cached.RefreshToken);
        else
            return null;

        if (IsExpiringSoon(cached.ExpiresAt))
        {
            try
            {
                var refreshed = await _supabase.Auth.RefreshSession();
                if (refreshed != null)
                {
                    var us = ToUserSession(refreshed);
                    await SetCachedSession(us);
                    return us;
                }
                await ClearCachedSession();
                return null;
            }
            catch
            {
                await ClearCachedSession();
                return null;
            }
        }

        return cached;
    }

    public async Task<UserSession?> Login(string email, string password)
    {
        await InitializeAsync();
        var session = await _supabase.Auth.SignIn(email, password);
        if (session == null) return null;

        var us = ToUserSession(session);
        await SetCachedSession(us);
        return us;
    }

    public async Task<UserSession?> SignUp(string email, string password)
    {
        await InitializeAsync();
        var session = await _supabase.Auth.SignUp(email, password);
        if (session == null) return null;

        var us = ToUserSession(session);
        await SetCachedSession(us);
        return us;
    }

    public async Task Logout()
    {
        await InitializeAsync();
        try
        {
            await _supabase.Auth.SignOut();
        }
        catch (Exception ex)
        {

            Debug.WriteLine(ex.Message);
        }
        await ClearCachedSession();
    }

    public async Task<string?> GetUserEmailAsync()
    {
        await InitializeAsync();
        var live = _supabase?.Auth?.CurrentUser?.Email;
        if (!string.IsNullOrWhiteSpace(live))
            return live;

        var cached = await GetCachedSession();
        return cached?.Email;
    }

    public async Task<Guid?> GetUserIdAsync()
    {
        await InitializeAsync();

        var idStr = _supabase?.Auth?.CurrentUser?.Id;
        if (!string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out var uid))
            return uid;

        var cached = await GetCachedSession();
        if (!string.IsNullOrWhiteSpace(cached?.UserId) && Guid.TryParse(cached.UserId, out var uid2))
            return uid2;

        return null;
    }

    private static bool IsExpiringSoon(DateTime? expiresAtUtc)
    {
        if (expiresAtUtc == null) return true;
        // Refresh if expires within 2 minutes
        return expiresAtUtc.Value <= DateTime.UtcNow.AddMinutes(2);
    }

    private static UserSession ToUserSession(dynamic supaSession)
    {
        return new UserSession
        {
            AccessToken = supaSession.AccessToken,
            RefreshToken = supaSession.RefreshToken,
            ExpiresAt = supaSession.ExpiresAt(),
            UserId = supaSession.User?.Id,
            Email = supaSession.User?.Email
        };
    }

    private static string Serialize(UserSession s) => JsonSerializer.Serialize(s);
    private static UserSession? Deserialize(string json) => string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<UserSession>(json);

    private static async Task SetCachedSession(UserSession s) =>
        await SecureStorage.SetAsync(SessionKey, Serialize(s));

    private static async Task<UserSession?> GetCachedSession() =>
        Deserialize(await SecureStorage.GetAsync(SessionKey));

    private static Task ClearCachedSession()
    {
        SecureStorage.Remove(SessionKey);
        return Task.CompletedTask;
    }
}
