using Blackjack.Core.Services;

namespace Blackjack.Core.Models;

public class DealerModel
{
    public DealerModel()
    {

    }

    public void InitializeHand(CardModel card)
    {
        if (Hand.Count >= 1)
            throw new InvalidOperationException("The Dealer is only supposed to have 1 face-up card and 1 hole card.");

        Hand.Add(card);

        if (card.Value is null)
            HandValue += 11;
        else
            HandValue += card.Value.Value;
    }

    public void InitializeHoleCard(CardModel card)
    {
        HoleCard = card;
    }

    public void NewRound()
    {
        Hand.Clear();
        HoleCard = null;
        HandValue = 0;
        BustedHand = false;
        HandHasAce = false;
        LowAceHand = false;
    }

    public void Hit(CardModel card)
    {
        Hand.Add(card);
        CalculateHandValue();
    }

    public void CalculateHandValue()
    {
        var cards = HoleCard is not null ? Hand.Append(HoleCard) : Hand;
        (HandValue, HandHasAce, LowAceHand, BustedHand) = HandCalculator.Evaluate(cards);
    }

    public List<CardModel> Hand { get; private set; } = new List<CardModel>();
    public CardModel? HoleCard { get; private set; }
    public int HandValue { get; private set; } = 0;

    public bool BustedHand { get; private set; } = false;
    public bool HandHasAce { get; private set; } = false;
    public bool LowAceHand { get; private set; } = false;
}
