using Microsoft.Extensions.Configuration;

namespace Nuages.Lambda
{
    
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}
