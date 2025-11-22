using BetterBetterTeleporter.Adapters;
using BetterBetterTeleporter.Tests.Fakes;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Tests.Utility.ItemRules;

[TestClass]
public sealed class ItemNameRuleTest
{
    private const bool keep = true;
    private const bool drop = false;

    private FakePlayerInfo player = null!;
    private FakeItemInfo clipboard = null!;

    [TestInitialize]
    public void Setup()
    {
        clipboard = new FakeItemInfo { Name = "clipboard", DisplayName = "ClipboardManual", TypeId = nameof(ClipboardItem) };
        var inventory = new IItemInfo?[4];
        inventory[0] = clipboard;
        player = new FakePlayerInfo { Slots = inventory, CurrentSlotIndex = 0 };
    }

    [TestMethod]
    public void Given_Keep_When_ListEmpty_Then_ItemKept()
    {
        var rules = ItemParser.ParseConfig("");
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("key")]
    [DataRow("key,shovel")]
    public void Given_Keep_When_NotInDropList_Then_ItemKept(string configValue)
    {
        var rules = ItemParser.ParseConfig(configValue);
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("clipboard")]
    [DataRow("key,clipboard")]
    [DataRow("clipboard,key")]
    public void Given_Keep_When_InDropList_Then_ItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, keep, rules));
    }

    [TestMethod]
    public void Given_Drop_When_ListEmpty_Then_ItemDropped()
    {
        var rules = ItemParser.ParseConfig("");
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("key")]
    [DataRow("key,shovel")]
    public void Given_Drop_When_NotInKeepList_Then_ItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("clipboard")]
    [DataRow("key,clipboard")]
    [DataRow("clipboard,key")]
    public void Given_Drop_When_InKeepList_Then_ItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("CLIPBOARD")]
    [DataRow("KEY,CLIPBOARD")]
    [DataRow("CLIPBOARD,KEY")]
    public void Given_Keep_When_InAllCapsDropList_Then_ItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, keep, rules));
    }
}
