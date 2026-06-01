using System.Text.Json;
using Microsoft.JSInterop;

namespace PetiteMaisonEpouvante.UI.Services
{
    public interface IAuthenticationService
    {
        Task SimulateLogin(string userId, string userName);
        Task Logout();
        Task<bool> IsAuthenticated();
        Task<(string UserId, string UserName)?> GetCurrentUser();
        event Action OnAuthenticationChanged;
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private const string StorageKey = "auth_user";
        
        public event Action OnAuthenticationChanged;

        public AuthenticationService(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public async Task SimulateLogin(string userId, string userName)
        {
            var userData = new { UserId = userId, UserName = userName };
            var json = JsonSerializer.Serialize(userData);
            
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
                // Add demo header so API can accept the simulated user in development
                _httpClient.DefaultRequestHeaders.Remove("X-Demo-User");
                _httpClient.DefaultRequestHeaders.Add("X-Demo-User", userId);
                OnAuthenticationChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la simulation de connexion : {ex.Message}");
            }
        }

        public async Task Logout()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
                _httpClient.DefaultRequestHeaders.Remove("X-Demo-User");
                OnAuthenticationChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la déconnexion : {ex.Message}");
            }
        }

        public async Task<bool> IsAuthenticated()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
                var exists = !string.IsNullOrEmpty(json);
                if (exists)
                {
                    // ensure header set for current session
                    try
                    {
                        var userData = JsonSerializer.Deserialize<JsonElement>(json);
                        var userId = userData.GetProperty("UserId").GetString();
                        if (!string.IsNullOrEmpty(userId))
                        {
                            _httpClient.DefaultRequestHeaders.Remove("X-Demo-User");
                            _httpClient.DefaultRequestHeaders.Add("X-Demo-User", userId);
                        }
                    }
                    catch { }
                }

                return exists;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(string UserId, string UserName)?> GetCurrentUser()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
                if (string.IsNullOrEmpty(json))
                    return null;

                var userData = JsonSerializer.Deserialize<JsonElement>(json);
                var userId = userData.GetProperty("UserId").GetString();
                var userName = userData.GetProperty("UserName").GetString();
                
                // ensure header present
                if (!string.IsNullOrEmpty(userId))
                {
                    _httpClient.DefaultRequestHeaders.Remove("X-Demo-User");
                    _httpClient.DefaultRequestHeaders.Add("X-Demo-User", userId);
                }

                return (userId, userName);
            }
            catch
            {
                return null;
            }
        }
    }
}
