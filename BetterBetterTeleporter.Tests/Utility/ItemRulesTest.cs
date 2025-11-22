using BetterBetterTeleporter.Adapters;
using BetterBetterTeleporter.Tests.Fakes;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Tests.Utility;

[TestClass]
public sealed class ItemRulesTest
{
    public sealed class AlwaysMatchesRule() : ItemRule("true") { public override bool IsMatch(IPlayerInfo player, IItemInfo item) => true; }
    public sealed class NeverMatchesRule() : ItemRule("false") { public override bool IsMatch(IPlayerInfo player, IItemInfo item) => false; }
    public sealed class UnmatchedFilter() : RuleFilter("false", []) { public override bool IsMatch(IPlayerInfo player, IItemInfo item) => false; }
    public sealed class EmptyMatchedFilter() : RuleFilter("true", []) { }
    public sealed class MatchedFilter() : RuleFilter("true/false", [new NeverMatchesRule()]) { }
    public sealed class NegatedMatchedFilter() : RuleFilter("true/true", [new AlwaysMatchesRule()]) { }

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
    [DataRow(true)]
    [DataRow(false)]
    public void Given_Behavior_When_NoRules_Then_ShouldMatchBehavior(bool behavior)
    {
        Assert.AreEqual(behavior, player.ShouldDropItem(clipboard, behavior, []));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_Behavior_When_NoMatchedRule_Then_ShouldMatchBehavior(bool behavior)
    {
        Assert.AreEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new NeverMatchesRule()]));
        Assert.AreEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new NeverMatchesRule(), new NeverMatchesRule()]));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_Behavior_When_MatchedRule_Then_ShouldFlipBehavior(bool behavior)
    {
        Assert.AreNotEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new AlwaysMatchesRule()]));
        Assert.AreNotEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new NeverMatchesRule(), new AlwaysMatchesRule()]));
        Assert.AreNotEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new AlwaysMatchesRule(), new NeverMatchesRule()]));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_Behavior_When_UnmatchedFilter_Then_ShouldMatchBehavior(bool behavior)
    {
        Assert.AreEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new UnmatchedFilter()]));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_Behavior_When_EmptyMatchedFilter_Then_ShouldFlipBehavior(bool behavior)
    {
        Assert.AreNotEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new EmptyMatchedFilter()]));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_Behavior_When_MatchedFilter_Then_ShouldFlipBehavior(bool behavior)
    {
        Assert.AreNotEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new MatchedFilter()]));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_Behavior_When_NegatedMatchedFilter_Then_ShouldMatchBehavior(bool behavior)
    {
        Assert.AreEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new NegatedMatchedFilter()]));
    }
}
