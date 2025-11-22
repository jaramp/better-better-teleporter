using BetterBetterTeleporter.Adapters;
using BetterBetterTeleporter.Tests.Fakes;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Tests.Utility.ItemRules;

[TestClass]
public sealed class ItemFilterTest
{
    private const bool drop = true;
    private const bool keep = false;

    private FakePlayerInfo player = null!;
    private FakeItemInfo clipboard = null!;
    private const int clipboardItemSlot = 0;

    [TestInitialize]
    public void Setup()
    {
        clipboard = new FakeItemInfo { Name = "clipboard", DisplayName = "ClipboardManual", TypeId = nameof(ClipboardItem) };
        var inventory = new IItemInfo?[4];
        inventory[clipboardItemSlot] = clipboard;
        player = new FakePlayerInfo { Slots = inventory, CurrentSlotIndex = clipboardItemSlot };
    }

    [TestMethod]
    [DataRow("[held:not([held])]")]
    [DataRow("[held:not([held],key)]")]
    [DataRow("[held:not(key,[held])]")]
    [DataRow("[held:not(clipboard,[held])]")]
    [DataRow("[held:not([held],clipboard)]")]
    [DataRow("key,[held:not([held])]")]
    [DataRow("key,[held:not([held],key)]")]
    [DataRow("key,[held:not(key,[held])]")]
    [DataRow("key,[held:not(clipboard,[held])]")]
    [DataRow("key,[held:not([held],clipboard)]")]
    [DataRow("[held:not([held])],key")]
    [DataRow("[held:not([held],key)],key")]
    [DataRow("[held:not(key,[held])],key")]
    [DataRow("[held:not(clipboard,[held])],key")]
    [DataRow("[held:not([held],clipboard)],key")]
    public void Given_Keep_When_SelfNegatedCategory_Then_ItemNotDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[held:not([held])]")]
    [DataRow("[held:not([held],key)]")]
    [DataRow("[held:not(key,[held])]")]
    [DataRow("[held:not(clipboard,[held])]")]
    [DataRow("[held:not([held],clipboard)]")]
    [DataRow("key,[held:not([held])]")]
    [DataRow("key,[held:not([held],key)]")]
    [DataRow("key,[held:not(key,[held])]")]
    [DataRow("key,[held:not(clipboard,[held])]")]
    [DataRow("key,[held:not([held],clipboard)]")]
    [DataRow("[held:not([held])],key")]
    [DataRow("[held:not([held],key)],key")]
    [DataRow("[held:not(key,[held])],key")]
    [DataRow("[held:not(clipboard,[held])],key")]
    [DataRow("[held:not([held],clipboard)],key")]
    public void Given_Drop_When_SelfNegatedCategory_Then_ItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[held]")]
    [DataRow("KEY,[held]")]
    [DataRow("[held],KEY")]
    public void Given_Keep_When_InAllCapsCategory_Then_ItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[held:not(key,shovel,[held:not(clipboard)],boombox)],bell")]
    [DataRow("[held:not(key,shovel,[held:not(airhorn,clipboard,bigbolt)],boombox)],bell")]
    public void Given_Keep_When_InDropListExtremeNesting_Then_ItemNotDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[held:not(key,shovel,clipboard,[held:not(airhorn,bigbolt)],boombox)],bell")]
    [DataRow("[held:not(key,shovel,[held:not(airhorn,bigbolt)],clipboard,boombox)],bell")]
    public void Given_Keep_When_NegatedDropListExtremeNesting_Then_ItemNotDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }
}
