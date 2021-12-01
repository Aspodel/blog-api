using AutoMapper;
using BlogApi.Core.Entities;
using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services;
using BlogApi.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailService _emailService;
        private readonly IOptionsMonitor<JwtTokenConfig> _tokenConfigOptionsAccessor;

        public AuthController(UserManager userManager,
                              SignInManager<User> signInManager,
                              IMapper mapper,
                              ILogger<AuthController> logger,
                              IEmailService emailService,
                              IOptionsMonitor<JwtTokenConfig> tokenConfigOptionsAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
            _tokenConfigOptionsAccessor = tokenConfigOptionsAccessor;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user is null)
                return new BadRequestObjectResult(new { message = "Username or password is incorrect" });

            var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!passwordCheck.Succeeded)
                return new BadRequestObjectResult(new { message = "Username or password is incorrect" });

            var tokenConfig = _tokenConfigOptionsAccessor.CurrentValue;
            var requestAt = DateTime.UtcNow;
            var expiresIn = requestAt + tokenConfig.ExpiresIn;
            var token = await GenerateToken(user, tokenConfig, expiresIn);
            var refresh_token = Guid.NewGuid().ToString().Replace("-", "");
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                State = 200,
                Msg = "OK",
                Lang = "en",
                Data = new
                {
                    requestAt,
                    expiresIn = tokenConfig.ExpiresIn.TotalSeconds,
                    tokenType = tokenConfig.TokenType,
                    accessToken = token,
                    refresh_token,
                    user.Email,
                    roles
                }
            });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string guid, string token)
        {
            if (string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(token))
                return NotFound();


            var user = await _userManager.FindByGuidAsync(guid);
            if (user is null)
                return NotFound();


            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);


            var result = await _userManager.ConfirmEmailAsync(user, normalToken);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok("Email confirmed successfully");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(string guid, string token)
        {
            var user = await _userManager.FindByGuidAsync(guid);
            if (user is null || user.IsDeleted)
                return NotFound();

            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManager.ResetPasswordAsync(user, token, "password");
            if (!result.Succeeded)
                return BadRequest(result);

            //Send email
            EmailModel email = new()
            {
                Subject = "Your password has been reset successfully",

            };

            return Ok("Reset password successfully");
        }

        private async Task<string> GenerateToken(User user, JwtTokenConfig tokenConfig, DateTime expires)
        {
            var handler = new JwtSecurityTokenHandler();

            var roles = await _userManager.GetRolesAsync(user);

            var identity = new ClaimsIdentity(
                new GenericIdentity(user.UserName, "TokenAuth"),
                new[] { new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), new Claim("id", user.Id.ToString()) }
                .Union(roles.Select(role => new Claim(ClaimTypes.Role, role)))
            );

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenConfig.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = tokenConfig.Issuer,
                Audience = tokenConfig.Issuer,
                SigningCredentials = creds,
                Subject = identity,
                Expires = expires
            });

            return handler.WriteToken(securityToken);
        }
    }
}
