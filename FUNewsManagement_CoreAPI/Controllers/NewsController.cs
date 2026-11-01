using FUNewsManagement_CoreAPI.DTOs;
using FUNewsManagement_CoreAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FUNewsManagement_CoreAPI.Controllers;

[ApiController]
[Route("api/news")]
public class NewsController : ControllerBase
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxImageSize = 2 * 1024 * 1024; // 2 MB

    private readonly FUNewsManagementContext _context;
    private readonly IWebHostEnvironment _env;   // để biết thư mục wwwroot nằm đâu

    public NewsController(FUNewsManagementContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET /api/news?search=ai&categoryId=1&status=true&sortDesc=true&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] bool? status,
        [FromQuery] bool sortDesc = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        // Chặn giá trị vô lý
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var query = _context.NewsArticles.AsQueryable();

        // 1. Lọc
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(n => n.NewsTitle.Contains(s)
                                  || (n.Headline != null && n.Headline.Contains(s)));
        }
        if (categoryId.HasValue)
            query = query.Where(n => n.CategoryId == categoryId.Value);
        if (status.HasValue)
            query = query.Where(n => n.NewsStatus == status.Value);

        // 2. Sắp xếp theo ngày tạo
        query = sortDesc
            ? query.OrderByDescending(n => n.CreatedDate)
            : query.OrderBy(n => n.CreatedDate);

        // 3. Đếm tổng TRƯỚC khi cắt trang
        int totalItems = await query.CountAsync();

        // 4. Cắt lấy đúng một trang
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NewsListItemDto(
                n.NewsArticleId, n.NewsTitle, n.Category.CategoryName,
                n.CreatedDate, n.NewsStatus, n.CreatedBy.AccountName, n.ViewCount))
            .ToListAsync();

        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        return Ok(new PagedResult<NewsListItemDto>(items, page, pageSize, totalItems, totalPages));
    }

    // GET /api/news/5  -> chi tiết, kèm danh sách tag
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var news = await _context.NewsArticles
            .Where(n => n.NewsArticleId == id)
            .Select(n => new NewsDetailDto(
                n.NewsArticleId, n.NewsTitle, n.Headline, n.NewsContent, n.NewsSource,
                n.ImageUrl, n.NewsStatus, n.ViewCount, n.CategoryId, n.Category.CategoryName,
                n.CreatedById, n.CreatedBy.AccountName, n.CreatedDate, n.ModifiedDate,
                n.Tags.Select(t => new TagSimpleDto(t.TagId, t.TagName)).ToList()))
            .FirstOrDefaultAsync();

        if (news == null)
            return NotFound(new { message = $"Article {id} not found." });
        return Ok(news);
    }

    // POST /api/news/5/view  -> tăng lượt xem (trang chi tiết sẽ gọi)
    [HttpPost("{id}/view")]
    public async Task<IActionResult> IncreaseView(int id)
    {
        int affected = await _context.NewsArticles
            .Where(n => n.NewsArticleId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.ViewCount, n => n.ViewCount + 1));

        if (affected == 0)
            return NotFound(new { message = $"Article {id} not found." });
        return NoContent();
    }

    // POST /api/news  -> tạo bài viết
    [HttpPost]
    public async Task<IActionResult> Create(NewsRequest request)
    {
        var (error, tags) = await ValidateAsync(request);
        if (error != null)
            return BadRequest(new { message = error });

        var article = new NewsArticle
        {
            NewsTitle = request.NewsTitle.Trim(),
            Headline = request.Headline,
            NewsContent = request.NewsContent,
            NewsSource = request.NewsSource,
            NewsStatus = request.NewsStatus,
            CategoryId = request.CategoryId,
            CreatedById = request.CurrentUserId,
            CreatedDate = DateTime.Now,
            ViewCount = 0
        };
        foreach (var tag in tags)
            article.Tags.Add(tag);   // EF tự ghi các dòng vào bảng NewsTag

        _context.NewsArticles.Add(article);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = article.NewsArticleId },
            new { article.NewsArticleId, message = "Article created successfully." });
    }

    // PUT /api/news/5  -> sửa bài viết
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, NewsRequest request)
    {
        var article = await _context.NewsArticles
            .Include(n => n.Tags)                 // lấy kèm danh sách tag hiện tại
            .FirstOrDefaultAsync(n => n.NewsArticleId == id);
        if (article == null)
            return NotFound(new { message = $"Article {id} not found." });

        var (error, tags) = await ValidateAsync(request);
        if (error != null)
            return BadRequest(new { message = error });

        article.NewsTitle = request.NewsTitle.Trim();
        article.Headline = request.Headline;
        article.NewsContent = request.NewsContent;
        article.NewsSource = request.NewsSource;
        article.NewsStatus = request.NewsStatus;
        article.CategoryId = request.CategoryId;
        article.UpdatedById = request.CurrentUserId;
        article.ModifiedDate = DateTime.Now;

        // Thay toàn bộ tag: xóa hết cái cũ, gắn lại theo danh sách mới
        article.Tags.Clear();
        foreach (var tag in tags)
            article.Tags.Add(tag);

        await _context.SaveChangesAsync();
        return Ok(new { message = "Article updated successfully." });
    }

    // DELETE /api/news/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await _context.NewsArticles
            .Include(n => n.Tags)
            .FirstOrDefaultAsync(n => n.NewsArticleId == id);
        if (article == null)
            return NotFound(new { message = $"Article {id} not found." });

        article.Tags.Clear();                    // gỡ liên kết trong bảng NewsTag trước
        _context.NewsArticles.Remove(article);
        await _context.SaveChangesAsync();

        DeleteImageFile(article.ImageUrl);       // dọn file ảnh nếu có
        return NoContent();
    }

    // ---------- Hàm phụ ----------

    // Kiểm tra Category, người dùng và Tag. Trả về 2 giá trị: thông báo lỗi và danh sách tag
    private async Task<(string? Error, List<Tag> Tags)> ValidateAsync(NewsRequest request)
    {
        bool categoryOk = await _context.Categories
            .AnyAsync(c => c.CategoryId == request.CategoryId && c.IsActive);
        if (!categoryOk)
            return ("Category does not exist or is inactive.", new List<Tag>());

        bool userOk = await _context.SystemAccounts
            .AnyAsync(a => a.AccountId == request.CurrentUserId);
        if (!userOk)
            return ("User does not exist.", new List<Tag>());

        var tagIds = request.TagIds.Distinct().ToList();
        var tags = await _context.Tags.Where(t => tagIds.Contains(t.TagId)).ToListAsync();
        if (tags.Count != tagIds.Count)
            return ("One or more tags do not exist.", new List<Tag>());

        return (null, tags);
    }

    private void DeleteImageFile(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;
        var path = Path.Combine(_env.WebRootPath,
            imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }
    // POST /api/news/5/image  -> upload ảnh cho bài viết
    [HttpPost("{id}/image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(int id, [FromForm] ImageUploadRequest request)
    {
        var article = await _context.NewsArticles.FindAsync(id);
        if (article == null)
            return NotFound(new { message = $"Article {id} not found." });

        var file = request.File;

        // 1. Kiểm tra dung lượng
        if (file.Length == 0)
            return BadRequest(new { message = "File is empty." });
        if (file.Length > MaxImageSize)
            return BadRequest(new { message = "Image must be 2 MB or smaller." });

        // 2. Kiểm tra loại file: cả đuôi file lẫn kiểu nội dung
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension) || !file.ContentType.StartsWith("image/"))
            return BadRequest(new { message = "Only .jpg, .jpeg, .png and .webp images are allowed." });

        // 3. Lưu file với tên ngẫu nhiên
        var folder = Path.Combine(_env.WebRootPath, "images", "news");
        Directory.CreateDirectory(folder);   // tạo thư mục nếu chưa có
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(folder, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 4. Xóa ảnh cũ (nếu có) rồi cập nhật đường dẫn mới
        DeleteImageFile(article.ImageUrl);
        article.ImageUrl = $"/images/news/{fileName}";
        await _context.SaveChangesAsync();

        return Ok(new { imageUrl = article.ImageUrl });
    }
}