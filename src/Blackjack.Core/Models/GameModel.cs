namespace Blackjack.Core.Models;

public class GameModel
{
    public GameModel(int playerCount)
    {
        Id = Guid.NewGuid();
        PlayerCount = playerCount;
        Deck = new DeckModel();
    }

    public void StartGame()
    {
        Dealer = new DealerModel();
        Players = new List<PlayerModel>();
        for (int i = 0; i < PlayerCount; i++)
        {
            Players.Add(new PlayerModel($"Player {i + 1}", i + 1, 1000));
        }

        // Cards should be dealt one at a time to each player and then to the dealer,
        // repeating until each player has two cards and the dealer has two cards (one face up and one face down).
        var cardsToDeal = 2*PlayerCount+1;
        for (int i = 0; i < cardsToDeal; i++)
        {
            //if (i < PlayerCount && Players[i].Hand.Count < 2)
            //{
            //    Players[i].InitializeHand(Deck.ShuffledDeck.Pop());
            //}
            //else if (i == PlayerCount)
            //{
            //    Dealer.InitializeHand(Deck.ShuffledDeck.Pop());
            //}

        }

        //Dealer.InitializeHand(Deck.ShuffledDeck.Pop(), Deck.ShuffledDeck.Pop());
    }

    public void Hit(PlayerModel player)
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

    public Guid Id { get; private set; }
    public int PlayerCount { get; private set; }

    public DeckModel Deck { get; private set; } 
    public DealerModel Dealer { get; private set; }
    public List<PlayerModel> Players { get; set; }
}
