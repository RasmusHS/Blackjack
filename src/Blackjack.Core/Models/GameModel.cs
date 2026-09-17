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

    public void StartGame()
    {
        Deck = new DeckModel();
        Dealer = new DealerModel();
        Players = new List<PlayerModel>();
        for (int i = 0; i < PlayerCount; i++)
        {
            Players.Add(new PlayerModel(Id, $"Player {i + 1}", 1000));
        }

        // Cards should be dealt one at a time to each player and then to the dealer,
        // repeating until each player has two cards and the dealer has two cards (one face up and one face down).
        var cardsToDeal = 2 * PlayerCount + 1;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < PlayerCount; j++) // Inner loop to deal cards to each player
            {
                if (Deck.ShuffledDeck.Count > 0)
                {
                    Players[j].InitializeHand(Deck.ShuffledDeck.Pop());
                }
            }

            if (i == 0) // Deal 1 face up card to the dealer, but only on the first iteration of the outer loop
            {
                Dealer.InitializeHand(Deck.ShuffledDeck.Pop());
            }
            else if (i == 1) // Deal 1 face down card to the dealer, but only on the second iteration of the outer loop
            {
                Dealer.InitializeHoleCard(Deck.ShuffledDeck.Pop());
            }
        }
    }

    public void HitPlayer(PlayerModel player)
    {
        if (Deck.ShuffledDeck.Count > 0)
        {
            if (!player.HasSplit)
            {
                player.Hit(Deck.ShuffledDeck.Pop(), null);
            }
            else
            {
                player.Hit(Deck.ShuffledDeck.Pop(), Deck.ShuffledDeck.Pop());
            }
        }
    }

    public void StandPlayer(PlayerModel player)
    {
        player.Stand();
    }

    public void DoubleDownPlayer(PlayerModel player)
    {
        if (Deck.ShuffledDeck.Count > 0)
        {
            if (!player.HasSplit)
            {
                player.DoubleDown(Deck.ShuffledDeck.Pop(), null);
            }
            else
            {
                player.DoubleDown(Deck.ShuffledDeck.Pop(), Deck.ShuffledDeck.Pop());
            }
        }
    }

    public void SplitPlayer(PlayerModel player)
    {
        if (Deck.ShuffledDeck.Count > 0)
        {
            player.Split(Deck.ShuffledDeck.Pop(), Deck.ShuffledDeck.Pop());
        }
    }

    /// <summary>
    /// Dealer has to hit until their hand value is 17 or higher.
    /// If the dealer goes over 21, they bust and the players win.
    /// </summary>
    public void HitDealer()
    {
        if (Deck.ShuffledDeck.Count > 0)
        {
            Dealer.CalculateHandValue();

            while (Dealer.HandValue < 17 && !Dealer.BustedHand) // Keep hitting until the dealer's hand value is 17 or higher, or the dealer busts
            {
                if (Deck.ShuffledDeck.Count == 0) break; // Break the loop if the deck is empty to avoid an exception
                Dealer.Hit(Deck.ShuffledDeck.Pop());
            }
        }
    }

    public Guid Id { get; private set; }
    public int PlayerCount { get; private set; }

    public DeckModel Deck { get; private set; }
    public DealerModel Dealer { get; private set; }
    public List<PlayerModel> Players { get; private set; }
}
