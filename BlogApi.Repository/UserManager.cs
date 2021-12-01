using BlogApi.Core.Entities;
using BlogApi.Repository.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlogApi.Repository
{
    public class UserManager : UserManager<User>
    {
        public UserManager(
            IUserStore<User> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher,
            IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<User>> logger
        ) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public async Task<User?> FindByGuidAsync(string guid)
            => await Users.Where(u => u.Guid == guid).FirstOrDefaultAsync();

        public new async Task<User?> FindByNameAsync(string userName)
        {
            var user = await base.FindByNameAsync(userName);
            if (user is null || user.IsDeleted)
                return null;

            return user;
        }

        public IQueryable<User> FindAll(Expression<Func<User, bool>>? predicate = null)
            => Users
                .Where(u => !u.IsDeleted)
                .WhereIf(predicate != null, predicate!);
    }
}
