using Microsoft.Extensions.Configuration;

namespace BackgroundProcessing.Core.Settings
{
    public interface IAzureSettingsManager
    {
        string CosmosConnectionStrings { get; }

        string AzureServiceBusPrimaryConnectionString { get; }

        IConfigurationSection GetConfigurationSection(string key);
    }
}
