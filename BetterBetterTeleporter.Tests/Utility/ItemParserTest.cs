using BetterBetterTeleporter.Adapters;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Tests.Utility;

public sealed class FakeItemInfo : IItemInfo
{
    public string Name { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string TypeId { get; init; } = "";
}

public sealed class FakePlayerItemContext : IPlayerInfo
{
    public IReadOnlyList<IItemInfo?> Slots { get; init; } = [];
    public int CurrentSlotIndex { get; set; }
}


[TestClass]
public sealed class ItemParserTest
{
    private const bool keep = true;
    private const bool drop = false;

    private FakePlayerItemContext player = null!;
    private FakeItemInfo clipboard = null!;
    private const int clipboardItemSlot = 0;
    private const int emptyItemSlot = 1;

    [TestInitialize]
    public void Setup()
    {
        clipboard = new FakeItemInfo { Name = "clipboard", DisplayName = "ClipboardManual", TypeId = nameof(ClipboardItem) };

        var inventory = new IItemInfo?[4];
        inventory[clipboardItemSlot] = clipboard;

        player = new FakePlayerItemContext
        {
            Slots = inventory,
            CurrentSlotIndex = clipboardItemSlot
        };
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

    #region Category

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
    public void Given_Keep_When_SelfNegatedCategory_Then_ItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, rules));
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
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[CURRENT]")]
    [DataRow("KEY,[CURRENT]")]
    [DataRow("[CURRENT],KEY")]
    public void Given_Keep_When_InAllCapsCategory_Then_ItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current:not(key,shovel,[current:not(clipboard)],boombox)],bell")]
    [DataRow("[current:not(key,shovel,[current:not(airhorn,clipboard,bigbolt)],boombox)],bell")]
    public void Given_Keep_When_InDropListExtremeNesting_Then_ItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current:not(key,shovel,clipboard,[current:not(airhorn,bigbolt)],boombox)],bell")]
    [DataRow("[current:not(key,shovel,[current:not(airhorn,bigbolt)],clipboard,boombox)],bell")]
    public void Given_Keep_When_NegatedDropListExtremeNesting_Then_ItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, rules));
    }

    #region Category: [current]
    #region Category: [current]: Default Keep

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
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, keep, rules));
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
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current:not(clipboard)]")]
    [DataRow("key,[current:not(clipboard)]")]
    [DataRow("[current:not(clipboard)],key")]
    public void Given_Keep_When_DropCurrentlyHeldExceptItem_Then_CurrentlyHeldItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, rules));
    }

    [TestMethod]
    [DataRow("[current:not(clipboard)]")]
    [DataRow("key,[current:not(clipboard)]")]
    [DataRow("[current:not(clipboard)],key")]
    public void Given_Keep_When_DropCurrentlyHeldExceptItem_Then_PocketedItemKept(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = emptyItemSlot;
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, rules));
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
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, rules));
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
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, rules));
    }

    #endregion Category: [current]: Default Keep
    #region Category: [current]: Default Drop

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
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, drop, rules));
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
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[current:not(clipboard)]")]
    [DataRow("key,[current:not(clipboard)]")]
    [DataRow("[current:not(clipboard)],key")]
    public void Given_Drop_When_KeepCurrentlyHeldExceptItem_Then_CurrentlyHeldItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = clipboardItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, rules));
    }

    [TestMethod]
    [DataRow("[current:not(clipboard)]")]
    [DataRow("key,[current:not(clipboard)]")]
    [DataRow("[current:not(clipboard)],key")]
    public void Given_Drop_When_KeepCurrentlyHeldExceptItem_Then_PocketedItemDropped(string except)
    {
        var rules = ItemParser.ParseConfig(except);
        player.CurrentSlotIndex = emptyItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, rules));
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
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, rules));
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
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, rules));
    }

    #endregion Category: [current]: Default Drop
    #endregion Category: [current]

    #endregion Category
}
