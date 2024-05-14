using Shared_Classes.Models;
using System.Net.Http.Json;

namespace Blacksmith.Blazor.Services
{
    public class PlayerDataService
    {
        public event Action? Action;
        public string Username { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Schematics { get; set; }
        public int Gold { get; set; }

        private readonly HttpClientTokenService _client;

        public PlayerDataService(HttpClientTokenService client)
        {
            _client = client;
        }

        public async Task<bool> UpdatePlayerData()
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

                Action?.Invoke();
                return true;
            }
            else 
            {
                return false;
            }
        }
    }
}
