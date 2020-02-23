using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthPlayground.Models
{
    public class User
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    }
}
