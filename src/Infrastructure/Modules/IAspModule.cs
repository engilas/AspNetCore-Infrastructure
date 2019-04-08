using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Modules
{
    /// <summary>
    ///     Installer interface for main installer class in module.
    ///     Implement this interface to auto-register module by reflection.
    /// </summary>
    public interface IAspModule
    {
        void Init(IServiceCollection services, IConfiguration configuration);
    }
}