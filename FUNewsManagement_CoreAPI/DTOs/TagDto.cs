using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement_CoreAPI.DTOs;

public record TagDto(int TagId, string TagName, string? Note, int ArticleCount);

public record TagArticleDto(int NewsArticleId, string NewsTitle);

public class TagRequest
{
    [Required(ErrorMessage = "Tag name is required")]
    [StringLength(50, ErrorMessage = "Tag name must be at most 50 characters")]
    public string TagName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Note { get; set; }
}