using AutoMapper;
using BlogApi.Contract;
using BlogApi.Core.Entities;
using BlogApi.DTOs;
using BlogApi.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogsController : ControllerBase
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IMapper _mapper;
        private readonly AuthorManager _authorManager;
        private readonly ICategoryRepository _categoryRepository;

        public BlogsController(IBlogRepository blogRepository, IMapper mapper, AuthorManager authorManager, ICategoryRepository categoryRepository)
        {
            _blogRepository = blogRepository;
            _mapper = mapper;
            _authorManager = authorManager;
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var blogs = await _blogRepository.FindAll().ToListAsync(cancellationToken);

            return Ok(_mapper.Map<IEnumerable<BlogDTO>>(blogs));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var blog = await _blogRepository.FindByIdAsync(id);
            if (blog is null)
                return NotFound();

            return Ok(_mapper.Map<BlogDTO>(blog));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BlogDTO dto, CancellationToken cancellationToken = default)
        {
            var blog = _mapper.Map<Blog>(dto);

            // Add authors to blog
            foreach (var author in dto.Authors)
            {
                var foundAuthor = await _authorManager.FindByGuidAsync(author);
                if (foundAuthor is null)
                    return NotFound($"AuthorGuid {author} not found");

                blog.Authors.Add(foundAuthor);
            }

            // Add categories to blog
            foreach (var category in dto.Categories)
            {
                var foundCategory = await _categoryRepository.FindByIdAsync(category);
                if (foundCategory is null)
                    return NotFound($"CategoryId {category} not found");

                blog.Categories.Add(foundCategory);
            }

            _blogRepository.Add(blog);
            await _blogRepository.SaveChangesAsync(cancellationToken);
            return CreatedAtAction(nameof(Get), new { blog.Id }, _mapper.Map<UserDTO>(blog));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] BlogDTO dto, CancellationToken cancellationToken = default)
        {
            var blog = await _blogRepository.FindByIdAsync(dto.Id);
            if (blog is null)
                return NotFound();

            _mapper.Map(dto, blog);

            ICollection<Author> authors = blog.Authors;
            ICollection<string> requestAuthors = dto.Authors;
            ICollection<string> originalAuthors = blog.Authors.Select(b => b.Guid).ToList();

            // Delete authors from blog
            ICollection<string> deleteAuthors = originalAuthors.Except(requestAuthors).ToList();
            if (deleteAuthors.Count > 0)
            {
                foreach (var author in deleteAuthors)
                {
                    var foundAuthor = await _authorManager.FindByGuidAsync(author);
                    if (foundAuthor is null)
                        return NotFound($"AuthorGuid {author} not found");

                    authors.Remove(foundAuthor);
                }
            }

            // Add new authors to blog
            ICollection<string> newAuthors = requestAuthors.Except(originalAuthors).ToList();
            if (newAuthors.Count > 0)
            {
                foreach (var author in newAuthors)
                {
                    var foundAuthor = await _authorManager.FindByGuidAsync(author);
                    if (foundAuthor is null)
                        return NotFound($"AuthorGuid {author} not found");

                    authors.Add(foundAuthor);
                }
            }

            ICollection<Category> categories = blog.Categories;
            ICollection<int> requestCategories = dto.Categories;
            ICollection<int> originalCategories = blog.Categories.Select(b => b.Id).ToList();

            // Delete categories from blog
            ICollection<int> deleteCategories = originalCategories.Except(requestCategories).ToList();
            if (deleteCategories.Count > 0)
            {
                foreach (var category in deleteCategories)
                {
                    var foundCategory = await _categoryRepository.FindByIdAsync(category);
                    if (foundCategory is null)
                        return NotFound($"CategoryId {category} not found");

                    categories.Remove(foundCategory);
                }
            }

            // Add new categories to blog
            ICollection<int> newCategories = requestCategories.Except(originalCategories).ToList();
            if (newCategories.Count > 0)
            {
                foreach (var category in newCategories)
                {
                    var foundCategory = await _categoryRepository.FindByIdAsync(category);
                    if (foundCategory is null)
                        return NotFound($"CategoryId {category} not found");

                    categories.Add(foundCategory);
                }
            }

            blog.Authors = authors;
            blog.Categories = categories;
            _blogRepository.Update(blog);
            await _blogRepository.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            //var blog = await _blogRepository.FindByIdAsync(id, cancellationToken);
            //if (blog is null)
            //    return NotFound();

            //_blogRepository.Delete(blog);
            //await _blogRepository.SaveChangesAsync(cancellationToken);

            var blog = await _blogRepository.FindByIdAsync(id);
            if (blog is null)
                return NotFound();

            blog.IsDeleted = true;
            _blogRepository.Update(blog);

            return NoContent();
        }

        [HttpGet("get-by-slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var blog = await _blogRepository.FindBySlug(slug);
            if (blog is null)
                return NotFound();

            return Ok(_mapper.Map<BlogDTO>(blog));
        }
    }
}
