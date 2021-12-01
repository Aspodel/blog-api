using BlogApi.Models;
using System.Threading.Tasks;

namespace BlogApi.Services
{
    public interface IEmailService
    {
        string GetTemplate(string type);
        Task Send(EmailModel emailModel);
    }
}
