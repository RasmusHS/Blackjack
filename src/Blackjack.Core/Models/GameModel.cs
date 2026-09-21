using System.ComponentModel.DataAnnotations.Schema;
using Blackjack.Core.Services;

namespace Blackjack.Core.Models;

public class GameModel
{
    internal GameModel() { } // For EF Core

    public GameModel(DealerModel dealer, List<PlayerModel> players)
    {
        Id = Guid.NewGuid();
        Dealer = dealer;
        Players = players;
        PlayerCount = Players.Count;

        foreach (var player in Players)
            player.GameId = Id;
    }

    public void StartGame(IDeck? deck = null)
    {
        Deck = deck ?? new DeckModel();

        DealCards();
    }

    public void NewRound(DealerModel dealer, List<PlayerModel> players, IDeck? deck = null)
    {
        var newDeck = deck ?? new DeckModel();
        if (newDeck.Count is not 52)
            throw new InvalidOperationException("The deck must have exactly 52 cards.");
        if (players.Any(p => p.Points < BaseValues.Bet))
            throw new InvalidOperationException("A player cannot cover the bet.");

        Deck = newDeck;
        Dealer = dealer;
        Players = players;

        foreach (var player in Players)
            player.GameId = Id;

        Dealer.NewRound();
        foreach (var player in Players)
        {
            player.NewRound(BaseValues.Bet); // Initialize each player with the base bet for the new round. Change BaseValues.Bet to a specific bet value if desired.
        }

        DealCards();
    }

    private void DealCards()
    {
        // Cards should be dealt one at a time to each player and then to the dealer,
        // repeating until each player has two cards and the dealer has two cards (one face up and one face down).
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < PlayerCount; j++) // Inner loop to deal cards to each player
            {
                if (Deck.Count > 0)
                {
                    Players[j].InitializeHand(Deck.Draw());
                }
            }

            if (i == 0) // Deal 1 face up card to the dealer, but only on the first iteration of the outer loop
            {
                Dealer.InitializeHand(Deck.Draw());
            }
            else if (i == 1) // Deal 1 face down card to the dealer, but only on the second iteration of the outer loop
            {
                Dealer.InitializeHoleCard(Deck.Draw());
            }
        }
    }

    public void Result(DealerModel dealer, List<PlayerModel> players)
    {
        bool dealerBust = dealer.BustedHand || dealer.HandValue > 21;

        foreach (var player in players) 
        {
            bool mainAlive = !player.BustedHand && player.HandValue <= 21;

            if (mainAlive)
            {
                if (dealerBust || player.HandValue > dealer.HandValue)
                    player.Points += player.IsBlackjack
                        ? (int)Math.Round(player.Bet * 2.5, MidpointRounding.AwayFromZero)
                        : player.Bet * 2;
                else if (player.HandValue == dealer.HandValue)
                    player.Points += player.Bet; // push
            }
            player.Bet = 0;

            // Split hand — never a natural blackjack
            if (player.HasSplit)
            {
                bool splitAlive = !player.BustedSplit && player.SplitHandValue <= 21;
                if (splitAlive)
                {
                    if (dealerBust || player.SplitHandValue > dealer.HandValue)
                        player.Points += player.SplitBet * 2;
                    else if (player.SplitHandValue == dealer.HandValue)
                        player.Points += player.SplitBet; // push
                }
                player.SplitBet = 0;
            }
        }
    }

    public void HitPlayer(PlayerModel player)
    {
        if (Deck.Count > 0)
            player.Hit(Deck.Draw());
    }

    public void StandPlayer(PlayerModel player)
    {
        player.Stand();
    }

    public void DoubleDownPlayer(PlayerModel player)
    {
        if (player.CanDoubleDown && Deck.Count > 0)
            player.DoubleDown(Deck.Draw());
    }

    public void SplitPlayer(PlayerModel player)
    {
        if (player.SplitPossible && Deck.Count >= 2)
            player.Split(Deck.Draw(), Deck.Draw());
    }

    /// <summary>
    /// Dealer has to hit until their hand value is 17 or higher.
    /// If the dealer goes over 21, they bust and the players win.
    /// </summary>
    public void HitDealer()
    {
        Dealer.CalculateHandValue();

        while (Dealer.HandValue < 17 && !Dealer.BustedHand && Deck.Count > 0) // Keep hitting until the dealer's hand value is 17 or higher, or the dealer busts
        {
            Dealer.Hit(Deck.Draw());
        }
    }

    public Guid Id { get; private set; }
    public int PlayerCount { get; private set; }

    [NotMapped] public IDeck Deck { get; private set; }
    public DealerModel Dealer { get; private set; }
    public List<PlayerModel> Players { get; private set; }
}
