using System;
using System.Collections.Generic;

namespace LogisticsSystem.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int? RoleId { get; set; }

    public virtual Courier? Courier { get; set; }

    public virtual Role? Role { get; set; }
}
