namespace mark.davison.common.tests.Utility;

[TestClass]
public class GuidUtilitiesTests
{
    [TestMethod]
    public void CombineTwoGuids_ReturnsConsistantly()
    {
        Guid guid1 = Guid.NewGuid();
        Guid guid2 = Guid.NewGuid();

        Assert.AreEqual(GuidUtilities.CombineTwoGuids(guid1, guid2), GuidUtilities.CombineTwoGuids(guid1, guid2));
    }

    [TestMethod]
    public void CombineTwoGuids_ReturnsDifferentToEitherInput()
    {
        Guid guid1 = Guid.NewGuid();
        Guid guid2 = Guid.NewGuid();

        var combined = GuidUtilities.CombineTwoGuids(guid1, guid2);

        Assert.AreNotEqual(guid1, combined);
        Assert.AreNotEqual(guid2, combined);
    }
}
