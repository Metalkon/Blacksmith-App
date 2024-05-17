using Microsoft.AspNetCore.Components.Authorization;
using Shared_Classes.Models;
using System.Net.Http.Json;

namespace Blacksmith.Blazor.Services
{
    public class PlayerDataService
    {
        public event Action? Action;
        public bool DisplayData { get; set; }
        public string Username { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Schematics { get; set; }
        public int Gold { get; set; }

        private readonly HttpClientTokenService _client;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public PlayerDataService(HttpClientTokenService client, AuthenticationStateProvider authenticationStateProvider)
        {
            _client = client;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<bool> UpdatePlayerData()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated == true)
            {
                var response = await _client.GetAsync($"https://localhost:8000/api/Account/PlayerData");
                var result = await response.Content.ReadFromJsonAsync<PlayerDataDTO>();

                if (result != null)
                {
                    Username = result.Username ?? "N/A";
                    Level = result.Level;
                    Experience = result.Experience;
                    Schematics = result.Schematics;
                    Gold = result.Gold;
                    DisplayData = true;
                    Action?.Invoke();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
