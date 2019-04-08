using Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Features
{
    public interface IFeature
    {
        void Initialize(IServiceCollection services, ModuleCollection moduleCollection);
        void Activate(IApplicationBuilder app);
    }
}