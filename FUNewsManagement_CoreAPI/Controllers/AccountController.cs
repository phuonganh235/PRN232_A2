using FUNewsManagement_CoreAPI.DTOs;
using FUNewsManagement_CoreAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FUNewsManagement_CoreAPI.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly FUNewsManagementContext _context;

    public AccountController(FUNewsManagementContext context)
    {
        _context = context;
    }

    // GET /api/account?role=2  -> danh sách, lọc theo vai trò (màn 4.8)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? role)
    {
        var query = _context.SystemAccounts.AsQueryable();
        if (role.HasValue)
            query = query.Where(a => a.AccountRole == role.Value);

        var data = await query
            .Select(a => new AccountDto(a.AccountId, a.AccountName, a.AccountEmail,
                a.AccountRole, a.AccountRole == 1 ? "Admin" : "Staff"))
            .ToListAsync();
        return Ok(data);
    }

    // GET /api/account/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _context.SystemAccounts
            .Where(a => a.AccountId == id)
            .Select(a => new AccountDto(a.AccountId, a.AccountName, a.AccountEmail,
                a.AccountRole, a.AccountRole == 1 ? "Admin" : "Staff"))
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound(new { message = $"Account {id} not found." });
        return Ok(account);
    }

    // POST /api/account  -> tạo, chặn trùng email
    [HttpPost]
    public async Task<IActionResult> Create(AccountCreateRequest request)
    {
        var email = request.AccountEmail.Trim().ToLower();
        if (await _context.SystemAccounts.AnyAsync(a => a.AccountEmail == email))
            return Conflict(new { message = $"Email '{email}' is already in use." });

        var account = new SystemAccount
        {
            AccountName = request.AccountName.Trim(),
            AccountEmail = email,
            AccountPassword = request.Password,
            AccountRole = request.AccountRole
        };
        _context.SystemAccounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = account.AccountId },
            new AccountDto(account.AccountId, account.AccountName, account.AccountEmail,
                account.AccountRole, account.AccountRole == 1 ? "Admin" : "Staff"));
    }

    // PUT /api/account/5  -> sửa thông tin (không đụng mật khẩu)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AccountUpdateRequest request)
    {
        var account = await _context.SystemAccounts.FindAsync(id);
        if (account == null)
            return NotFound(new { message = $"Account {id} not found." });

        var email = request.AccountEmail.Trim().ToLower();
        if (await _context.SystemAccounts.AnyAsync(a => a.AccountEmail == email && a.AccountId != id))
            return Conflict(new { message = $"Email '{email}' is already in use." });

        account.AccountName = request.AccountName.Trim();
        account.AccountEmail = email;
        account.AccountRole = request.AccountRole;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Account updated successfully." });
    }

    // PUT /api/account/5/password  -> đổi mật khẩu, BẮT BUỘC đúng mật khẩu cũ
    [HttpPut("{id}/password")]
    public async Task<IActionResult> ChangePassword(int id, ChangePasswordRequest request)
    {
        var account = await _context.SystemAccounts.FindAsync(id);
        if (account == null)
            return NotFound(new { message = $"Account {id} not found." });

        if (account.AccountPassword != request.OldPassword)
            return BadRequest(new { message = "Old password is incorrect." });

        if (request.NewPassword == request.OldPassword)
            return BadRequest(new { message = "New password must be different from the old one." });

        account.AccountPassword = request.NewPassword;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Password changed successfully." });
    }

    // DELETE /api/account/5  -> không xóa tài khoản đã viết/sửa bài
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var account = await _context.SystemAccounts.FindAsync(id);
        if (account == null)
            return NotFound(new { message = $"Account {id} not found." });

        bool hasArticles = await _context.NewsArticles
            .AnyAsync(n => n.CreatedById == id || n.UpdatedById == id);
        if (hasArticles)
            return Conflict(new { message = "Cannot delete this account because it has created or edited articles." });

        // Xóa refresh token của tài khoản (nếu có) trước, vì bảng RefreshToken trỏ tới Account
        await _context.RefreshTokens.Where(r => r.AccountId == id).ExecuteDeleteAsync();

        _context.SystemAccounts.Remove(account);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}