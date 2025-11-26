using BetterBetterTeleporter.Adapters;
using BetterBetterTeleporter.Tests.Fakes;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Tests.Utility;

[TestClass]
public sealed class ItemRulesTest
{
    public sealed class AlwaysMatchesRule() : ItemRule("true") { public override bool IsMatch(IPlayerInfo player, IItemInfo item) => true; }
    public sealed class NeverMatchesRule() : ItemRule("false") { public override bool IsMatch(IPlayerInfo player, IItemInfo item) => false; }
    public sealed class EmptyAnd() : ItemFilter("true", new(false, [])) { }
    public sealed class EmptyNot() : ItemFilter("true", new(true, [])) { }
    public sealed class NoMatchesOnAnd() : ItemFilter("true/false", new(false, [new NeverMatchesRule()])) { }
    public sealed class NoMatchesOnNot() : ItemFilter("true/false", new(true, [new NeverMatchesRule()])) { }
    public sealed class MatchesOnAnd() : ItemFilter("true/true", new(false, [new AlwaysMatchesRule()])) { }
    public sealed class MatchesOnNot() : ItemFilter("true/true", new(true, [new AlwaysMatchesRule()])) { }

    private FakePlayerInfo player = null!;
    private FakeClipboardItemInfo clipboard = null!;

    [TestInitialize]
    public void Setup()
    {
        clipboard = new FakeClipboardItemInfo();
        player = new FakePlayerInfo(clipboard);
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
    public void Given_Behavior_When_MatchedWithEmpty_Then_ShouldFlipBehavior(bool behavior)
    {
        Assert.AreNotEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new EmptyAnd()]));
        Assert.AreNotEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new EmptyNot()]));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_Match_When_FailsOnAnd_Then_ShouldMatchBehavior(bool behavior)
    {
        Assert.AreEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new NoMatchesOnAnd()]));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_Match_When_FailsOnNot_Then_ShouldFlipBehavior(bool behavior)
    {
        Assert.AreNotEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new NoMatchesOnNot()]));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_Match_When_MatchesOnAnd_Then_ShouldFlipBehavior(bool behavior)
    {
        Assert.AreNotEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new MatchesOnAnd()]));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_Match_When_MatchesOnNot_Then_ShouldMatchBehavior(bool behavior)
    {
        Assert.AreEqual(behavior, player.ShouldDropItem(clipboard, behavior, [new MatchesOnNot()]));
    }
}
