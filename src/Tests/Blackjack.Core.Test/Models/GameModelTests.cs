using Blackjack.Core.Models;
using Blackjack.Core.Services;
using Blackjack.Core.Test.TestSupport;
using Moq;

namespace Blackjack.Core.Test.Models;

public class GameModelTests
{
    private static List<PlayerModel> Roster(int n) =>
        Enumerable.Range(1, n).Select(_ => new PlayerModel(Guid.NewGuid(), "x", null)).ToList();

    private static GameModel Started(int players)
    {
        var game = new GameModel(new DealerModel(), Roster(players));
        game.StartGame();
        return game;
    }

    [Fact]
    public void Ctor_DerivesPlayerCount_AndUniqueId()
    {
        var a = new GameModel(new DealerModel(), Roster(3));
        var b = new GameModel(new DealerModel(), Roster(3));
        Assert.Equal(3, a.PlayerCount);
        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.NotEqual(a.Id, b.Id);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void StartGame_DealsTwoCardsToEachPlayer(int n)
    {
        var game = Started(n);
        Assert.Equal(n, game.Players.Count);
        Assert.All(game.Players, p => Assert.Equal(2, p.Hand.Count));
    }

    [Fact]
    public void StartGame_DealsDealerFaceUpPlusHole()
    {
        var game = Started(2);
        Assert.Single(game.Dealer.Hand);       // face-up only; hole not merged pre-reveal
        Assert.NotNull(game.Dealer.HoleCard);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void StartGame_LeavesCorrectDeckRemainder(int n)
    {
        Assert.Equal(52 - (2 * n + 2), Started(n).Deck.Count);
    }

    [Fact]
    public void StartGame_ConservesAll52Cards()
    {
        var game = Started(2);
        var total = game.Players.Sum(p => p.Hand.Count)
                    + game.Dealer.Hand.Count + 1          // hole
                    + game.Deck.Count;
        Assert.Equal(52, total);
    }

    [Fact]
    public void StartGame_NamesAndAntesPlayers()
    {
        var game = Started(2);
        Assert.Equal("Player 1", game.Players[0].Name);
        Assert.Equal("Player 2", game.Players[1].Name);
        Assert.All(game.Players, p =>
        {
            Assert.Equal(game.Id, p.GameId);
            Assert.Equal(50, p.Bet);
            Assert.Equal(950, p.Points);
        });
    }

    // ---------------- NewRound ----------------

    [Fact]
    public void NewRound_ResetsAndRedealsFromFullDeck()
    {
        var dealer = new DealerModel();
        var players = Roster(1);
        var game = new GameModel(dealer, players);
        game.StartGame();                 // play a round's worth of state onto them
        game.HitPlayer(players[0]);       // dirty state: extra card, revalued hand

        game.NewRound(dealer, players);   // real 52-card deck (guard requires exactly 52)

        Assert.All(players, p => Assert.Equal(2, p.Hand.Count));   // redealt to two
        Assert.Single(dealer.Hand);                                // face-up only
        Assert.NotNull(dealer.HoleCard);
        Assert.Equal(52 - (2 * 1 + 2), game.Deck.Count);           // 48 left after the deal
    }

    [Fact]
    public void NewRound_RejectsNonFullDeck()
    {
        var game = new GameModel(new DealerModel(), Roster(1));
        var shortDeck = new ScriptedDeck(new[] { Card(2), Card(3) }); // 2 cards, not 52

        Assert.Throws<InvalidOperationException>(
            () => game.NewRound(new DealerModel(), Roster(1), shortDeck));
    }

    [Fact]
    public void StandPlayer_EndsPlayerTurn()
    {
        var game = Started(1);
        game.StandPlayer(game.Players[0]);
        Assert.True(game.Players[0].EndedTurn);
    }

    [Fact]
    public void SplitPlayer_WhenNotSplittable_DrawsNothing()
    {
        var deck = new Mock<IDeck>();
        deck.Setup(d => d.Count).Returns(10);
        deck.Setup(d => d.Draw()).Returns(new CardModel("2 of Spades", 2, "2"));

        var game = new GameModel(new DealerModel(), new());
        game.StartGame(deck.Object);     // 0 players, but still draws 2 for the dealer
        deck.Invocations.Clear();        // measure only draws that follow, not StartGame's deal

        var player = new PlayerModel(Guid.NewGuid(), "P1", null);
        player.InitializeHand(new CardModel("10 of Spades", 10, "10"));
        player.InitializeHand(new CardModel("King of Spades", 10, "King")); // same value, types differ

        game.SplitPlayer(player);        // SplitPossible == false -> short-circuits before Deck.Count

        deck.Verify(d => d.Draw(), Times.Never);
    }

    private static CardModel Card(int v) => new($"{v} of Spades", v, v.ToString());
    private static CardModel Ace() => new("Ace of Spades", null, "Ace");

    // 0-player game; `script` drives every draw. First two feed the dealer's deal.
    private static GameModel StartedWith(params CardModel[] script)
    {
        var game = new GameModel(new DealerModel(), new());
        game.StartGame(new ScriptedDeck(script));
        return game;
    }

    // For player-action tests: prepends the 2 dealer-deal cards so `afterDeal` is what the action method draws.
    private static GameModel StartedForPlayerAction(params CardModel[] afterDeal) =>
        StartedWith(new[] { Card(2), Card(2) }.Concat(afterDeal).ToArray());

    // ---------------- HitPlayer ----------------

    [Fact]
    public void HitPlayer_NoSplit_DrawsOneAndRevalues()
    {
        var game = StartedForPlayerAction(Card(6));
        var player = new PlayerModel(Guid.NewGuid(), "P1", null);
        player.InitializeHand(Card(9));
        player.InitializeHand(Card(5));          // 14, no split

        game.HitPlayer(player);                  // draws 6 -> 20

        Assert.Equal(3, player.Hand.Count);
        Assert.Equal(20, player.HandValue);
        Assert.False(player.SplitPossible);      // Hit disables split
        Assert.Equal(0, game.Deck.Count);
    }

    [Fact]
    public void HitPlayer_Split_BothLive_DealsToBothHands()
    {
        var game = StartedForPlayerAction(Card(9), Card(7));
        var player = new PlayerModel(Guid.NewGuid(), "P1", null);
        player.InitializeHand(Card(8));
        player.InitializeHand(Card(8));
        player.Split(Card(2), Card(3));          // Hand=[8,2]=10, Split=[8,3]=11

        game.HitPlayer(player);                  // main<-9 (19), split<-7 (18)

        Assert.Equal(19, player.HandValue);
        Assert.Equal(18, player.SplitHandValue);
        Assert.Equal(0, game.Deck.Count);        // drew exactly two
    }

    // The point of the whole seam: a busted hand gets null, not a card.
    [Fact]
    public void HitPlayer_Split_MainBusted_DealsOnlyToSplit()
    {
        var game = StartedForPlayerAction(Card(4), Card(2)); // Card(4) = the one live draw; Card(2) = spare
        var player = new PlayerModel(Guid.NewGuid(), "P1", null);
        player.InitializeHand(Card(10));
        player.InitializeHand(Card(10));
        player.Split(Card(5), Card(2));          // Hand=[10,5]=15, Split=[10,2]=12
        player.Hit(Card(9), Card(3));            // Hand=[10,5,9]=24 BUST, Split=[10,2,3]=15

        game.HitPlayer(player);                  // main busted -> null; split <- 4

        Assert.Equal(3, player.Hand.Count);      // frozen
        Assert.Equal(24, player.HandValue);
        Assert.Equal(4, player.SplitHand.Count); // grew
        Assert.Equal(19, player.SplitHandValue);
        Assert.Equal(1, game.Deck.Count);        // spare untouched -> drew exactly one
    }

    // Mirror: catches an asymmetric routing bug where only main-busted is handled.
    [Fact]
    public void HitPlayer_Split_SplitBusted_DealsOnlyToMain()
    {
        var game = StartedForPlayerAction(Card(2), Card(2)); // first = live main draw, second = spare
        var player = new PlayerModel(Guid.NewGuid(), "P1", null);
        player.InitializeHand(Card(10));
        player.InitializeHand(Card(10));
        player.Split(Card(5), Card(2));          // Hand=[10,5]=15, Split=[10,2]=12
        player.Hit(Card(3), Card(10));           // Hand=[10,5,3]=18, Split=[10,2,10]=22 BUST

        game.HitPlayer(player);                  // split busted -> null; main <- 2

        Assert.Equal(4, player.Hand.Count);
        Assert.Equal(20, player.HandValue);
        Assert.Equal(3, player.SplitHand.Count); // frozen
        Assert.True(player.BustedSplit);
        Assert.Equal(1, game.Deck.Count);
    }

    // ---------------- DoubleDownPlayer ----------------

    [Fact]
    public void DoubleDownPlayer_NoSplit_DrawsOneAndStands()
    {
        var game = StartedForPlayerAction(Card(9));
        var player = new PlayerModel(Guid.NewGuid(), "P1", null);
        player.InitializeHand(Card(5));
        player.InitializeHand(Card(6));          // Bet 50, Points 950, value 11

        game.DoubleDownPlayer(player);           // draws 9 -> 20

        Assert.Equal(20, player.HandValue);
        Assert.Equal(100, player.Bet);           // no-split branch fired (bet math owned by PlayerModelTests)
        Assert.Equal(900, player.Points);
        Assert.True(player.EndedTurn);
        Assert.Equal(0, game.Deck.Count);
    }

    [Fact]
    public void DoubleDownPlayer_Split_Rejected_DrawsNothing()
    {
        var game = StartedForPlayerAction(Card(2), Card(4)); // spares; must stay in the deck
        var player = new PlayerModel(Guid.NewGuid(), "P1", null);
        player.InitializeHand(Card(10));
        player.InitializeHand(Card(10));
        player.Split(Card(3), Card(5));       // HasSplit -> CanDoubleDown false

        var deckBefore = game.Deck.Count;
        game.DoubleDownPlayer(player);        // no-op under NDAS

        Assert.Equal(deckBefore, game.Deck.Count);  // nothing drawn
        Assert.Equal(50, player.Bet);                // bet untouched
        Assert.False(player.EndedTurn);              // rejected double doesn't force-stand
    }

    // ---------------- SplitPlayer ----------------

    [Fact]
    public void SplitPlayer_Splittable_DrawsTwoAndSplits()
    {
        var game = StartedForPlayerAction(Card(3), Card(5));
        var player = new PlayerModel(Guid.NewGuid(), "P1", null);
        player.InitializeHand(Card(8));
        player.InitializeHand(Card(8));          // matching pair

        game.SplitPlayer(player);                // Hand=[8,3]=11, Split=[8,5]=13

        Assert.True(player.HasSplit);
        Assert.Equal(11, player.HandValue);
        Assert.Equal(13, player.SplitHandValue);
        Assert.Equal(0, game.Deck.Count);        // drew exactly two
    }

    // Guards the other half of bug 5: a splittable player still can't split a deck that can't supply both.
    [Fact]
    public void SplitPlayer_DeckHasFewerThanTwo_DrawsNothing()
    {
        var game = StartedForPlayerAction(Card(3)); // only one card after the deal
        var player = new PlayerModel(Guid.NewGuid(), "P1", null);
        player.InitializeHand(Card(8));
        player.InitializeHand(Card(8));

        game.SplitPlayer(player);                // Count >= 2 fails -> no-op

        Assert.False(player.HasSplit);
        Assert.Equal(1, game.Deck.Count);        // nothing drawn
    }

    // ---------------- HitDealer ----------------

    // The do/while regression catcher: a made 17 must not take a card.
    [Fact]
    public void HitDealer_StandsOnSeventeen_DoesNotHit()
    {
        var game = StartedWith(Card(10), Card(7)); // face-up 10, hole 7 -> 17 on reveal

        game.HitDealer();

        Assert.Equal(17, game.Dealer.HandValue);
        Assert.Single(game.Dealer.Hand);          // still just the face-up; a hit would make it 2
    }

    [Fact]
    public void HitDealer_HitsUntilSeventeen()
    {
        var game = StartedWith(Card(5), Card(6), Card(4), Card(3)); // reveal 11 -> +4=15 -> +3=18

        game.HitDealer();

        Assert.Equal(18, game.Dealer.HandValue);
        Assert.False(game.Dealer.BustedHand);
        Assert.Equal(3, game.Dealer.Hand.Count);  // face-up + two hits (hole counted, not in Hand)
    }

    [Fact]
    public void HitDealer_HitsIntoBust_Stops()
    {
        var game = StartedWith(Card(10), Card(6), Card(10)); // reveal 16 -> +10 = 26

        game.HitDealer();

        Assert.Equal(26, game.Dealer.HandValue);
        Assert.True(game.Dealer.BustedHand);
        Assert.Equal(2, game.Dealer.Hand.Count);
    }

    // Reveal must fold in the hole even with no cards left to draw (the old Count>0 wrapper skipped it).
    [Fact]
    public void HitDealer_RevealsHoleEvenWhenDeckEmpty()
    {
        var game = StartedWith(Card(5), Card(6)); // reveal 11, deck now empty

        game.HitDealer();                          // must not throw

        Assert.Equal(11, game.Dealer.HandValue);   // reveal ran (face-up-only would be 5)
        Assert.Single(game.Dealer.Hand);
    }

    [Fact]
    public void HitDealer_StandsOnSoftSeventeen()
    {
        var game = StartedWith(Ace(), Card(6));   // reveal A+6 -> 17

        game.HitDealer();

        Assert.Equal(17, game.Dealer.HandValue);
        Assert.Single(game.Dealer.Hand);          // soft 17 stands (this build's rule)
    }

    [Fact]
    public void HitDealer_SoftHand_DemotesAceAndContinues()
    {
        var game = StartedWith(Ace(), Card(3), Card(9), Card(5)); // reveal A+3=14 -> +9: 11+3+9=23 -> demote -> 13 -> +5 -> 18

        game.HitDealer();

        Assert.Equal(18, game.Dealer.HandValue);
        Assert.False(game.Dealer.BustedHand);
        Assert.True(game.Dealer.LowAceHand);
        Assert.Equal(3, game.Dealer.Hand.Count);   // face-up + two hits
    }
}
