using FUNewsManagement_CoreAPI.DTOs;
using FUNewsManagement_CoreAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FUNewsManagement_CoreAPI.Controllers;

[ApiController]
[Route("api/tag")]
public class TagController : ControllerBase
{
    private readonly FUNewsManagementContext _context;

    public TagController(FUNewsManagementContext context)
    {
        _context = context;
    }

    // GET /api/tag?search=ai  -> danh sách, có tìm kiếm (màn 4.7)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var query = _context.Tags.AsQueryable();   // bắt đầu "viết phiếu order"

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.TagName.Contains(search.Trim()));

        var data = await query
            .OrderBy(t => t.TagName)
            .Select(t => new TagDto(t.TagId, t.TagName, t.Note, t.NewsArticles.Count))
            .ToListAsync();                         // lúc này mới "gửi xuống bếp"
        return Ok(data);
    }

    // GET /api/tag/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tag = await _context.Tags
            .Where(t => t.TagId == id)
            .Select(t => new TagDto(t.TagId, t.TagName, t.Note, t.NewsArticles.Count))
            .FirstOrDefaultAsync();

        if (tag == null)
            return NotFound(new { message = $"Tag {id} not found." });
        return Ok(tag);
    }

    // GET /api/tag/5/articles  -> các bài viết dùng tag này (màn 4.7)
    [HttpGet("{id}/articles")]
    public async Task<IActionResult> GetArticles(int id)
    {
        var tag = await _context.Tags
            .Where(t => t.TagId == id)
            .Select(t => new
            {
                t.TagId,
                t.TagName,
                Articles = t.NewsArticles
                    .Select(n => new TagArticleDto(n.NewsArticleId, n.NewsTitle))
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (tag == null)
            return NotFound(new { message = $"Tag {id} not found." });
        return Ok(tag);
    }

    // POST /api/tag  -> tạo, chặn trùng tên
    [HttpPost]
    public async Task<IActionResult> Create(TagRequest request)
    {
        var name = request.TagName.Trim();

        bool exists = await _context.Tags.AnyAsync(t => t.TagName == name);
        if (exists)
            return Conflict(new { message = $"Tag '{name}' already exists." });

        var tag = new Tag { TagName = name, Note = request.Note };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = tag.TagId },
            new TagDto(tag.TagId, tag.TagName, tag.Note, 0));
    }

    // PUT /api/tag/5  -> sửa, chặn trùng tên với tag KHÁC
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TagRequest request)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null)
            return NotFound(new { message = $"Tag {id} not found." });

        var name = request.TagName.Trim();
        bool duplicated = await _context.Tags.AnyAsync(t => t.TagName == name && t.TagId != id);
        if (duplicated)
            return Conflict(new { message = $"Tag '{name}' already exists." });

        tag.TagName = name;
        tag.Note = request.Note;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Tag updated successfully." });
    }

    // DELETE /api/tag/5  -> không cho xóa tag đang được bài viết sử dụng
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tag = await _context.Tags
            .Include(t => t.NewsArticles)
            .FirstOrDefaultAsync(t => t.TagId == id);
        if (tag == null)
            return NotFound(new { message = $"Tag {id} not found." });

        if (tag.NewsArticles.Any())
            return Conflict(new { message = "Cannot delete this tag because it is used by articles." });

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}