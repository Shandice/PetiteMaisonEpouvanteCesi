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
        private const string StorageKey = "auth_user";
        
        public event Action OnAuthenticationChanged;

        public AuthenticationService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SimulateLogin(string userId, string userName)
        {
            var userData = new { UserId = userId, UserName = userName };
            var json = JsonSerializer.Serialize(userData);
            
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
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
                return !string.IsNullOrEmpty(json);
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
                
                return (userId, userName);
            }
            catch
            {
                return null;
            }
        }
    }
}
