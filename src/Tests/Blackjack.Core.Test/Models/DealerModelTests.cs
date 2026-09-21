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

    // --- NewRound ---

    [Fact]
    public void NewRound_ClearsHandHoleAndFlags()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Ace());
        dealer.InitializeHoleCard(Card(6));
        dealer.Hit(Card(10));                // 27 -> 17, LowAceHand + HandHasAce set
                                             // precondition: state the reset must clear is actually set
        Assert.True(dealer.HandHasAce && dealer.LowAceHand);
        Assert.NotNull(dealer.HoleCard);

        dealer.NewRound();

        Assert.Empty(dealer.Hand);
        Assert.Null(dealer.HoleCard);        // fails today — HoleCard retained
        Assert.Equal(0, dealer.HandValue);   // fails today — HandValue left at 17
        Assert.False(dealer.BustedHand);
        Assert.False(dealer.HandHasAce);
        Assert.False(dealer.LowAceHand);
    }

    [Fact]
    public void NewRound_ClearsBust()
    {
        var dealer = new DealerModel();
        dealer.InitializeHand(Card(10));
        dealer.InitializeHoleCard(Card(9));
        dealer.Hit(Card(5));                 // 24, bust
        Assert.True(dealer.BustedHand);

        dealer.NewRound();

        Assert.False(dealer.BustedHand);
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

    // --- Aces ---

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
        Assert.Equal(2, dealer.Hand.Count);   // hole never re-added to Hand — the actual thing that regressed
    }
}
