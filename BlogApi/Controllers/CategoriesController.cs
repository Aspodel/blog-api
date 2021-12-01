using AutoMapper;
using BlogApi.Contract;
using BlogApi.Core.Entities;
using BlogApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var categories = await _categoryRepository.FindAll().ToListAsync(cancellationToken);

            return Ok(_mapper.Map<IEnumerable<CategoryDTO>>(categories));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var category = await _categoryRepository.FindByIdAsync(id);
            if (category is null)
                return NotFound();

            return Ok(_mapper.Map<CategoryDTO>(category));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryDTO dto, CancellationToken cancellationToken = default)
        {
            var category = _mapper.Map<Category>(dto);
            _categoryRepository.Add(category);
            await _categoryRepository.SaveChangesAsync(cancellationToken);
            return Ok(_mapper.Map<CategoryDTO>(category));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(CategoryDTO dto, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.FindByIdAsync(dto.Id);
            if (category is null)
                return NotFound();

            _mapper.Map(dto, category);
            await _categoryRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.FindByIdAsync(id);
            if (category is null)
                return NotFound();

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpGet("get-by-slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var category = await _categoryRepository.FindBySlug(slug);
            if (category is null)
                return NotFound();

            return Ok(_mapper.Map<CategoryDTO>(category));
        }
    }
}
