using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement_CoreAPI.DTOs;

// Một dòng trong danh sách bài viết (màn 4.3)
public record NewsListItemDto(
    int NewsArticleId,
    string NewsTitle,
    string CategoryName,
    DateTime CreatedDate,
    bool NewsStatus,
    string AuthorName,
    int ViewCount);

public record TagSimpleDto(int TagId, string TagName);

// Chi tiết một bài viết (màn 4.5)
public record NewsDetailDto(
    int NewsArticleId,
    string NewsTitle,
    string? Headline,
    string NewsContent,
    string? NewsSource,
    string? ImageUrl,
    bool NewsStatus,
    int ViewCount,
    int CategoryId,
    string CategoryName,
    int CreatedById,
    string AuthorName,
    DateTime CreatedDate,
    DateTime? ModifiedDate,
    List<TagSimpleDto> Tags);

// "Một trang" kết quả: dùng chung cho mọi danh sách có phân trang
public record PagedResult<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

// NHẬN VÀO khi tạo/sửa bài viết
public class NewsRequest
{
    [Required(ErrorMessage = "Title is required"), StringLength(200)]
    public string NewsTitle { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Headline { get; set; }

    [Required(ErrorMessage = "Content is required")]
    public string NewsContent { get; set; } = string.Empty;

    [StringLength(200)]
    public string? NewsSource { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    public bool NewsStatus { get; set; } = true;

    [MinLength(1, ErrorMessage = "At least one tag is required")]
    public List<int> TagIds { get; set; } = new();

    // TẠM THỜI: id người đang thao tác. Buổi 3 sẽ lấy từ JWT token và XÓA trường này.
    [Range(1, int.MaxValue, ErrorMessage = "Current user is required")]
    public int CurrentUserId { get; set; }
}

// Dùng cho upload ảnh (Phần 3)
public class ImageUploadRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
}