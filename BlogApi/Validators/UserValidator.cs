using BlogApi.Core.Entities;
using BlogApi.DTOs;
using BlogApi.DTOs.Create;
using BlogApi.Repository;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BlogApi.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserDTO>
    {
        public CreateUserValidator(UserManager userManager, RoleManager<Role> roleManager)
        {
            RuleFor(u => u.Username).MustAsync((username, _) => userManager.Users.AllAsync(u => u.NormalizedUserName != userManager.NormalizeName(username))).WithMessage("{PropertyName} has been taken");
            RuleFor(u => u.Email).MustAsync((email, _) => userManager.Users.AllAsync(u => u.Email != userManager.NormalizeEmail(email))).WithMessage("{PropertyName} has been taken");
            RuleFor(u => u.Roles).MustAsync(async (roles, _) =>
            {
                var systemRoles = await roleManager.Roles.ToListAsync();
                return !roles.Except(systemRoles.Select(r => r.Name)).Any();
            }).WithMessage("Some role does not exist");
        }
    }

    public class UpdateUserValidator : AbstractValidator<UserDTO>
    {
        public UpdateUserValidator(UserManager userManager, RoleManager<Role> roleManager)
        {
            RuleFor(u => u.Email).MustAsync(async (dto, email, _) => !await userManager.Users.AnyAsync(u => u.Email == userManager.NormalizeEmail(email) && u.Guid != dto.Guid)).WithMessage("{PropertyName} has been taken");
            RuleFor(u => u.Roles).MustAsync(async (roles, _) =>
            {
                var systemRoles = await roleManager.Roles.ToListAsync();
                return !roles.Except(systemRoles.Select(r => r.Name)).Any();
            }).WithMessage("Some role does not exist");
        }
    }
}
