using System.ComponentModel.DataAnnotations;

namespace Blacksmith.WebApi.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime TokenExp { get; set; }
        public UserModel User { get; set; }

        public RefreshToken()
        {
            Token = Guid.NewGuid().ToString("N");
            TokenExp = DateTime.UtcNow.AddDays(30);
        }
    }
}
