using System.Threading.Tasks;

namespace Infrastructure.Startup
{
    public interface IInitService
    {
        Task Init();
    }
}
