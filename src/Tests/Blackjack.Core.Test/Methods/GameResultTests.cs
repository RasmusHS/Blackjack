using Blackjack.Core.Models;

namespace Blackjack.Core.Test.Methods;

public class GameResultTests
{
    private static CardModel Card(int v) => new($"{v} of Spades", v, v.ToString());
    private static CardModel Ace() => new("Ace of Spades", null, "Ace");
    private static CardModel Ten(string type) => new($"{type} of Spades", 10, type);
    private static GameModel Game() => new(new DealerModel(), new());

    // Dealer with a known, revealed hand value. Result reads only HandValue/BustedHand,
    // so building via Hit (no hole) is sufficient and keeps setup to one line.
    private static DealerModel DealerWith(params CardModel[] cards)
    {
        var d = new DealerModel();
        d.InitializeHand(cards[0]);
        for (int i = 1; i < cards.Length; i++) d.Hit(cards[i]);
        return d;
    }

    // Main-hand player with two known cards; returns player + its post-ante balance.
    private static (PlayerModel p, int baseline) PlayerWith(CardModel a, CardModel b)
    {
        var p = new PlayerModel(Guid.NewGuid(), "P1", null);
        p.InitializeHand(a);
        p.InitializeHand(b);
        return (p, p.Points);
    }

    // --- Losses: stake already taken up front, so a loss credits nothing ---

    [Fact]
    public void Result_PlayerBusts_NoCredit()
    {
        var (p, baseline) = PlayerWith(Ten("King"), Card(9));
        p.Hit(Card(5));                                   // 24, bust
        Game().Result(DealerWith(Ten("King"), Card(8)), new() { p });   // dealer 18
        Assert.Equal(baseline, p.Points);
    }

    [Fact]
    public void Result_PlayerLowerThanDealer_NoCredit()
    {
        var (p, baseline) = PlayerWith(Ten("King"), Card(7));   // 17
        Game().Result(DealerWith(Ten("King"), Card(9)), new() { p });   // 19
        Assert.Equal(baseline, p.Points);
    }

    // --- Wins ---

    [Fact]
    public void Result_RegularWin_PaysTwoTimesBet()
    {
        var (p, baseline) = PlayerWith(Ten("King"), Card(9));   // 19
        Game().Result(DealerWith(Ten("King"), Card(7)), new() { p });   // 17
        Assert.Equal(baseline + BaseValues.Bet * 2, p.Points);
    }

    [Fact]
    public void Result_DealerBusts_LivePlayerWins()             // dealer-bust win condition
    {
        var (p, baseline) = PlayerWith(Ten("King"), Card(8));   // 18
        Game().Result(DealerWith(Ten("King"), Card(6), Card(9)), new() { p });  // 25 bust
        Assert.Equal(baseline + BaseValues.Bet * 2, p.Points);
    }

    // --- Push ---

    [Fact]
    public void Result_Push_ReturnsBet()
    {
        var (p, baseline) = PlayerWith(Ten("King"), Card(8));   // 18
        Game().Result(DealerWith(Ten("King"), Card(8)), new() { p });   // 18
        Assert.Equal(baseline + BaseValues.Bet, p.Points);
    }

    // --- Blackjack: natural (two-card 21) only ---

    [Fact]
    public void Result_NaturalBlackjack_PaysThreeToTwo()
    {
        var (p, baseline) = PlayerWith(Ace(), Ten("King"));     // two-card 21
        Assert.True(p.IsBlackjack);
        Game().Result(DealerWith(Ten("King"), Card(9)), new() { p });   // 19
        Assert.Equal(baseline + (int)Math.Round(BaseValues.Bet * 2.5, MidpointRounding.AwayFromZero), p.Points);
    }

    [Fact]
    public void Result_DrawnTwentyOne_PaysEvenMoney_NotBlackjack()
    {
        var (p, baseline) = PlayerWith(Card(7), Card(6));       // 13
        p.Hit(Card(8));                                   // 21, three cards
        Assert.False(p.IsBlackjack);
        Game().Result(DealerWith(Ten("King"), Card(9)), new() { p });   // 19
        Assert.Equal(baseline + BaseValues.Bet * 2, p.Points); // 2x, not 2.5x
    }

    // --- Split ---

    [Fact]
    public void Result_Split_OneWinsOneLoses_PaysWinnerOnly()
    {
        var p = new PlayerModel(Guid.NewGuid(), "P1", null);
        p.InitializeHand(Card(8));
        p.InitializeHand(Card(8));
        p.Split(Card(10), Card(3));            // main=[8,10]=18 active, split=[8,3]=11
        var baseline = p.Points;
        p.Stand();                             // resolve main (18) -> active split
        p.Hit(Card(9));                        // split [8,3,9]=20
        Game().Result(DealerWith(Ten("King"), Card(9)), new() { p });  // dealer 19: main 18 loses, split 20 wins

        Assert.Equal(baseline + BaseValues.Bet * 2, p.Points);
    }

    [Fact]
    public void Result_Split_TwentyOne_IsNotBlackjack()
    {
        var p = new PlayerModel(Guid.NewGuid(), "P1", null);
        p.InitializeHand(Ace());
        p.InitializeHand(Ace());
        p.Split(Ten("King"), Ten("Queen"));    // Hand=[A,K]=21, Split=[A,Q]=21 — split aces, NOT blackjack
        var baseline = p.Points;
        Game().Result(DealerWith(Ten("King"), Card(9)), new() { p });   // 19: both win at 2x
        Assert.Equal(baseline + BaseValues.Bet * 2 + BaseValues.Bet * 2, p.Points);
    }
}
