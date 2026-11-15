namespace Application.Features.Auth.Register;

using Application.Abstractions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

public sealed class RegisterCommandHandler(
    IAppDbContext dbContext,
    IPasswordHasher passwordHasher)
    : ICommandHandler<RegisterCommand, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> HandleAsync(
        RegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        var existingUser = await dbContext.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (existingUser)
        {
            return Result<RegisterResponse>.Failure(
                Error.Conflict(
                    "Auth.EmailAlreadyExists",
                    "An account with this email already exists."));
        }

        var role = UserRole.Student;

        if (!string.IsNullOrWhiteSpace(command.Role))
        {
            if (!Enum.TryParse<UserRole>(
                command.Role,
                true,
                out role))
            {
                return Result<RegisterResponse>.Failure(
                    Error.Validation(
                        "Auth.InvalidRole",
                        "The specified role is invalid."));
            }
        }

        var student = await dbContext.Students
            .FirstOrDefaultAsync(
                s => s.Email == email,
                cancellationToken);

        var user = new ApplicationUser
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(command.Password),
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            Role = role,
            StudentId = student?.Id,
            IsActive = true
        };

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new RegisterResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.StudentId);

        return Result<RegisterResponse>.Success(response);
    }
}