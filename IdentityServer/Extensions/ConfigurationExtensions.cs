using IdentityServer.Models;
using Microsoft.Extensions.Configuration;

namespace IdentityServer.Extensions
{
    public static class ConfigurationExtensions
    {
        public static ProviderConfig GetProviderConfiguration(this IConfigurationSection section, string providerName)
        {
            var config = new ProviderConfig();
            if(!string.IsNullOrEmpty(providerName))
            {
                section = section.GetSection(providerName);
            }
            section.Bind(config);

            return config;
        }
    }
}
