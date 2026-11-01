using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement_CoreAPI.DTOs;

// Phiếu GỬI RA: thông tin chuyên mục kèm số bài viết
public record CategoryDto(
    int CategoryId,
    string CategoryName,
    string? CategoryDescription,
    bool IsActive,
    int ArticleCount);

// Phiếu NHẬN VÀO: dữ liệu khi tạo/sửa chuyên mục
public class CategoryRequest
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name must be at most 100 characters")]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? CategoryDescription { get; set; }

    public bool IsActive { get; set; } = true;
}