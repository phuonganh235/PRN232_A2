using System;
using System.Collections.Generic;

namespace FUNewsManagement_CoreAPI.Models;

public partial class AuditLog
{
    public int AuditLogId { get; set; }

    public string UserEmail { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public string? EntityId { get; set; }

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }

    public DateTime Timestamp { get; set; }
}
