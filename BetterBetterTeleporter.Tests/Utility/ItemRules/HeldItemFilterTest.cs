using BetterBetterTeleporter.Tests.Fakes;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Tests.Utility.ItemRules;

[TestClass]
public sealed class HeldItemFilterTest
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
    [DataRow("[held]")]
    [DataRow("key,[held]")]
    [DataRow("[held],key")]
    [DataRow("[held:not(shovel)]")]
    [DataRow("[held:not(shovel)],key")]
    [DataRow("key,[held:not(shovel)]")]
    public void Given_Keep_When_DropHeld_Then_HeldItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsHeld = true;
        Assert.IsTrue(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[held]")]
    [DataRow("key,[held]")]
    [DataRow("[held],key")]
    [DataRow("[held:not(shovel)]")]
    [DataRow("[held:not(shovel)],key")]
    [DataRow("key,[held:not(shovel)]")]
    public void Given_Keep_When_DropHeld_Then_PocketedItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsPocketed = true;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[held:not(clipboard)]")]
    [DataRow("key,[held:not(clipboard)]")]
    [DataRow("[held:not(clipboard)],key")]
    public void Given_Keep_When_DropHeldExceptItem_Then_HeldItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsHeld = true;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[held:not(clipboard)]")]
    [DataRow("key,[held:not(clipboard)]")]
    [DataRow("[held:not(clipboard)],key")]
    public void Given_Keep_When_DropHeldExceptItem_Then_PocketedItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsPocketed = true;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[held:not(key,clipboard)]")]
    [DataRow("[held:not(clipboard,key)]")]
    [DataRow("key,[held:not(key,clipboard)]")]
    [DataRow("key,[held:not(clipboard,key)]")]
    [DataRow("[held:not(key,clipboard)],key")]
    [DataRow("[held:not(clipboard,key)],key")]
    public void Given_Keep_When_DropHeldExceptListContainsItem_Then_HeldItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsHeld = true;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[held:not(key,clipboard)]")]
    [DataRow("[held:not(clipboard,key)]")]
    [DataRow("key,[held:not(key,clipboard)]")]
    [DataRow("key,[held:not(clipboard,key)]")]
    [DataRow("[held:not(key,clipboard)],key")]
    [DataRow("[held:not(clipboard,key)],key")]
    public void Given_Keep_When_DropHeldExceptListContainsItem_Then_PocketedItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsPocketed = true;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[held]")]
    [DataRow("key,[held]")]
    [DataRow("[held],key")]
    [DataRow("[held:not(shovel)]")]
    [DataRow("[held:not(shovel)],key")]
    [DataRow("key,[held:not(shovel)]")]
    public void Given_Drop_When_KeepHeld_Then_HeldItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsHeld = true;
        Assert.IsFalse(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[held]")]
    [DataRow("key,[held]")]
    [DataRow("[held],key")]
    [DataRow("[held:not(shovel)]")]
    [DataRow("[held:not(shovel)],key")]
    [DataRow("key,[held:not(shovel)]")]
    public void Given_Drop_When_KeepHeld_Then_PocketedItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsPocketed = true;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[held:not(clipboard)]")]
    [DataRow("key,[held:not(clipboard)]")]
    [DataRow("[held:not(clipboard)],key")]
    public void Given_Drop_When_KeepHeldExceptItem_Then_HeldItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsHeld = true;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[held:not(clipboard)]")]
    [DataRow("key,[held:not(clipboard)]")]
    [DataRow("[held:not(clipboard)],key")]
    public void Given_Drop_When_KeepHeldExceptItem_Then_PocketedItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsPocketed = true;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[held:not(key,clipboard)]")]
    [DataRow("[held:not(clipboard,key)]")]
    [DataRow("key,[held:not(key,clipboard)]")]
    [DataRow("key,[held:not(clipboard,key)]")]
    [DataRow("[held:not(key,clipboard)],key")]
    [DataRow("[held:not(clipboard,key)],key")]
    public void Given_Drop_When_KeepHeldExceptListContainsItem_Then_HeldItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsHeld = true;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[held:not(key,clipboard)]")]
    [DataRow("[held:not(clipboard,key)]")]
    [DataRow("key,[held:not(key,clipboard)]")]
    [DataRow("key,[held:not(clipboard,key)]")]
    [DataRow("[held:not(key,clipboard)],key")]
    [DataRow("[held:not(clipboard,key)],key")]
    public void Given_Drop_When_KeepHeldExceptListContainsItem_Then_PocketedItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        clipboard.IsPocketed = true;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }
}
