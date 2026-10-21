using CaoThaiToan_2380602281_KTGK_WebAPI.models;
using CaoThaiToan_2380602281_KTGK_WebAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [Route("api/category")]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepo;

        public CategoryApiController(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        // Lấy danh sách danh mục (công khai cho tất cả mọi người)
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryRepo.GetCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryRepo.GetCategoryByIdAsync(id);
            if (category == null) return NotFound(new { message = "Không tìm thấy danh mục." });
            return Ok(category);
        }

        // Admin có quyền Thêm danh mục
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                return BadRequest(new { message = "Tên danh mục không được để trống." });

            await _categoryRepo.AddCategoryAsync(category);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        // Admin có quyền Sửa danh mục
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            if (id != category.Id)
                return BadRequest(new { message = "ID danh mục không khớp." });

            await _categoryRepo.UpdateCategoryAsync(category);
            return NoContent();
        }

        // Admin có quyền Xóa danh mục (Các thiết bị liên quan sẽ tự động set CategoryId = NULL)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _categoryRepo.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}
