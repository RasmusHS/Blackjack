using Blackjack.Core.Services;

namespace Blackjack.Core.Models;

public class PlayerModel
{
    public PlayerModel(Guid gameId, string name, int? points)
    {
        Id = Guid.NewGuid();
        GameId = gameId;
        Name = name;
        if (points is null || points <= 0)
            Points = BaseValues.InitialPlayerBalance;
        else
            Points = (int)points;

        Bet = BaseValues.Bet;
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

    public void NewRound(int bet)
    {
        Hand.Clear();
        HandValue = 0;
        SplitHand.Clear();
        SplitHandValue = 0;
        HasSplit = false;
        EndedTurn = false;
        BustedHand = false;
        BustedSplit = false;
        HandHasAce = false;
        LowAceHand = false;
        SplitHasAce = false;
        LowAceSplit = false;
        Bet = bet;
        Points -= Bet;
        SplitBet = 0;
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
        (HandValue, HandHasAce, LowAceHand, BustedHand) = HandCalculator.Evaluate(Hand);

        if (HasSplit)
            (SplitHandValue, SplitHasAce, LowAceSplit, BustedSplit) = HandCalculator.Evaluate(SplitHand);

        if (HasSplit)
        {
            if (BustedHand && BustedSplit) EndedTurn = true;   // both hands dead = fully bust
        }
        else if (BustedHand)
        {
            EndedTurn = true;
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
    public bool IsBlackjack => !HasSplit && Hand.Count == 2 && HandValue == 21;
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
