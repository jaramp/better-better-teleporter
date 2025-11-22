using BetterBetterTeleporter.Adapters;
using BetterBetterTeleporter.Tests.Fakes;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Tests.Utility.ItemRules;

[TestClass]
public sealed class CurrentlyHeldFilterTest
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
    [DataRow("[current]")]
    [DataRow("key,[current]")]
    [DataRow("[current],key")]
    [DataRow("[current:not(shovel)]")]
    [DataRow("[current:not(shovel)],key")]
    [DataRow("key,[current:not(shovel)]")]
    public void Given_Keep_When_DropCurrentlyHeld_Then_CurrentlyHeldItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current]")]
    [DataRow("key,[current]")]
    [DataRow("[current],key")]
    [DataRow("[current:not(shovel)]")]
    [DataRow("[current:not(shovel)],key")]
    [DataRow("key,[current:not(shovel)]")]
    public void Given_Keep_When_DropCurrentlyHeld_Then_PocketedItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = emptyItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current:not(clipboard)]")]
    [DataRow("key,[current:not(clipboard)]")]
    [DataRow("[current:not(clipboard)],key")]
    public void Given_Keep_When_DropCurrentlyHeldExceptItem_Then_CurrentlyHeldItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current:not(clipboard)]")]
    [DataRow("key,[current:not(clipboard)]")]
    [DataRow("[current:not(clipboard)],key")]
    public void Given_Keep_When_DropCurrentlyHeldExceptItem_Then_PocketedItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = emptyItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current:not(key,clipboard)]")]
    [DataRow("[current:not(clipboard,key)]")]
    [DataRow("key,[current:not(key,clipboard)]")]
    [DataRow("key,[current:not(clipboard,key)]")]
    [DataRow("[current:not(key,clipboard)],key")]
    [DataRow("[current:not(clipboard,key)],key")]
    public void Given_Keep_When_DropCurrentlyHeldExceptListContainsItem_Then_CurrentlyHeldItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current:not(key,clipboard)]")]
    [DataRow("[current:not(clipboard,key)]")]
    [DataRow("key,[current:not(key,clipboard)]")]
    [DataRow("key,[current:not(clipboard,key)]")]
    [DataRow("[current:not(key,clipboard)],key")]
    [DataRow("[current:not(clipboard,key)],key")]
    public void Given_Keep_When_DropCurrentlyHeldExceptListContainsItem_Then_PocketedItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = emptyItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current]")]
    [DataRow("key,[current]")]
    [DataRow("[current],key")]
    [DataRow("[current:not(shovel)]")]
    [DataRow("[current:not(shovel)],key")]
    [DataRow("key,[current:not(shovel)]")]
    public void Given_Drop_When_KeepCurrentlyHeld_Then_CurrentlyHeldItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsFalse(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[current]")]
    [DataRow("key,[current]")]
    [DataRow("[current],key")]
    [DataRow("[current:not(shovel)]")]
    [DataRow("[current:not(shovel)],key")]
    [DataRow("key,[current:not(shovel)]")]
    public void Given_Drop_When_KeepCurrentlyHeld_Then_PocketedItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = emptyItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[current:not(clipboard)]")]
    [DataRow("key,[current:not(clipboard)]")]
    [DataRow("[current:not(clipboard)],key")]
    public void Given_Drop_When_KeepCurrentlyHeldExceptItem_Then_CurrentlyHeldItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[current:not(clipboard)]")]
    [DataRow("key,[current:not(clipboard)]")]
    [DataRow("[current:not(clipboard)],key")]
    public void Given_Drop_When_KeepCurrentlyHeldExceptItem_Then_PocketedItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = emptyItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[current:not(key,clipboard)]")]
    [DataRow("[current:not(clipboard,key)]")]
    [DataRow("key,[current:not(key,clipboard)]")]
    [DataRow("key,[current:not(clipboard,key)]")]
    [DataRow("[current:not(key,clipboard)],key")]
    [DataRow("[current:not(clipboard,key)],key")]
    public void Given_Drop_When_KeepCurrentlyHeldExceptListContainsItem_Then_CurrentlyHeldItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[current:not(key,clipboard)]")]
    [DataRow("[current:not(clipboard,key)]")]
    [DataRow("key,[current:not(key,clipboard)]")]
    [DataRow("key,[current:not(clipboard,key)]")]
    [DataRow("[current:not(key,clipboard)],key")]
    [DataRow("[current:not(clipboard,key)],key")]
    public void Given_Drop_When_KeepCurrentlyHeldExceptListContainsItem_Then_PocketedItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = emptyItemSlot;
        Assert.IsTrue(player.ShouldDropItem(clipboard, drop, rules));
    }
}
