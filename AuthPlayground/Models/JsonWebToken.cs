using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthPlayground.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class JsonWebToken
    {
        public string AccessToken { get; set; }
        public DateTime Expiration { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }
}
