using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FUNewsManagement_CoreAPI.Models;

public partial class FUNewsManagementContext : DbContext
{
    public FUNewsManagementContext(DbContextOptions<FUNewsManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<NewsArticle> NewsArticles { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<SystemAccount> SystemAccounts { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AuditLog__EB5F6CBDB97ECFE0");

            entity.ToTable("AuditLog");

            entity.Property(e => e.Action).HasMaxLength(20);
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.EntityName).HasMaxLength(50);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.UserEmail).HasMaxLength(100);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0B09D68A80");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryDescription).HasMaxLength(500);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
        });

        modelBuilder.Entity<NewsArticle>(entity =>
        {
            entity.HasKey(e => e.NewsArticleId).HasName("PK__NewsArti__4CD0928C27333439");

            entity.ToTable("NewsArticle");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Headline).HasMaxLength(300);
            entity.Property(e => e.ImageUrl).HasMaxLength(300);
            entity.Property(e => e.NewsSource).HasMaxLength(200);
            entity.Property(e => e.NewsTitle).HasMaxLength(200);

            entity.HasOne(d => d.Category).WithMany(p => p.NewsArticles)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NewsArtic__Categ__2D27B809");

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.NewsArticleCreatedBies)
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NewsArtic__Creat__2E1BDC42");

            entity.HasOne(d => d.UpdatedBy).WithMany(p => p.NewsArticleUpdatedBies)
                .HasForeignKey(d => d.UpdatedById)
                .HasConstraintName("FK__NewsArtic__Updat__300424B4");

            entity.HasMany(d => d.Tags).WithMany(p => p.NewsArticles)
                .UsingEntity<Dictionary<string, object>>(
                    "NewsTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__NewsTag__TagId__33D4B598"),
                    l => l.HasOne<NewsArticle>().WithMany()
                        .HasForeignKey("NewsArticleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__NewsTag__NewsArt__32E0915F"),
                    j =>
                    {
                        j.HasKey("NewsArticleId", "TagId").HasName("PK__NewsTag__9A875D1647A3771D");
                        j.ToTable("NewsTag");
                    });
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__RefreshT__F5845E392A6B17F4");

            entity.ToTable("RefreshToken");

            entity.HasIndex(e => e.Token, "UQ__RefreshT__1EB4F81796B57A05").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Token).HasMaxLength(200);

            entity.HasOne(d => d.Account).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefreshTo__Accou__37A5467C");
        });

        modelBuilder.Entity<SystemAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__SystemAc__349DA5A6BD355C48");

            entity.ToTable("SystemAccount");

            entity.HasIndex(e => e.AccountEmail, "UQ__SystemAc__FC770D331BCA48F2").IsUnique();

            entity.Property(e => e.AccountEmail).HasMaxLength(100);
            entity.Property(e => e.AccountName).HasMaxLength(100);
            entity.Property(e => e.AccountPassword).HasMaxLength(255);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tag__657CF9AC3892F8D9");

            entity.ToTable("Tag");

            entity.HasIndex(e => e.TagName, "UQ__Tag__BDE0FD1D7AF2E771").IsUnique();

            entity.Property(e => e.Note).HasMaxLength(200);
            entity.Property(e => e.TagName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
