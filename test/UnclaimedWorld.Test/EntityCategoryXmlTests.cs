using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UnclaimedWorld.Test;

/// <summary>
/// TUTORIAL - Castaways exported with `dataexport --scenarios` and played as a user scenario
/// crashed when the stock panel opened (Kastuk, 2026-11-15): ArgumentNullException in
/// Grid.TryGetEntry from InventoryPanel.PopulateWithCategories. Its own items (item:lines, the
/// catamaran parts) set EntityType.Category, which is [XmlIgnore], and never CategoryKey - so the
/// export wrote no category for them and they loaded back with Category == null.
/// </summary>
[TestClass]
public sealed class EntityCategoryXmlTests
{
    [TestMethod]
    public void ACategorySetOnlyAsAnObjectIsWrittenAsItsKey()
    {
        var type = new EntityType("item:lines")
        {
            Category = new EntityCategory { KeyName = "rawMaterials", Name = "Raw materials" },
        };
        var serializer = new XmlSerializer(typeof(EntityType));
        var text = new StringWriter();
        serializer.Serialize(text, type);
        StringAssert.Contains(text.ToString(), "<CategoryKey>rawMaterials</CategoryKey>");

        var back = (EntityType)serializer.Deserialize(new StringReader(text.ToString()))!;
        Assert.AreEqual("rawMaterials", back.CategoryKey);
    }

    [TestMethod]
    public void AnExplicitCategoryKeyStillWins()
    {
        var type = new EntityType("item:x")
        {
            CategoryKey = "weapons",
            Category = new EntityCategory { KeyName = "tools" },
        };
        Assert.AreEqual("weapons", type.CategoryKey);
        Assert.IsNull(new EntityType("item:y").CategoryKey);
    }
}
