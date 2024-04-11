using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace Blacksmith.Blazor.Services
{
    public class HttpClientTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;


        public HttpClientTokenService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
            _navigationManager = navigationManager; 
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            try
            {
                await CheckToken();
                var response = await _httpClient.GetAsync(url);
                await CheckStatus(response);
                return response;
            }
            catch (Exception ex)
            {
                await ErrorRedirect(ex.Message);
                throw new Exception($"ERROR: {ex.Message}");
            }
        }

        internal async Task<HttpResponseMessage> PutAsync<T, TData>(string url, TData data)
        {
            try
            {
                await CheckToken();
                var response = await _httpClient.PutAsJsonAsync(url, data);
                await CheckStatus(response);
                return response;
            }
            catch (Exception ex)
            {
                await ErrorRedirect(ex.Message);
                throw new Exception($"ERROR: {ex.Message}");

            }
        }

        internal async Task<HttpResponseMessage> PostAsync<T, TData>(string url, TData data)
        {
            try
            {
                await CheckToken();
                var response = await _httpClient.PostAsJsonAsync(url, data);
                await CheckStatus(response);
                return response;
            }
            catch (Exception ex)
            {
                await ErrorRedirect(ex.Message);
                throw new Exception($"ERROR: {ex.Message}");
            }
        }

        internal async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            try
            {
                await CheckToken();
                var response = await _httpClient.DeleteAsync(url);
                await CheckStatus(response);
                return response;
            }
            catch (Exception ex)
            {
                await ErrorRedirect(ex.Message);
                throw new Exception($"ERROR: {ex.Message}");
            }
        }

        // Check status code, redirect to error page if any issues
        private async Task CheckStatus(HttpResponseMessage httpResponse)
        {
            if (httpResponse.IsSuccessStatusCode || httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
            else
            {
                string error = await httpResponse.Content.ReadAsStringAsync();
                await ErrorRedirect(error);
                throw new Exception($"ERROR: {error}");
            }
        }

        // Redirect to error page
        private async Task ErrorRedirect(string error)
        {
            await _localStorage.SetItemAsync("error", error);
            _navigationManager.NavigateTo($"/error");
        }

        // Check if a jwt is expired and request a new one
        internal async Task CheckToken()
        {
            try
            {
                DateTime jwtExpiry = await JwtExpirationDateTime();
                string refreshToken = await _localStorage.GetItemAsStringAsync("refreshtoken");
                refreshToken = refreshToken.Replace("\"", "");

                if (string.IsNullOrEmpty(refreshToken))
                {
                    await Logout();
                    throw new Exception("Login Session Has ended");
                }
                if (jwtExpiry <= DateTime.UtcNow)
                {
                    HttpResponseMessage request = await _httpClient.PostAsJsonAsync($"https://localhost:8000/api/Account/refresh", refreshToken);
                    string result = await request.Content.ReadAsStringAsync();

                    if (request.IsSuccessStatusCode)
                    {
                        await _localStorage.SetItemAsync("token", result);
                        await _authStateProvider.GetAuthenticationStateAsync();
                    }
                    else
                    {
                        Console.WriteLine(result);
                        await Logout();
                        throw new Exception("Login Session Has ended");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred while checking JWT: {ex}");
            }
        }

        // Convert the stored jwt's expiration claim into DateTime
        private async Task<DateTime> JwtExpirationDateTime()
        {
            string token = await _localStorage.GetItemAsStringAsync("token");
            IEnumerable<Claim> claims = CustomAuthStateProvider.ParseClaimsFromJwt(token);
            Claim expirationClaim = claims.FirstOrDefault(c => c.Type == "exp");
            long.TryParse(expirationClaim.Value, out long unixTimestamp);
            DateTime expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
            return expirationDateTime;
        }

        private async Task Logout()
        {
            await _localStorage.RemoveItemAsync("token");
            await _localStorage.RemoveItemAsync("refreshtoken");
            await _localStorage.RemoveItemAsync("refreshexpiry");
            await _authStateProvider.GetAuthenticationStateAsync();
            _navigationManager.NavigateTo($"/login");
        }
    }
}
