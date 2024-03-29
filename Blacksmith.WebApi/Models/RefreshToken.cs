﻿using System.ComponentModel.DataAnnotations;

namespace Blacksmith.WebApi.Models
{
    public class RefreshToken
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public DateTime TokenExp { get; set; }
        public UserModel User { get; set; }

        public RefreshToken()
        {
            Token = Guid.NewGuid().ToString("N");
            TokenExp = DateTime.UtcNow.AddDays(30);
        }
    }
}
