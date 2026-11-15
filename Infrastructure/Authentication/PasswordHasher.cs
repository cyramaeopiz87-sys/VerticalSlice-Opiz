namespace Infrastructure.Authentication;

using Application.Abstractions;
using IdentityPasswordHasher = Microsoft.AspNetCore.Identity.PasswordHasher<object>;
using Microsoft.AspNetCore.Identity;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly IdentityPasswordHasher _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(null!, password);

    public bool Verify(string password, string hash)
        => _hasher.VerifyHashedPassword(null!, hash, password)
           is PasswordVerificationResult.Success
           or PasswordVerificationResult.SuccessRehashNeeded;
}