using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement_CoreAPI.DTOs;

// GỬI RA: không có mật khẩu!
public record AccountDto(
    int AccountId,
    string AccountName,
    string AccountEmail,
    int AccountRole,
    string RoleName);

// NHẬN VÀO khi tạo tài khoản
public class AccountCreateRequest
{
    [Required, StringLength(100)]
    public string AccountName { get; set; } = string.Empty;

    [Required, EmailAddress(ErrorMessage = "Invalid email format"), StringLength(100)]
    public string AccountEmail { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Range(1, 2, ErrorMessage = "Role must be 1 (Admin) or 2 (Staff)")]
    public int AccountRole { get; set; } = 2;
}

// NHẬN VÀO khi sửa: KHÔNG có mật khẩu (đổi mật khẩu có API riêng)
public class AccountUpdateRequest
{
    [Required, StringLength(100)]
    public string AccountName { get; set; } = string.Empty;

    [Required, EmailAddress(ErrorMessage = "Invalid email format"), StringLength(100)]
    public string AccountEmail { get; set; } = string.Empty;

    [Range(1, 2, ErrorMessage = "Role must be 1 (Admin) or 2 (Staff)")]
    public int AccountRole { get; set; }
}

// NHẬN VÀO khi đổi mật khẩu
public class ChangePasswordRequest
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string NewPassword { get; set; } = string.Empty;
}