namespace Blackjack.Core.Models;

public class PlayerModel
{
    public PlayerModel(Guid gameId, string name, int points)
    {
        Id = Guid.NewGuid();
        GameId = gameId;
        Name = name;
        Points = points;

        Bet = 50;
        Points -= Bet;
    }

    public void InitializeHand(CardModel card)
    {
        if (Hand.Count >= 2)
            throw new InvalidOperationException("Player hand cannot have more than 2 cards at the start of the game.");

        Hand.Add(card);

        if (Hand.Count == 2)
            CalculateHandValue();   // aces (incl. a pair) flow through here; no forced split, no deck reach

    }

    public void Hit(CardModel? card, CardModel? splitCard)
    {
        if (!HasSplit)
        {
            if (card is not null)
                Hand.Add(card);
        }
        else
        {
            if (card is not null && !BustedHand) Hand.Add(card);
            if (splitCard is not null && !BustedSplit) SplitHand.Add(splitCard);
        }

        CalculateHandValue();
    }

    public void Stand()
    {
        EndedTurn = true;
    }

    public void DoubleDown(CardModel card)
    {
        if (CanDoubleDown)
        {
            Bet *= 2; // Double the bet for the hand
            Points -= Bet / 2; // Subtract the additional bet from the player's points

            Hand.Add(card);

            CalculateHandValue();
            Stand(); // End the turn after doubling down
        }
    }

    public void Split(CardModel card1, CardModel card2)
    {
        // Check if the player has two cards of the same type (e.g., two 8s)
        // And only has their initial two cards in their hand
        if (Hand.Count == 2 && Hand[0].Type == Hand[1].Type && SplitPossible)
        {
            SplitHand.Add(Hand[1]);
            Hand.RemoveAt(1);

            Hand.Add(card1);
            SplitHand.Add(card2);

            HasSplit = true;

            SplitBet = Bet;
            Points -= Bet;

            CalculateHandValue();
        }
    }

    private void CalculateHandValue()
    {
        (HandValue, HandHasAce, LowAceHand, BustedHand) = Evaluate(Hand);

        if (HasSplit)
            (SplitHandValue, SplitHasAce, LowAceSplit, BustedSplit) = Evaluate(SplitHand);

        if (HasSplit)
        {
            if (BustedHand && BustedSplit) EndedTurn = true;   // both hands dead = fully bust
        }
        else if (BustedHand)
        {
            EndedTurn = true;
        }

        static (int value, bool hasAce, bool lowAce, bool busted) Evaluate(List<CardModel> cards)
        {
            int value = 0, aces = 0;
            bool hasAce = false;

            foreach (var card in cards)
            {
                if (card.Value is null) { hasAce = true; aces++; value += 11; }   // ace
                else { value += card.Value.Value; }
            }

            bool lowAce = false;
            while (value > 21 && aces > 0)   // demote one ace at a time, only as far as needed
            {
                value -= 10;
                aces--;
                lowAce = true;
            }

            return (value, hasAce, lowAce, value > 21);
        }
    }

    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }

    public string Name { get; private set; }

    public List<CardModel> Hand { get; private set; } = new List<CardModel>();
    public int HandValue { get; private set; } = 0;
    public List<CardModel> SplitHand { get; private set; } = new List<CardModel>();
    public int SplitHandValue { get; private set; } = 0;

    public int Points { get; set; } = 1000;
    public int Bet { get; set; } = 0;
    public int SplitBet { get; set; } = 0;

    public bool SplitPossible => !HasSplit && Hand.Count == 2 && Hand[0].Type == Hand[1].Type;
    public bool CanDoubleDown => !HasSplit && Hand.Count == 2;
    public bool HasSplit { get; private set; } = false;
    public bool EndedTurn { get; set; } = false;
    public bool BustedHand { get; private set; } = false;
    public bool BustedSplit { get; private set; } = false;
    public bool HandHasAce { get; private set; } = false;
    public bool LowAceHand { get; private set; } = false;
    public bool SplitHasAce { get; private set; } = false;
    public bool LowAceSplit { get; private set; } = false;

    public GameModel Game { get; set; }
}
