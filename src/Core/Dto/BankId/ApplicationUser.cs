using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace FSH.Infrastructure.Auth;

public class ApplicationUser : IdentityUser
{
    public string TenantId { get; set; }
    public string? PersonalNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; } = new DateTime(2001, 1, 1).ToUniversalTime();

    public string? ObjectId { get; set; }
}
