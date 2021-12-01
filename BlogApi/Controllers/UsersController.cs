using AutoMapper;
using BlogApi.Core.Entities;
using BlogApi.DTOs;
using BlogApi.DTOs.Create;
using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BlogApi.Utils.Constants;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;
        private readonly IEmailService _emailService;

        public UsersController(UserManager userManager, IMapper mapper, ILogger<UsersController> logger, IEmailService emailService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var users = await _userManager.FindAll().ToListAsync(cancellationToken);
            return Ok(_mapper.Map<IEnumerable<UserDTO>>(users));
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> Get(string guid)
        {
            var user = await _userManager.FindByGuidAsync(guid);
            if (user is null)
                return NotFound();

            return Ok(_mapper.Map<UserDTO>(user));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO dto)
        {
            var user = _mapper.Map<User>(dto);

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Unable to create user {username}. Result details: {result}", dto.Username, string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
                return BadRequest(result);
            }

            // Send email for account confirmation
            await SendEmailConfirmation(user);

            // Add user to specified roles
            var addToRolesResult = await _userManager.AddToRolesAsync(user, dto.Roles);
            if (!addToRolesResult.Succeeded)
            {
                _logger.LogError("Unable to assign user {username} to roles {roles}. Result details: {result}", dto.Username, string.Join(", ", dto.Roles), string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
                return BadRequest("Fail to add role");
            }

            return Ok(_mapper.Map<UserDTO>(user));
        }

        [HttpPut("{guid}")]
        public async Task<IActionResult> Update(UserDTO dto)
        {
            var user = await _userManager.FindByGuidAsync(dto.Guid);
            if (user is null || user.IsDeleted)
                return NotFound();

            _mapper.Map(dto, user);
            await _userManager.UpdateAsync(user);

            ICollection<string> requestRoles = dto.Roles;
            ICollection<string> originalRoles = await _userManager.GetRolesAsync(user);

            // Delete Roles
            ICollection<string> deleteRoles = originalRoles.Except(requestRoles).ToList();
            if (deleteRoles.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, deleteRoles);

            // Add Roles
            ICollection<string> newRoles = requestRoles.Except(originalRoles).ToList();
            if (newRoles.Count > 0)
                await _userManager.AddToRolesAsync(user, newRoles);

            return NoContent();
        }

        [HttpDelete("{guid}")]
        public async Task<IActionResult> Delete(string guid)
        {
            var user = await _userManager.FindByGuidAsync(guid);
            if (user is null)
                return NotFound();

            user.IsDeleted = true;
            await _userManager.UpdateAsync(user);
            return NoContent();
        }

        private async Task SendEmailConfirmation(User user)
        {
            // Encode confirmation token
            var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var validEmailToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmEmailToken));

            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
            string confirmUrl = $"{baseUrl}/api/auth/confirm-email?guid={user.Guid}&token={validEmailToken}";

            //Get email template
            string templateBody = _emailService.GetTemplate(TemplateType.EmailConfirmation);
            templateBody = templateBody.Replace("[username]", user.UserName).Replace("[confirmUrl]", confirmUrl);

            // Get email template
            // string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\EmailConfirmation.html";
            // StreamReader str = new(FilePath);
            // string templateBody = str.ReadToEnd();
            // str.Close();
            // templateBody = templateBody.Replace("[username]", user.UserName).Replace("[confirmUrl]", confirmUrl);


            EmailModel emailModel = new()
            {
                To = user.Email,
                Subject = $"Welcome {user.FirstName} {user.LastName} to BlogHub",
                Body = templateBody
            };

            await _emailService.Send(emailModel);
        }
    }
}
