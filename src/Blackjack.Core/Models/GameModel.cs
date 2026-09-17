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
    }

    public void StartGame(IDeck? deck = null)
    {
        Deck = deck ?? new DeckModel();
        Dealer = new DealerModel();
        Players = new List<PlayerModel>();
        for (int i = 0; i < PlayerCount; i++)
        {
            Players.Add(new PlayerModel(Id, $"Player {i + 1}", 1000));
        }

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

    public void HitPlayer(PlayerModel player)
    {
        if (!player.HasSplit)
        {
            if (Deck.Count > 0)
                player.Hit(Deck.Draw(), null);
        }
        else
        {
            var main = !player.BustedHand && Deck.Count > 0 ? Deck.Draw() : null;
            var split = !player.BustedSplit && Deck.Count > 0 ? Deck.Draw() : null;
            player.Hit(main, split);
        }
    }

    public void StandPlayer(PlayerModel player)
    {
        player.Stand();
    }

    public void DoubleDownPlayer(PlayerModel player)
    {
        if (player.CanDoubleDown && Deck.Count > 0)
            player.DoubleDown(Deck.Draw());
        //if (!player.HasSplit && player.CanDoubleDown)
        //{
        //    if (Deck.Count > 0)
        //        player.DoubleDown(Deck.Draw(), null);
        //}
        //else
        //{
        //    var main = !player.BustedHand && Deck.Count > 0 && player.CanDoubleDown ? Deck.Draw() : null;
        //    var split = !player.BustedSplit && Deck.Count > 0 && player.CanDoubleDown ? Deck.Draw() : null;
        //    player.DoubleDown(main, split);
        //}
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
