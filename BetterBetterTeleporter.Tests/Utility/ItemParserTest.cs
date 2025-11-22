using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Tests.Utility;

[TestClass]
public sealed class ItemParserTest
{
    [TestMethod]
    public void Given_EmptyString_When_Parsed_Then_EmptyResult()
    {
        Assert.IsEmpty(ItemParser.ParseConfig(""));
    }

    [TestMethod]
    public void Given_DelimitedItemList_When_Parsed_Then_ReturnsCollection()
    {
        string[] expected = ["key", "shovel", "clipboard"];
        string[] actual = [.. ItemParser.ParseConfig("key,shovel,clipboard").Select(x => x.ToString())];
        Assert.HasCount(3, actual);
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Given_NestedSingleRule_When_Parsed_Then_SingleResult()
    {
        var actual = ItemParser.ParseConfig("[current:not(key,shovel,clipboard)]");
        Assert.HasCount(1, actual);
        Assert.AreEqual("current", actual[0].ToString());
    }

    [TestMethod]
    public void Given_ComplexList_When_Parsed_Then_ReturnsCollection()
    {
        var actual = ItemParser.ParseConfig("shovel,[current:not(key,clipboard)],airhorn");
        Assert.HasCount(3, actual);
        Assert.AreEqual("shovel", actual[0].ToString());
        Assert.AreEqual("current", actual[1].ToString());
        Assert.AreEqual("airhorn", actual[2].ToString());
    }

    [TestMethod]
    public void Given_EscapedDelimiter_When_Parsed_Then_SingleResult()
    {
        var actual = ItemParser.ParseConfig("key\\,shovel");
        Assert.HasCount(1, actual);
        Assert.AreEqual("key\\,shovel", actual[0].ToString());
    }

    [TestMethod]
    public void Given_EscapedEscapeChar_When_Parsed_Then_ReturnsCollection()
    {
        var actual = ItemParser.ParseConfig("key\\\\,shovel");
        Assert.HasCount(2, actual);
        Assert.AreEqual("key\\", actual[0].ToString());
        Assert.AreEqual("shovel", actual[1].ToString());
    }
}
