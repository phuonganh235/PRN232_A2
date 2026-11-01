using FUNewsManagement_CoreAPI.DTOs;
using FUNewsManagement_CoreAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FUNewsManagement_CoreAPI.Controllers;

[ApiController]
[Route("api/category")]
public class CategoryController : ControllerBase
{
    private readonly FUNewsManagementContext _context;

    public CategoryController(FUNewsManagementContext context)
    {
        _context = context;
    }

    // GET /api/category  -> danh sách + số bài mỗi chuyên mục
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _context.Categories
            .Select(c => new CategoryDto(
                c.CategoryId, c.CategoryName, c.CategoryDescription,
                c.IsActive, c.NewsArticles.Count))
            .ToListAsync();
        return Ok(data);
    }

    // GET /api/category/5  -> một chuyên mục
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _context.Categories
            .Where(c => c.CategoryId == id)
            .Select(c => new CategoryDto(
                c.CategoryId, c.CategoryName, c.CategoryDescription,
                c.IsActive, c.NewsArticles.Count))
            .FirstOrDefaultAsync();   // không thấy thì trả về null

        if (category == null)
            return NotFound(new { message = $"Category {id} not found." });

        return Ok(category);
    }

    // POST /api/category  -> tạo mới
    [HttpPost]
    public async Task<IActionResult> Create(CategoryRequest request)
    {
        var category = new Category
        {
            CategoryName = request.CategoryName.Trim(),
            CategoryDescription = request.CategoryDescription,
            IsActive = request.IsActive
        };

        _context.Categories.Add(category);   // đưa vào "giỏ hàng"
        await _context.SaveChangesAsync();   // "thanh toán": thực sự ghi xuống DB

        var result = new CategoryDto(category.CategoryId, category.CategoryName,
            category.CategoryDescription, category.IsActive, 0);

        // 201 Created + đường dẫn tới chuyên mục vừa tạo
        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, result);
    }

    // PUT /api/category/5  -> sửa
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound(new { message = $"Category {id} not found." });

        category.CategoryName = request.CategoryName.Trim();
        category.CategoryDescription = request.CategoryDescription;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync();   // EF tự biết những gì đã đổi
        return Ok(new { message = "Category updated successfully." });
    }

    // PATCH /api/category/5/toggle  -> bật/tắt IsActive (màn 4.6)
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound(new { message = $"Category {id} not found." });

        category.IsActive = !category.IsActive;   // đảo trạng thái
        await _context.SaveChangesAsync();
        return Ok(new { message = "Status changed.", isActive = category.IsActive });
    }

    // DELETE /api/category/5  -> xóa, KHÔNG cho xóa nếu còn bài viết
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound(new { message = $"Category {id} not found." });

        bool hasArticles = await _context.NewsArticles.AnyAsync(n => n.CategoryId == id);
        if (hasArticles)
            return Conflict(new { message = "Cannot delete this category because it contains articles." });

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();   // 204: xóa xong, không có gì để trả về
    }
}