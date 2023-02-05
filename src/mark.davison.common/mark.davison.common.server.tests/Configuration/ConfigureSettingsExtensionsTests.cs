using mark.davison.common.server.abstractions.Configuration;
using mark.davison.common.server.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace mark.davison.common.server.tests.Configuration;

[TestClass]
public class ConfigureSettingsExtensionsTests
{
    private const string RootValue = "ROOT_VALUE";
    private const string NestedValue = "NESTED_VALUE";
    private class NestedAppSettings : IAppSettings
    {
        public string SECTION => "NESTED";
        public string NestedValue { get; set; } = string.Empty;
    }

    private class RootAppSettings : IAppSettings
    {
        public string SECTION => "ROOT";
        public string RootValue { get; set; } = string.Empty;
        public NestedAppSettings NESTED { get; set; } = new();
    }

    private readonly IDictionary<string, string?> _config = new Dictionary<string, string?>
    {
        { "ROOT:RootValue", RootValue },
        { "ROOT:NESTED:NestedValue", NestedValue },
    };

    private IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder().AddInMemoryCollection(_config!).Build();
    }

    [TestMethod]
    public void ConfigureSettingsServices_ReturnsCorrectAppSettings()
    {
        var configuration = GetConfiguration();
        var services = new ServiceCollection();
        var appSettings = services.ConfigureSettingsServices<RootAppSettings>(configuration);

        Assert.AreEqual(RootValue, appSettings.RootValue);
        Assert.AreEqual(NestedValue, appSettings.NESTED.NestedValue);
    }

    [TestMethod]
    public void ConfigureSettingsServices_RegistersAllIOptions()
    {
        var configuration = GetConfiguration();
        var services = new ServiceCollection();
        services.ConfigureSettingsServices<RootAppSettings>(configuration);

        var provider = services.BuildServiceProvider();

        var rootAppSettings = provider.GetService<IOptions<RootAppSettings>>();
        var nestedAppSettings = provider.GetService<IOptions<NestedAppSettings>>();

        Assert.IsNotNull(rootAppSettings);
        Assert.IsNotNull(nestedAppSettings);

        Assert.AreEqual(RootValue, rootAppSettings.Value.RootValue);
        Assert.AreEqual(NestedValue, nestedAppSettings.Value.NestedValue);
    }
}
