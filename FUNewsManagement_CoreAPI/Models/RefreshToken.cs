using System;
using System.Collections.Generic;

namespace FUNewsManagement_CoreAPI.Models;

public partial class RefreshToken
{
    public int RefreshTokenId { get; set; }

    public int AccountId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual SystemAccount Account { get; set; } = null!;
}
