using BetterBetterTeleporter.Adapters;
using BetterBetterTeleporter.Utility;
using Moq;

namespace BetterBetterTeleporter.Tests.Utility;

[TestClass]
public sealed class ItemParserTest
{
    private const bool keep = true;
    private const bool drop = false;
    private IPlayerControllerB player = null!;
    private IGrabbableObject clipboard = null!;
    private const int clipboardItemSlot = 0;
    private const int emptyItemSlot = 1;
    private int currentItemSlot = 0;

    [TestInitialize]
    public void Setup()
    {
        var playerMock = new Mock<IPlayerControllerB>();
        player = playerMock.Object;
        var clipboardMock = new Mock<IGrabbableObject>();
        var itemPropertiesMock = new Mock<IItem>();
        itemPropertiesMock.Setup(p => p.name).Returns("clipboard");
        itemPropertiesMock.Setup(p => p.itemName).Returns("ClipboardManual");
        clipboardMock.Setup(o => o.itemProperties).Returns(itemPropertiesMock.Object);
        clipboard = clipboardMock.Object;
        var inventory = new IGrabbableObject[4];
        inventory[0] = clipboard;
        playerMock.Setup(p => p.ItemSlots).Returns(inventory);
        playerMock.Setup(p => p.currentItemSlot).Returns(() => currentItemSlot);
    }

    [TestMethod]
    public void Given_Keep_When_ListEmpty_Then_ItemKept()
    {
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, []));
    }

    [TestMethod]
    [DataRow(["key"])]
    [DataRow(["key", "shovel"])]
    public void Given_Keep_When_NotInDropList_Then_ItemKept(string[] except)
    {
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, except));
    }

    [TestMethod]
    [DataRow(["clipboard"])]
    [DataRow(["key", "clipboard"])]
    [DataRow(["clipboard", "key"])]
    public void Given_Keep_When_InDropList_Then_ItemDropped(string[] except)
    {
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, keep, except));
    }

    [TestMethod]
    public void Given_Drop_When_ListEmpty_Then_ItemDropped()
    {
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, []));
    }

    [TestMethod]
    [DataRow(["key"])]
    [DataRow(["key", "shovel"])]
    public void Given_Drop_When_NotInKeepList_Then_ItemDropped(string[] except)
    {
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, except));
    }

    [TestMethod]
    [DataRow(["clipboard"])]
    [DataRow(["key", "clipboard"])]
    [DataRow(["clipboard", "key"])]
    public void Given_Drop_When_InKeepList_Then_ItemKept(string[] except)
    {
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, drop, except));
    }

    [TestMethod]
    [DataRow(["CLIPBOARD"])]
    [DataRow(["KEY", "CLIPBOARD"])]
    [DataRow(["CLIPBOARD", "KEY"])]
    public void Given_Keep_When_InAllCapsDropList_Then_ItemDropped(string[] except)
    {
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, keep, except));
    }

    #region Category

    [TestMethod]
    [DataRow(["[current:not([current])]"])]
    [DataRow(["[current:not([current],key)]"])]
    [DataRow(["[current:not(key,[current])]"])]
    [DataRow(["[current:not(clipboard,[current])]"])]
    [DataRow(["[current:not([current],clipboard)]"])]
    [DataRow(["key", "[current:not([current])]"])]
    [DataRow(["key", "[current:not([current],key)]"])]
    [DataRow(["key", "[current:not(key,[current])]"])]
    [DataRow(["key", "[current:not(clipboard,[current])]"])]
    [DataRow(["key", "[current:not([current],clipboard)]"])]
    [DataRow(["[current:not([current])]", "key"])]
    [DataRow(["[current:not([current],key)]", "key"])]
    [DataRow(["[current:not(key,[current])]", "key"])]
    [DataRow(["[current:not(clipboard,[current])]", "key"])]
    [DataRow(["[current:not([current],clipboard)]", "key"])]
    public void Given_Keep_When_SelfNegatedCategory_Then_ItemKept(string[] except)
    {
        currentItemSlot = clipboardItemSlot;
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, except));
    }
    
    [TestMethod]
    [DataRow(["[current:not([current])]"])]
    [DataRow(["[current:not([current],key)]"])]
    [DataRow(["[current:not(key,[current])]"])]
    [DataRow(["[current:not(clipboard,[current])]"])]
    [DataRow(["[current:not([current],clipboard)]"])]
    [DataRow(["key", "[current:not([current])]"])]
    [DataRow(["key", "[current:not([current],key)]"])]
    [DataRow(["key", "[current:not(key,[current])]"])]
    [DataRow(["key", "[current:not(clipboard,[current])]"])]
    [DataRow(["key", "[current:not([current],clipboard)]"])]
    [DataRow(["[current:not([current])]", "key"])]
    [DataRow(["[current:not([current],key)]", "key"])]
    [DataRow(["[current:not(key,[current])]", "key"])]
    [DataRow(["[current:not(clipboard,[current])]", "key"])]
    [DataRow(["[current:not([current],clipboard)]", "key"])]
    public void Given_Drop_When_SelfNegatedCategory_Then_ItemDropped(string[] except)
    {
        currentItemSlot = clipboardItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, except));
    }

    [TestMethod]
    [DataRow(["[CURRENT]"])]
    [DataRow(["KEY", "[CURRENT]"])]
    [DataRow(["[CURRENT]", "KEY"])]
    public void Given_Keep_When_InAllCapsCategory_Then_ItemDropped(string[] except)
    {
        currentItemSlot = clipboardItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, keep, except));
    }

    #region Category: [current]
    #region Category: [current]: Default Keep

    [TestMethod]
    [DataRow(["[current]"])]
    [DataRow(["key", "[current]"])]
    [DataRow(["[current]", "key"])]
    [DataRow(["[current:not(shovel)]"])]
    [DataRow(["[current:not(shovel)]", "key"])]
    [DataRow(["key", "[current:not(shovel)]"])]
    public void Given_Keep_When_DropCurrentlyHeld_Then_CurrentlyHeldItemDropped(string[] except)
    {
        currentItemSlot = clipboardItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, keep, except));
    }

    [TestMethod]
    [DataRow(["[current]"])]
    [DataRow(["key", "[current]"])]
    [DataRow(["[current]", "key"])]
    [DataRow(["[current:not(shovel)]"])]
    [DataRow(["[current:not(shovel)]", "key"])]
    [DataRow(["key", "[current:not(shovel)]"])]
    public void Given_Keep_When_DropCurrentlyHeld_Then_PocketedItemKept(string[] except)
    {
        currentItemSlot = emptyItemSlot;
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, except));
    }

    [TestMethod]
    [DataRow(["[current:not(clipboard)]"])]
    [DataRow(["key", "[current:not(clipboard)]"])]
    [DataRow(["[current:not(clipboard)]", "key"])]
    public void Given_Keep_When_DropCurrentlyHeldExceptItem_Then_CurrentlyHeldItemKept(string[] except)
    {
        currentItemSlot = clipboardItemSlot;
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, except));
    }

    [TestMethod]
    [DataRow(["[current:not(clipboard)]"])]
    [DataRow(["key", "[current:not(clipboard)]"])]
    [DataRow(["[current:not(clipboard)]", "key"])]
    public void Given_Keep_When_DropCurrentlyHeldExceptItem_Then_PocketedItemKept(string[] except)
    {
        currentItemSlot = emptyItemSlot;
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, except));
    }

    [TestMethod]
    [DataRow(["[current:not(key,clipboard)]"])]
    [DataRow(["[current:not(clipboard,key)]"])]
    [DataRow(["key", "[current:not(key,clipboard)]"])]
    [DataRow(["key", "[current:not(clipboard,key)]"])]
    [DataRow(["[current:not(key,clipboard)]", "key"])]
    [DataRow(["[current:not(clipboard,key)]", "key"])]
    public void Given_Keep_When_DropCurrentlyHeldExceptListContainsItem_Then_CurrentlyHeldItemKept(string[] except)
    {
        currentItemSlot = clipboardItemSlot;
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, except));
    }

    [TestMethod]
    [DataRow(["[current:not(key,clipboard)]"])]
    [DataRow(["[current:not(clipboard,key)]"])]
    [DataRow(["key", "[current:not(key,clipboard)]"])]
    [DataRow(["key", "[current:not(clipboard,key)]"])]
    [DataRow(["[current:not(key,clipboard)]", "key"])]
    [DataRow(["[current:not(clipboard,key)]", "key"])]
    public void Given_Keep_When_DropCurrentlyHeldExceptListContainsItem_Then_PocketedItemKept(string[] except)
    {
        currentItemSlot = emptyItemSlot;
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, keep, except));
    }

    #endregion Category: [current]: Default Keep
    #region Category: [current]: Default Drop

    [TestMethod]
    [DataRow(["[current]"])]
    [DataRow(["key", "[current]"])]
    [DataRow(["[current]", "key"])]
    [DataRow(["[current:not(shovel)]"])]
    [DataRow(["[current:not(shovel)]", "key"])]
    [DataRow(["key", "[current:not(shovel)]"])]
    public void Given_Drop_When_KeepCurrentlyHeld_Then_CurrentlyHeldItemKept(string[] except)
    {
        currentItemSlot = clipboardItemSlot;
        Assert.IsFalse(ItemParser.ShouldDrop(player, clipboard, drop, except));
    }

    [TestMethod]
    [DataRow(["[current]"])]
    [DataRow(["key", "[current]"])]
    [DataRow(["[current]", "key"])]
    [DataRow(["[current:not(shovel)]"])]
    [DataRow(["[current:not(shovel)]", "key"])]
    [DataRow(["key", "[current:not(shovel)]"])]
    public void Given_Drop_When_KeepCurrentlyHeld_Then_PocketedItemDropped(string[] except)
    {
        currentItemSlot = emptyItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, except));
    }

    [TestMethod]
    [DataRow(["[current:not(clipboard)]"])]
    [DataRow(["key", "[current:not(clipboard)]"])]
    [DataRow(["[current:not(clipboard)]", "key"])]
    public void Given_Drop_When_KeepCurrentlyHeldExceptItem_Then_CurrentlyHeldItemDropped(string[] except)
    {
        currentItemSlot = clipboardItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, except));
    }

    [TestMethod]
    [DataRow(["[current:not(clipboard)]"])]
    [DataRow(["key", "[current:not(clipboard)]"])]
    [DataRow(["[current:not(clipboard)]", "key"])]
    public void Given_Drop_When_KeepCurrentlyHeldExceptItem_Then_PocketedItemDropped(string[] except)
    {
        currentItemSlot = emptyItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, except));
    }

    [TestMethod]
    [DataRow(["[current:not(key,clipboard)]"])]
    [DataRow(["[current:not(clipboard,key)]"])]
    [DataRow(["key", "[current:not(key,clipboard)]"])]
    [DataRow(["key", "[current:not(clipboard,key)]"])]
    [DataRow(["[current:not(key,clipboard)]", "key"])]
    [DataRow(["[current:not(clipboard,key)]", "key"])]
    public void Given_Drop_When_KeepCurrentlyHeldExceptListContainsItem_Then_CurrentlyHeldItemDropped(string[] except)
    {
        currentItemSlot = clipboardItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, except));
    }

    [TestMethod]
    [DataRow(["[current:not(key,clipboard)]"])]
    [DataRow(["[current:not(clipboard,key)]"])]
    [DataRow(["key", "[current:not(key,clipboard)]"])]
    [DataRow(["key", "[current:not(clipboard,key)]"])]
    [DataRow(["[current:not(key,clipboard)]", "key"])]
    [DataRow(["[current:not(clipboard,key)]", "key"])]
    public void Given_Drop_When_KeepCurrentlyHeldExceptListContainsItem_Then_PocketedItemDropped(string[] except)
    {
        currentItemSlot = emptyItemSlot;
        Assert.IsTrue(ItemParser.ShouldDrop(player, clipboard, drop, except));
    }

    #endregion Category: [current]: Default Drop
    #endregion Category: [current]

    #endregion Category
}
