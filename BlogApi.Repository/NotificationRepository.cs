using BlogApi.Contract;
using BlogApi.Core;
using BlogApi.Core.Database;

namespace BlogApi.Repository
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context) { }
    }
}