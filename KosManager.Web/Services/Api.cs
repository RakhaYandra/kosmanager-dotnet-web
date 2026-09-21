using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace KosManager.Web.Services;

public class AuthState(ProtectedSessionStorage storage)
{
    private const string Key = "km-auth";

    public string? Token { get; private set; }
    public string? Email { get; private set; }
    public string? Role { get; private set; }
    public bool LoggedIn => Token is not null;
    public bool IsOwner => Role == "owner";
    public event Action? Changed;

    public async Task EnsureLoadedAsync()
    {
        if (LoggedIn) return;
        try
        {
            var res = await storage.GetAsync<Saved>(Key);
            if (res.Success && res.Value is not null)
            {
                Token = res.Value.Token; Email = res.Value.Email; Role = res.Value.Role;
                Changed?.Invoke();
            }
        }
        catch (InvalidOperationException) { /* prerender: JS interop belum ada */ }
    }

    public async Task SetAsync(string token, string email, string role)
    {
        Token = token; Email = email; Role = role;
        try { await storage.SetAsync(Key, new Saved(token, email, role)); } catch { }
        Changed?.Invoke();
    }

    public async Task ClearAsync()
    {
        Token = Email = Role = null;
        try { await storage.DeleteAsync(Key); } catch { }
        Changed?.Invoke();
    }

    private record Saved(string Token, string Email, string Role);
}

public class ApiClient(IHttpClientFactory f, AuthState auth)
{
    private HttpClient Client()
    {
        var c = f.CreateClient("api");
        if (auth.Token is not null)
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return c;
    }

    public async Task<(string Token, string Role)> LoginAsync(string email, string password)
    {
        var c = f.CreateClient("api");
        var res = await c.PostAsJsonAsync("/api/auth/login", new { email, password });
        if (!res.IsSuccessStatusCode) throw new Exception("Email atau kata sandi salah.");
        var doc = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", doc!["token"]);
        var meRes = await c.SendAsync(req);
        var meDoc = await meRes.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        return (doc["token"], meDoc.GetProperty("role").GetString()!);
    }

    public Task<T?> Get<T>(string url) => Client().GetFromJsonAsync<T>(url);
    public async Task<T?> Post<T>(string url, object body)
    {
        var res = await Client().PostAsJsonAsync(url, body);
        res.EnsureSuccessStatusCode();
        if (typeof(T) == typeof(bool)) return (T)(object)true;
        return await res.Content.ReadFromJsonAsync<T>();
    }
    public async Task Put(string url, object body)
    {
        var res = await Client().PutAsJsonAsync(url, body);
        res.EnsureSuccessStatusCode();
    }
    public async Task Delete(string url)
    {
        var res = await Client().DeleteAsync(url);
        res.EnsureSuccessStatusCode();
    }
}
