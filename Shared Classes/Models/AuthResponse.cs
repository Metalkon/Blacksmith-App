using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Classes.Models
{
    public class AuthResponse
    {
        public string Message { get; set; }
        public string? Subject { get; set; }
        public string? Contents { get; set; }
    }
}
