using Microsoft.Extensions.Configuration;

namespace BackgroundProcessing.Core.Settings
{
    public class AzureSettingsManager : IAzureSettingsManager
    {
        private readonly IConfiguration _configuration;
        public AzureSettingsManager(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public string CosmosConnectionStrings { get => _configuration["CosmosConnectionStrings"]; }
        public string AzureServiceBusPrimaryConnectionString { get => _configuration["AzureServiceBusPrimaryConnectionString"]; }

        public IConfigurationSection GetConfigurationSection(string key)
        {
            return this._configuration.GetSection(key);
        }
    }
}
