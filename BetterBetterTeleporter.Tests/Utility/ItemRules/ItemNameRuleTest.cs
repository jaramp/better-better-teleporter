using BetterBetterTeleporter.Tests.Fakes;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Tests.Utility.ItemRules;

[TestClass]
public sealed class ItemNameRuleTest
{
    private const bool drop = true;
    private const bool keep = false;

    private FakePlayerInfo player = null!;
    private FakeClipboardItemInfo clipboard = null!;

    [TestInitialize]
    public void Setup()
    {
        clipboard = new FakeClipboardItemInfo();
        player = new FakePlayerInfo(clipboard);
    }

    [TestMethod]
    public void Given_Keep_When_ListEmpty_Then_ItemNotDropped()
    {
        var rules = ItemParser.ParseConfig("");
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("key")]
    [DataRow("key,shovel")]
    public void Given_Keep_When_NotInDropList_Then_ItemNotDropped(string configValue)
    {
        var rules = ItemParser.ParseConfig(configValue);
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("clipboard")]
    [DataRow("key,clipboard")]
    [DataRow("clipboard,key")]
    public void Given_Keep_When_InDropList_Then_ItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        Assert.IsTrue(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    public void Given_Drop_When_ListEmpty_Then_ItemDropped()
    {
        var rules = ItemParser.ParseConfig("");
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("key")]
    [DataRow("key,shovel")]
    public void Given_Drop_When_NotInKeepList_Then_ItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("clipboard")]
    [DataRow("key,clipboard")]
    [DataRow("clipboard,key")]
    public void Given_Drop_When_InKeepList_Then_ItemNotDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        Assert.IsFalse(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("CLIPBOARD")]
    [DataRow("KEY,CLIPBOARD")]
    [DataRow("CLIPBOARD,KEY")]
    public void Given_Keep_When_InAllCapsDropList_Then_ItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        Assert.IsTrue(player.ShouldDropItem(clipboard, keep, rules));
    }
}
