using Blackjack.Core.Models;

namespace Blackjack.Core.Test.Models;

public class PlayerModelTests
{
    private static PlayerModel Player() => new(Guid.NewGuid(), "P1", null);
    private static CardModel Card(int value) => new($"{value} of Spades", value, value.ToString());
    private static CardModel Ace() => new("Ace of Spades", null, "Ace");
    private static CardModel Face(string type) => new($"{type} of Spades", 10, type); // Jack/Queen/King

    private static PlayerModel Pair(int value)   // [v, v], fully initialized: Bet 50, Points 950
    {
        var p = Player();
        p.InitializeHand(Card(value));
        p.InitializeHand(Card(value));
        return p;
    }

    // --- Construction ---

    [Fact]
    public void Ctor_AssignsIdentity()
    {
        var gameId = Guid.NewGuid();

        var p = new PlayerModel(gameId, "Alice", null);

        Assert.NotEqual(Guid.Empty, p.Id);
        Assert.Equal(gameId, p.GameId);
        Assert.Equal("Alice", p.Name);
    }

    [Fact]
    public void Ctor_NullPoints_UsesDefaultBalance()
    {
        var p = new PlayerModel(Guid.NewGuid(), "P1", null);
        Assert.Equal(BaseValues.InitialPlayerBalance - BaseValues.Bet, p.Points);
        Assert.Equal(BaseValues.Bet, p.Bet);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Ctor_NonPositivePoints_UsesDefaultBalance(int points)
    {
        var p = new PlayerModel(Guid.NewGuid(), "P1", points);
        Assert.Equal(BaseValues.InitialPlayerBalance - BaseValues.Bet, p.Points);
    }

    [Fact]
    public void Ctor_PositivePoints_HonorsProvidedBalance()
    {
        var p = new PlayerModel(Guid.NewGuid(), "P1", 500);   // != default, so this proves the else-branch
        Assert.Equal(500 - BaseValues.Bet, p.Points);
    }

    // --- InitializeHand ---

    [Fact]
    public void InitializeHand_FirstCard_DoesNotValueYet()
    {
        var p = Player();
        p.InitializeHand(Card(9));
        Assert.Equal(0, p.HandValue);   // value shown only once both cards are dealt
    }

    [Fact]
    public void InitializeHand_TwoCards_ComputesValue()
    {
        var p = Player();
        p.InitializeHand(Card(9));
        p.InitializeHand(Card(7));
        Assert.Equal(16, p.HandValue);
    }

    [Fact]
    public void InitializeHand_DoesNotAlterBet()
    {
        var p = Player();
        var betAfterCtor = p.Bet;
        var pointsAfterCtor = p.Points;

        p.InitializeHand(Card(9));
        p.InitializeHand(Card(7));

        Assert.Equal(betAfterCtor, p.Bet);        // deal touches neither
        Assert.Equal(pointsAfterCtor, p.Points);
    }

    [Fact]
    public void InitializeHand_ThirdCard_Throws()
    {
        var p = Player();
        p.InitializeHand(Card(9));
        p.InitializeHand(Card(7));
        Assert.Throws<InvalidOperationException>(() => p.InitializeHand(Card(2)));
    }

    [Fact]
    public void InitializeHand_PairOfAces_IsSplittableNotForced()
    {
        var p = Player();
        p.InitializeHand(Ace());
        p.InitializeHand(Ace());

        Assert.Equal(12, p.HandValue);   // 11 + 1
        Assert.False(p.HasSplit);
        Assert.True(p.SplitPossible);
        Assert.False(p.EndedTurn);
    }

    // --- NewRound ---

    [Fact]
    public void NewRound_ResetsSplitAndAceState_PostsNewBet()
    {
        var p = Player();
        p.InitializeHand(Ace());
        p.InitializeHand(Ace());         // pair of aces
        p.Split(Ace(), Ace());           // Hand=[A,A]=12, Split=[A,A]=12
                                         // precondition: everything the reset must clear is actually set
        Assert.True(p.HasSplit);
        Assert.True(p.HandHasAce && p.LowAceHand && p.SplitHasAce && p.LowAceSplit);
        Assert.Equal(50, p.SplitBet);

        var pointsBefore = p.Points;
        p.NewRound(75);

        Assert.Empty(p.Hand);
        Assert.Empty(p.SplitHand);
        Assert.False(p.HasSplit);
        Assert.False(p.HandHasAce);
        Assert.False(p.LowAceHand);
        Assert.False(p.SplitHasAce);
        Assert.False(p.LowAceSplit);
        Assert.Equal(0, p.SplitBet);
        Assert.Equal(75, p.Bet);
        Assert.Equal(pointsBefore - 75, p.Points);   // new ante deducted from carried-over stack
    }

    [Fact]
    public void NewRound_ClearsBustAndEndedTurn()
    {
        var p = Player();
        p.InitializeHand(Card(10));
        p.InitializeHand(Card(9));
        p.Hit(Card(5), null);            // 24 -> BustedHand, EndedTurn
        Assert.True(p.BustedHand && p.EndedTurn);

        p.NewRound(50);

        Assert.False(p.BustedHand);
        Assert.False(p.EndedTurn);
    }

    [Fact]
    public void NewRound_ResetsHandValues()
    {
        var p = Player();
        p.InitializeHand(Card(10));
        p.InitializeHand(Card(9));       // HandValue 19
        p.NewRound(50);
        Assert.Equal(0, p.HandValue);
        Assert.Equal(0, p.SplitHandValue);
    }

    // --- Hit (no split) ---

    [Fact]
    public void Hit_UnderTwentyOne_Revalues()
    {
        var p = Player();
        p.InitializeHand(Card(5));
        p.InitializeHand(Card(6));
        p.Hit(Card(9), null);
        Assert.Equal(20, p.HandValue);
        Assert.False(p.BustedHand);
    }

    [Fact]
    public void Hit_Busts_EndsTurn()
    {
        var p = Player();
        p.InitializeHand(Card(10));
        p.InitializeHand(Card(9));
        p.Hit(Card(5), null);            // 24
        Assert.True(p.BustedHand);
        Assert.True(p.EndedTurn);
    }

    [Fact]
    public void Hit_DisablesSplit()
    {
        var p = Pair(8);
        p.Hit(Card(2), null);            // Count -> 3
        Assert.False(p.SplitPossible);
    }

    // Regression: latch bug — a soft hand must not false-bust on a later hit.
    [Fact]
    public void Hit_SoftHand_SecondHit_DoesNotFalselyBust()
    {
        var p = Player();
        p.InitializeHand(Ace());
        p.InitializeHand(Card(6));       // 17
        p.Hit(Card(5), null);            // 22 -> 12
        p.Hit(Card(4), null);            // 26 -> 16
        Assert.Equal(16, p.HandValue);
        Assert.False(p.BustedHand);
    }

    // --- Stand ---

    [Fact]
    public void Stand_EndsTurn()
    {
        var p = Player();
        p.Stand();
        Assert.True(p.EndedTurn);
    }

    // --- SplitPossible (behavioral; agnostic to derived vs stored) ---

    [Fact]
    public void SplitPossible_TrueForMatchingPair()
    {
        Assert.True(Pair(8).SplitPossible);
    }

    [Fact]
    public void SplitPossible_FalseForDifferentType()
    {
        var p = Player();
        p.InitializeHand(Card(10));
        p.InitializeHand(Face("King")); // both value 10, types "10" vs "King"
        Assert.False(p.SplitPossible);
    }

    [Fact]
    public void SplitPossible_FalseAfterHit()
    {
        var p = Pair(8);
        p.Hit(Card(2), null);
        Assert.False(p.SplitPossible);
    }

    // --- Split ---

    [Fact]
    public void Split_MatchingPair_CreatesTwoHands_AndCharges()
    {
        var p = Pair(10);                // Bet 50, Points 950

        p.Split(Card(3), Card(5));

        Assert.True(p.HasSplit);
        Assert.False(p.SplitPossible);
        Assert.Equal(new int?[] { 10, 3 }, p.Hand.Select(c => c.Value));
        Assert.Equal(new int?[] { 10, 5 }, p.SplitHand.Select(c => c.Value));
        Assert.Equal(13, p.HandValue);
        Assert.Equal(15, p.SplitHandValue);
        Assert.Equal(50, p.SplitBet);    // matches original bet
        Assert.Equal(900, p.Points);     // second bet deducted
    }

    [Fact]
    public void Split_AceInSplitHand_KeptHigh_FlagsWiredCorrectly()
    {
        var p = Pair(10);
        p.Split(Card(5), Ace());         // Hand=[10,5]=15, Split=[10,Ace]=21

        Assert.Equal(21, p.SplitHandValue);
        Assert.True(p.SplitHasAce);      // ace detected in split hand
        Assert.False(p.LowAceSplit);     // 21, not demoted — flags differ, so a swapped tuple fails here
    }

    [Fact]
    public void Split_AceInSplitHand_DemotesWhenOver()
    {
        var p = Pair(10);
        p.Split(Card(3), Ace());         // Hand=[10,3]=13, Split=[10,Ace]=21
        p.Hit(Card(8), Card(5));         // Hand=[10,3,8]=21, Split=[10,Ace,5]=26 -> demote -> 16

        Assert.Equal(16, p.SplitHandValue);
        Assert.True(p.LowAceSplit);
        Assert.False(p.BustedSplit);
    }

    [Fact]
    public void Split_SameValueDifferentType_NoOp()
    {
        var p = Player();
        p.InitializeHand(Card(10));
        p.InitializeHand(Face("King"));

        p.Split(Card(3), Card(5));

        Assert.False(p.HasSplit);
        Assert.Empty(p.SplitHand);
        Assert.Equal(2, p.Hand.Count);
    }

    [Fact]
    public void Split_AfterHit_NoOp()
    {
        var p = Pair(4);
        p.Hit(Card(2), null);            // [4,4,2] = 10, Count -> 3

        p.Split(Card(3), Card(5));

        Assert.False(p.HasSplit);
    }

    [Fact]
    public void Split_OneHandBusts_OtherLive_TurnContinues()
    {
        var p = Pair(10);
        p.Split(Card(5), Card(2));       // Hand=[10,5]=15, Split=[10,2]=12

        p.Hit(Card(9), Card(3));         // Hand=24 (bust), Split=15 (live)

        Assert.True(p.BustedHand);
        Assert.False(p.BustedSplit);
        Assert.False(p.EndedTurn);       // still a live hand
    }

    [Fact]
    public void Split_BothHandsBust_EndsTurn()
    {
        var p = Pair(10);
        p.Split(Card(5), Card(4));       // Hand=15, Split=14

        p.Hit(Card(9), Card(10));        // Hand=24, Split=24

        Assert.True(p.BustedHand);
        Assert.True(p.BustedSplit);
        Assert.True(p.EndedTurn);
    }

    // Bug 4: busted hand receives no further cards — GameModel passes null for it.
    [Fact]
    public void Split_NullForBustedHand_DealsOnlyToLiveHand()
    {
        var p = Pair(10);
        p.Split(Card(5), Card(2));       // Hand=15, Split=12
        p.Hit(Card(9), Card(3));         // Hand=24 (bust), Split=15

        p.Hit(null, Card(4));            // busted Hand skipped; Split=[10,2,3,4]=19

        Assert.Equal(3, p.Hand.Count);
        Assert.Equal(24, p.HandValue);
        Assert.Equal(19, p.SplitHandValue);
        Assert.False(p.BustedSplit);
    }

    // --- DoubleDown ---

    // Regression: Points -= Points - (Bet/2) wiped the stack; deduction is the added wager only.
    [Fact]
    public void DoubleDown_NoSplit_DoublesBet_DeductsOriginal_Stands()
    {
        var p = Player();
        p.InitializeHand(Card(5));
        p.InitializeHand(Card(6));       // Bet 50, Points 950, value 11

        p.DoubleDown(Card(9));     // 20

        Assert.Equal(100, p.Bet);
        Assert.Equal(900, p.Points);
        Assert.Equal(20, p.HandValue);
        Assert.True(p.EndedTurn);
    }

    // NDAS: doubling is not allowed once the hand is split.
    [Fact]
    public void DoubleDown_AfterSplit_Rejected()
    {
        var p = Pair(10);
        p.Split(Card(3), Card(5));       // HasSplit = true; Points 900
        var pointsBefore = p.Points;
        var betBefore = p.Bet;

        p.DoubleDown(Card(2));  // CanDoubleDown == false -> no-op

        Assert.False(p.HasSplit && p.EndedTurn); // turn not force-ended by a rejected double
        Assert.Equal(betBefore, p.Bet);          // no bet change
        Assert.Equal(pointsBefore, p.Points);    // no deduction
        Assert.Equal(2, p.Hand.Count);           // no card added
    }

    // Guards CanDoubleDown's count arm: no double after a hit (3+ cards).
    [Fact]
    public void DoubleDown_AfterHit_Rejected()
    {
        var p = Player();
        p.InitializeHand(Card(5));
        p.InitializeHand(Card(6));
        p.Hit(Card(2), null);            // Hand.Count -> 3
        var pointsBefore = p.Points;

        p.DoubleDown(Card(9));

        Assert.Equal(3, p.Hand.Count);   // still 3; double rejected, no card added
        Assert.Equal(pointsBefore, p.Points);
    }
}
