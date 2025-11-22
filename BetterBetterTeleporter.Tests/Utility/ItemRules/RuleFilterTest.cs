using BetterBetterTeleporter.Adapters;
using BetterBetterTeleporter.Tests.Fakes;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Tests.Utility.ItemRules;

[TestClass]
public sealed class RuleFilterTest
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
    [DataRow("[current:not([current])]")]
    [DataRow("[current:not([current],key)]")]
    [DataRow("[current:not(key,[current])]")]
    [DataRow("[current:not(clipboard,[current])]")]
    [DataRow("[current:not([current],clipboard)]")]
    [DataRow("key,[current:not([current])]")]
    [DataRow("key,[current:not([current],key)]")]
    [DataRow("key,[current:not(key,[current])]")]
    [DataRow("key,[current:not(clipboard,[current])]")]
    [DataRow("key,[current:not([current],clipboard)]")]
    [DataRow("[current:not([current])],key")]
    [DataRow("[current:not([current],key)],key")]
    [DataRow("[current:not(key,[current])],key")]
    [DataRow("[current:not(clipboard,[current])],key")]
    [DataRow("[current:not([current],clipboard)],key")]
    public void Given_Keep_When_SelfNegatedCategory_Then_ItemNotDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current:not([current])]")]
    [DataRow("[current:not([current],key)]")]
    [DataRow("[current:not(key,[current])]")]
    [DataRow("[current:not(clipboard,[current])]")]
    [DataRow("[current:not([current],clipboard)]")]
    [DataRow("key,[current:not([current])]")]
    [DataRow("key,[current:not([current],key)]")]
    [DataRow("key,[current:not(key,[current])]")]
    [DataRow("key,[current:not(clipboard,[current])]")]
    [DataRow("key,[current:not([current],clipboard)]")]
    [DataRow("[current:not([current])],key")]
    [DataRow("[current:not([current],key)],key")]
    [DataRow("[current:not(key,[current])],key")]
    [DataRow("[current:not(clipboard,[current])],key")]
    [DataRow("[current:not([current],clipboard)],key")]
    public void Given_Drop_When_SelfNegatedCategory_Then_ItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[CURRENT]")]
    [DataRow("KEY,[CURRENT]")]
    [DataRow("[CURRENT],KEY")]
    public void Given_Keep_When_InAllCapsCategory_Then_ItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current:not(key,shovel,[current:not(clipboard)],boombox)],bell")]
    [DataRow("[current:not(key,shovel,[current:not(airhorn,clipboard,bigbolt)],boombox)],bell")]
    public void Given_Keep_When_InDropListExtremeNesting_Then_ItemNotDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current:not(key,shovel,clipboard,[current:not(airhorn,bigbolt)],boombox)],bell")]
    [DataRow("[current:not(key,shovel,[current:not(airhorn,bigbolt)],clipboard,boombox)],bell")]
    public void Given_Keep_When_NegatedDropListExtremeNesting_Then_ItemNotDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }
}
