using mark.davison.common.server.abstractions.Configuration;

namespace mark.davison.common.server.abstratctions.tests.Configuration;


public class GrandchildAppSettings : IAppSettings
{
    public string SECTION => "GRANDCHILD";
    [AppSettingSecret]
    public int GRANDCHILD_SECRET { get; set; } = 666;
}

public class ChildAppSettings : IAppSettings
{
    public string SECTION => "CHILD";
    public GrandchildAppSettings GRANDCHILD { get; set; } = new();
    public string ChildString { get; set; } = "child string value";
    [AppSettingSecret]
    public string ChildSecretString { get; set; } = "child secret string value";
}

public class RootAppSettings : IAppSettings
{
    public string SECTION => "ROOT";
    public ChildAppSettings CHILD { get; set; } = new();
    public int RootInt { get; set; } = 123;
}


[TestClass]
public class AppSettingsExtensionsTests
{
    [TestMethod]
    public void DumpAppSettings_DumpsChildSettingsCorrectly()
    {
        var settings = new RootAppSettings();

        var content = settings.DumpAppSettings(true);

        Assert.IsTrue(content.Contains(settings.SECTION));
        Assert.IsTrue(content.Contains(nameof(RootAppSettings.RootInt)));
        Assert.IsTrue(content.Contains(settings.RootInt.ToString()));

        Assert.IsTrue(content.Contains(settings.CHILD.SECTION));
        Assert.IsTrue(content.Contains(nameof(ChildAppSettings.ChildString)));
        Assert.IsTrue(content.Contains(settings.CHILD.ChildString.ToString()));

        Assert.IsTrue(content.Contains(settings.CHILD.GRANDCHILD.SECTION));
    }

    [TestMethod]
    public void DumpAppSettings_DoesNotExposeSecrets()
    {
        var settings = new RootAppSettings();

        var content = settings.DumpAppSettings(true);

        Assert.IsTrue(content.Contains(nameof(ChildAppSettings.ChildSecretString)));
        Assert.IsFalse(content.Contains(settings.CHILD.ChildSecretString.ToString()));

        Assert.IsTrue(content.Contains(nameof(GrandchildAppSettings.GRANDCHILD_SECRET)));
        Assert.IsFalse(content.Contains(settings.CHILD.GRANDCHILD.GRANDCHILD_SECRET.ToString()));

        Assert.IsTrue(content.Contains("*****"));
    }

    [TestMethod]
    public void DumpAppSettings_ExposesSecrets_WhenNotSafe()
    {
        var settings = new RootAppSettings();

        var content = settings.DumpAppSettings(false);

        Assert.IsTrue(content.Contains(nameof(ChildAppSettings.ChildSecretString)));
        Assert.IsTrue(content.Contains(settings.CHILD.ChildSecretString.ToString()));

        Assert.IsTrue(content.Contains(nameof(GrandchildAppSettings.GRANDCHILD_SECRET)));
        Assert.IsTrue(content.Contains(settings.CHILD.GRANDCHILD.GRANDCHILD_SECRET.ToString()));

        Assert.IsFalse(content.Contains("*****"));
    }
}
