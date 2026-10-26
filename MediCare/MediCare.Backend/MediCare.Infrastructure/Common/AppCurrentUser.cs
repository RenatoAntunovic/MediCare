using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MediCare.Application.Abstractions;

namespace MediCare.Infrastructure.Common;

/// <summary>
/// Implementation of IAppCurrentUser that reads data from a JWT token.
/// </summary>
public sealed class AppCurrentUser(IHttpContextAccessor httpContextAccessor)
    : IAppCurrentUser
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public int? UserId =>
        int.TryParse(_user?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;

    public string? Email =>
        _user?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated =>
        _user?.Identity?.IsAuthenticated ?? false;

    // Roles come from the standard role claim issued by JwtTokenService
    public bool IsAdmin => _user?.IsInRole("Admin") ?? false;

    public bool IsManager => _user?.IsInRole("Manager") ?? false;

    public bool IsEmployee => _user?.IsInRole("User") ?? false;
}