using BetterBetterTeleporter.Adapters;
using BetterBetterTeleporter.Tests.Fakes;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Tests.Utility.ItemRules;

[TestClass]
public sealed class HeldItemFilterTest
{
    private const bool drop = true;
    private const bool keep = false;

    private FakePlayerInfo player = null!;
    private FakeItemInfo clipboard = null!;
    private const int clipboardItemSlot = 0;
    private const int emptyItemSlot = 1;

    [TestInitialize]
    public void Setup()
    {
        clipboard = new FakeItemInfo { Name = "clipboard", DisplayName = "ClipboardManual", TypeId = nameof(ClipboardItem) };
        var inventory = new IItemInfo?[4];
        inventory[clipboardItemSlot] = clipboard;
        player = new FakePlayerInfo { Slots = inventory, CurrentSlotIndex = clipboardItemSlot };
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
        player.CurrentSlotIndex = clipboardItemSlot;
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
        player.CurrentSlotIndex = emptyItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[held:not(clipboard)]")]
    [DataRow("key,[held:not(clipboard)]")]
    [DataRow("[held:not(clipboard)],key")]
    public void Given_Keep_When_DropHeldExceptItem_Then_HeldItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[held:not(clipboard)]")]
    [DataRow("key,[held:not(clipboard)]")]
    [DataRow("[held:not(clipboard)],key")]
    public void Given_Keep_When_DropHeldExceptItem_Then_PocketedItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = emptyItemSlot;
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
        player.CurrentSlotIndex = clipboardItemSlot;
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
        player.CurrentSlotIndex = emptyItemSlot;
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
        player.CurrentSlotIndex = clipboardItemSlot;
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
        player.CurrentSlotIndex = emptyItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[held:not(clipboard)]")]
    [DataRow("key,[held:not(clipboard)]")]
    [DataRow("[held:not(clipboard)],key")]
    public void Given_Drop_When_KeepHeldExceptItem_Then_HeldItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[held:not(clipboard)]")]
    [DataRow("key,[held:not(clipboard)]")]
    [DataRow("[held:not(clipboard)],key")]
    public void Given_Drop_When_KeepHeldExceptItem_Then_PocketedItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = emptyItemSlot;
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
        player.CurrentSlotIndex = clipboardItemSlot;
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
        player.CurrentSlotIndex = emptyItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }
}
