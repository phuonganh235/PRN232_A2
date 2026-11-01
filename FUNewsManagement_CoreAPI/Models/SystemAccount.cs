using System;
using System.Collections.Generic;

namespace FUNewsManagement_CoreAPI.Models;

public partial class SystemAccount
{
    public int AccountId { get; set; }

    public string AccountName { get; set; } = null!;

    public string AccountEmail { get; set; } = null!;

    public string AccountPassword { get; set; } = null!;

    public int AccountRole { get; set; }

    public virtual ICollection<NewsArticle> NewsArticleCreatedBies { get; set; } = new List<NewsArticle>();

    public virtual ICollection<NewsArticle> NewsArticleUpdatedBies { get; set; } = new List<NewsArticle>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
