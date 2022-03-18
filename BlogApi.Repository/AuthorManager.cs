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
    public class AuthorManager : UserManager<Author>
    {
        public AuthorManager(
            IUserStore<Author> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<Author> passwordHasher,
            IEnumerable<IUserValidator<Author>> userValidators,
            IEnumerable<IPasswordValidator<Author>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<Author>> logger
        ) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            RegisterTokenProvider(TokenOptions.DefaultProvider, new EmailTokenProvider<Author>());
            //RegisterTokenProvider(TokenOptions.DefaultProvider, new <Author>());
        }

        public async Task<Author?> FindByGuidAsync(string guid)
            => await Users.Where(u => u.Guid == guid).FirstOrDefaultAsync();

        public new async Task<Author?> FindByNameAsync(string userName)
        {
            var user = await base.FindByNameAsync(userName);
            if (user is null || user.IsDeleted)
                return null;

            return user;
        }

        public IQueryable<Author> FindAll(Expression<Func<Author, bool>>? predicate = null)
            => Users
                .Where(u => !u.IsDeleted)
                .WhereIf(predicate != null, predicate!);
    }
}
