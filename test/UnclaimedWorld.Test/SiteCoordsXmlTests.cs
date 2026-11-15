using System.Xml.Serialization;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;

namespace UnclaimedWorld.Test;

/// <summary>
/// An exported scenario's world map showed none of its locations: GeodeticCoordinate was get-only,
/// so SiteData.Coords was written as an empty &lt;Coords /&gt; and read back as 0, 0 (Kastuk,
/// 2026-11-14, The Clay Pit's Tellus at 17.825, 70.68).
/// </summary>
[TestClass]
public sealed class SiteCoordsXmlTests
{
    [TestMethod]
    public void SiteCoordinatesSurviveTheXmlRoundTrip()
    {
        var site = new SiteData { KeyName = "otherSite1", Name = "Tellus", Coords = new GeodeticCoordinate(17.825d, 70.68d) };
        var serializer = new XmlSerializer(typeof(SiteData));
        var text = new StringWriter();
        serializer.Serialize(text, site);
        StringAssert.Contains(text.ToString(), "<Longitude>17.825</Longitude>");

        var back = (SiteData)serializer.Deserialize(new StringReader(text.ToString()))!;
        Assert.AreEqual(17.825d, back.Coords.Longitude);
        Assert.AreEqual(70.68d, back.Coords.Latitude);
    }
}
