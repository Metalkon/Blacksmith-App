using System.Net.Http.Json;
using System.Security.Claims;

namespace Blacksmith.Blazor.Services
{
    public class HttpClientTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public HttpClientTokenService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task CheckToken()
        {
            bool expired = await CheckJwt();
            if (expired == true)
            {
                await Logout();
                throw new Exception("Login Session Has ended");
            }
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync(string url, object data)
        {
            bool expired = await CheckJwt();
            if (expired == true)
            {
                await Logout();
                throw new Exception("Login Session Has ended");
            }
            return await _httpClient.PostAsJsonAsync(url, data);
        }

        public async Task<T> GetFromJsonAsync<T>(string url)
        {
            bool expired = await CheckJwt();
            if (expired == true)
            {
                await Logout();
                throw new Exception("Login Session Has ended");
            }
            return await _httpClient.GetFromJsonAsync<T>(url);
        }

        public async Task<HttpResponseMessage> PutAsJsonAsync(string url, object data)
        {
            bool expired = await CheckJwt();
            if (expired == true)
            {
                await Logout();
                throw new Exception("Login Session Has ended");
            }
            return await _httpClient.PutAsJsonAsync(url, data);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            bool expired = await CheckJwt();
            if (expired == true)
            {
                await Logout();
                throw new Exception("Login Session Has ended");
            }
            return await _httpClient.DeleteAsync(url);
        }

        // -----------------------------------
        // Token Management
        // -----------------------------------

        // Method that checks if a jwt is expired, and uses a valid refresh token to obtain a new one. Returns true if there are any issues to logout the user.
        public async Task<bool> CheckJwt()
        {
            try
            {
                DateTime jwtExpiry = await JwtExpirationDateTime();
                string refreshToken = await _localStorage.GetItemAsStringAsync("refreshtoken");
                refreshToken = refreshToken.Replace("\"", "");

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return true;
                }
                if (jwtExpiry >= DateTime.UtcNow)
                {
                    return false;
                }
                if (jwtExpiry <= DateTime.UtcNow)
                {
                    HttpResponseMessage request = await _httpClient.PostAsJsonAsync($"https://localhost:8000/api/Account/refresh", refreshToken);
                    string result = await request.Content.ReadAsStringAsync();

                    if (request.IsSuccessStatusCode)
                    {
                        await _localStorage.SetItemAsync("token", result);
                        await _authStateProvider.GetAuthenticationStateAsync();
                        return false;
                    }
                    else
                    {
                        Console.WriteLine(result);
                        return true;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred while checking JWT: {ex}");
                return true;
            }
        }

        // Convert the stored jwt's expiration claim into DateTime
        public async Task<DateTime> JwtExpirationDateTime()
        {
            string token = await _localStorage.GetItemAsStringAsync("token");
            IEnumerable<Claim> claims = CustomAuthStateProvider.ParseClaimsFromJwt(token);
            Claim expirationClaim = claims.FirstOrDefault(c => c.Type == "exp");
            long.TryParse(expirationClaim.Value, out long unixTimestamp);
            DateTime expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
            return expirationDateTime;
        }

        async Task Logout()
        {
            await _localStorage.RemoveItemAsync("token");
            await _localStorage.RemoveItemAsync("refreshtoken");
            await _localStorage.RemoveItemAsync("refreshexpiry");
            await _authStateProvider.GetAuthenticationStateAsync();
        }
    }
}
