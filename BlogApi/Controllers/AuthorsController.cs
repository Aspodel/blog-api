using AutoMapper;
using BlogApi.Contract;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BlogApi.Utils.Constants;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly AuthorManager _authorManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsController> _logger;
        private readonly IEmailService _emailService;
        private readonly IBlogRepository _blogRepository;

        public AuthorsController(AuthorManager authorManager, IMapper mapper, ILogger<AuthorsController> logger, IEmailService emailService, IBlogRepository blogRepository)
        {
            _authorManager = authorManager;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
            _blogRepository = blogRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var authors = await _authorManager.FindAll().ToListAsync(cancellationToken);
            return Ok(_mapper.Map<IEnumerable<AuthorDTO>>(authors));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var author = await _authorManager.FindByGuidAsync(id);
            if (author is null)
                return NotFound();

            return Ok(_mapper.Map<AuthorDTO>(author));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO dto)
        {
            var author = _mapper.Map<Author>(dto);

            var result = await _authorManager.CreateAsync(author, dto.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Unable to create author {username}. Result details: {result}", dto.Username, string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
                return BadRequest(result);
            }

            // Send email for account confirmation
            await SendEmailConfirmation(author);

            // Add user to specified roles
            var addToRolesResult = await _authorManager.AddToRoleAsync(author, Roles.Author);
            if (!addToRolesResult.Succeeded)
            {
                _logger.LogError("Unable to assign user {username} to roles {roles}. Result details: {result}", dto.Username, string.Join(", ", dto.Roles), string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
                return BadRequest("Fail to add role");
            }

            return Ok(_mapper.Map<AuthorDTO>(author));
        }

        [HttpPut("{guid}")]
        public async Task<IActionResult> Update(AuthorDTO dto)
        {
            var author = await _authorManager.FindByGuidAsync(dto.Guid);
            if (author is null)
                return NotFound();

            _mapper.Map(dto, author);

            ICollection<Blog> blogs = author.Blogs;
            ICollection<int> requestBlogs = dto.Blogs;
            ICollection<int> originalBlogs = author.Blogs.Select(b => b.Id).ToList();

            // Delete blogs from author
            ICollection<int> deleteBlogs = originalBlogs.Except(requestBlogs).ToList();
            if (deleteBlogs.Count > 0)
            {
                foreach (var blog in deleteBlogs)
                {
                    var foundBlog = await _blogRepository.FindByIdAsync(blog);
                    if (foundBlog is null)
                        return NotFound($"BlogId {blog} not found");

                    blogs.Remove(foundBlog);
                }
            }

            // Add new blogs to author
            ICollection<int> newBlogs = requestBlogs.Except(originalBlogs).ToList();
            if (newBlogs.Count > 0)
            {
                foreach (var blog in newBlogs)
                {
                    var foundBlog = await _blogRepository.FindByIdAsync(blog);
                    if (foundBlog is null)
                        return NotFound($"BlogId {blog} not found");

                    blogs.Add(foundBlog);
                }
            }

            await _authorManager.UpdateAsync(author);

            ICollection<string> requestRoles = dto.Roles;
            ICollection<string> originalRoles = await _authorManager.GetRolesAsync(author);

            // Delete Roles
            ICollection<string> deleteRoles = originalRoles.Except(requestRoles).ToList();
            if (deleteRoles.Count > 0)
                await _authorManager.RemoveFromRolesAsync(author, deleteRoles);

            // Add Roles
            ICollection<string> newRoles = requestRoles.Except(originalRoles).ToList();
            if (newRoles.Count > 0)
                await _authorManager.AddToRolesAsync(author, newRoles);

            return NoContent();
        }

        [HttpDelete("{guid}")]
        public async Task<IActionResult> Delete(string guid)
        {
            var author = await _authorManager.FindByGuidAsync(guid);
            if (author is null)
                return NotFound();

            author.IsDeleted = true;
            await _authorManager.UpdateAsync(author);
            return NoContent();
        }

        private async Task SendEmailConfirmation(Author author)
        {
            // Encode confirmation token
            var confirmEmailToken = await _authorManager.GenerateEmailConfirmationTokenAsync(author);
            var validEmailToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmEmailToken));

            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
            string confirmUrl = $"{baseUrl}/api/users/ConfirmEmail?guid={author.Guid}&token={validEmailToken}";


            // Get email template
            string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\EmailConfirmation.html";
            StreamReader str = new(FilePath);
            string templateBody = str.ReadToEnd();
            str.Close();
            templateBody = templateBody.Replace("[username]", author.UserName).Replace("[confirmUrl]", confirmUrl);


            EmailModel emailModel = new()
            {
                To = author.Email,
                Subject = $"Welcome {author.FirstName} {author.LastName} to be a BlogHub's author",
                Body = templateBody
            };

            await _emailService.Send(emailModel);
        }
    }
}
