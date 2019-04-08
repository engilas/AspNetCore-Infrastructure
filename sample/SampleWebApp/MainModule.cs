using Infrastructure.Extensions;
using Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SampleWebApp
{
    public class MainModule : IAspModule
    {
        public void Init(IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureAndValidate<Options>(configuration.GetSection("modules:main:configuration"));

            //Services initialization here
        }
    }
}
