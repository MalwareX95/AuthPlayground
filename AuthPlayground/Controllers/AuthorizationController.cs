using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthPlayground.Models;
using AuthPlayground.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthPlayground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly AccountService accountService;

        public AuthorizationController(AccountService accountService)
        {
            this.accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<JsonWebToken>> Login(LoginModel loginModel)
        {
            try
            {
                return await accountService.SignInAsync(loginModel.Username, loginModel.Password);
            }
            catch (Exception)
            {
                return Unauthorized();
            }
        }

        [HttpPost("refresh")]
        public ActionResult<JsonWebToken> Refresh(RefreshRequest refreshRequest)
        {
            return accountService.RefreshAccessToken(refreshRequest.AccessToken, refreshRequest.RefreshToken);
        }
    }
}