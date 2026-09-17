using Blackjack.Core.Models;

namespace Blackjack.Core.Test.Models;

public class DealerModelTests
{
    private static CardModel Card(int value) => new($"{value} of Spades", value, value.ToString());
    private static CardModel Ace() => new("Ace of Spades", null, "Ace");

    // --- Initialization ---

    [Fact]
    public void InitializeHand_AddsCardToHand()
    {
        var dealer = new DealerModel();
        var card = Card(10);

        dealer.InitializeHand(card);

        Assert.Same(card, Assert.Single(dealer.Hand));
    }

    [Fact]
    public void InitializeHand_SetsFaceUpValue_ForNumberCard()
    {
        var dealer = new DealerModel();

        dealer.InitializeHand(Card(9));

        Assert.Equal(9, dealer.HandValue);
    }

    [Fact]
    public void InitializeHand_AceFaceUp_CountsAsElevenButDoesNotRaiseAceFlag()
    {
        var dealer = new DealerModel();

        dealer.InitializeHand(Ace());

        Assert.Equal(11, dealer.HandValue);
        Assert.False(dealer.HandHasAce);   // flag is a reveal-phase concept; InitializeHand doesn't own it
    }

    [Fact]
    public void InitializeHand_SecondCard_Throws()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Card(10));

        Assert.Throws<InvalidOperationException>(() => dealer.InitializeHand(Card(5)));
    }

    [Fact]
    public void InitializeHoleCard_SetsHoleCard_WithoutTouchingHandOrValue()
    {
        var dealer = new DealerModel();
        var hole = Card(7);

        dealer.InitializeHoleCard(hole);

        Assert.Same(hole, dealer.HoleCard);
        Assert.Empty(dealer.Hand);
        Assert.Equal(0, dealer.HandValue);    // hole stays hidden from the player-facing value
    }

    [Fact]
    public void InitializeHand_RejectedSecondCard_LeavesHandIntact()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Card(10));

        Assert.Throws<InvalidOperationException>(() => dealer.InitializeHand(Card(5)));
        Assert.Single(dealer.Hand);
    }

    // --- Hard hands ---

    [Fact]
    public void Hit_HardHand_CountsFaceUpHoleAndHit()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Card(10));
        dealer.InitializeHoleCard(Card(7));

        dealer.Hit(Card(4));                 // 10 + 7 + 4

        Assert.Equal(21, dealer.HandValue);
        Assert.False(dealer.BustedHand);
    }

    [Fact]
    public void Hit_HardHand_OverTwentyOne_Busts()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Card(10));
        dealer.InitializeHoleCard(Card(9));

        dealer.Hit(Card(5));                 // 24

        Assert.Equal(24, dealer.HandValue);
        Assert.True(dealer.BustedHand);
    }

    // --- Aces ---

    [Fact]
    public void Hit_SoftHand_DemotesAceToStayAlive()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Ace());
        dealer.InitializeHoleCard(Card(6));

        dealer.Hit(Card(10));                // 11+6+10 = 27 -> ace as 1 -> 17

        Assert.Equal(17, dealer.HandValue);
        Assert.True(dealer.HandHasAce);
        Assert.True(dealer.LowAceHand);
        Assert.False(dealer.BustedHand);
    }

    // Encodes the one-at-a-time rule. For all-aces-to-1, this expected value becomes 11.
    [Fact]
    public void Hit_TwoAces_DemotesOnlyOne()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Ace());
        dealer.InitializeHoleCard(Ace());

        dealer.Hit(Card(9));                 // 11+11+9 = 31 -> demote one -> 21

        Assert.Equal(21, dealer.HandValue);
        Assert.False(dealer.BustedHand);
    }

    [Fact]
    public void Hit_AcesDemotedButStillOverTwentyOne_Busts()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Card(10));
        dealer.InitializeHoleCard(Card(10));

        dealer.Hit(Ace());                   // 10+10+11 = 31 -> 21 (alive)
        dealer.Hit(Ace());                   // +11 = 32 -> demote both -> 22

        Assert.Equal(22, dealer.HandValue);
        Assert.True(dealer.BustedHand);
    }

    // Bug 3 regression: soft hand must not false-bust on a later hit.
    [Fact]
    public void Hit_SoftHand_SecondHit_DoesNotFalselyBust()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Ace());
        dealer.InitializeHoleCard(Card(9));

        dealer.Hit(Card(5));                 // 11+9+5 = 25 -> 15
        dealer.Hit(Card(4));                 // +4 = 29 -> 19

        Assert.Equal(19, dealer.HandValue);
        Assert.False(dealer.BustedHand);
    }

    // --- Hole card handling ---

    // Bug 1 regression: hole card counted once, not re-added each calc.
    [Fact]
    public void Hit_Twice_DoesNotDoubleCountHoleCard()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Card(5));
        dealer.InitializeHoleCard(Card(6));

        dealer.Hit(Card(4));                 // 5+6+4  = 15
        dealer.Hit(Card(3));                 // +3     = 18

        Assert.Equal(18, dealer.HandValue);
        Assert.False(dealer.BustedHand);
    }

    // Bug 2 regression: no hole card set must not throw.
    [Fact]
    public void Hit_WithNoHoleCard_CountsOnlyHand()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Card(10));

        dealer.Hit(Card(5));                 // 10 + 5, hole absent

        Assert.Equal(15, dealer.HandValue);
        Assert.False(dealer.BustedHand);
    }

    // Idempotency: recomputing over the same cards is a no-op.
    [Fact]
    public void CalculateHandValue_IsIdempotent()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Ace());
        dealer.InitializeHoleCard(Card(6));
        dealer.Hit(Card(10));                // 17

        dealer.CalculateHandValue();
        dealer.CalculateHandValue();

        Assert.Equal(17, dealer.HandValue);
    }
}
