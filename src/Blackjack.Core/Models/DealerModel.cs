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

    public void Hit(CardModel card)
    {
        Hand.Add(card);
        CalculateHandValue();
    }

    public void CalculateHandValue()
    {
        HandValue = 0;
        HandHasAce = false;
        LowAceHand = false;
        BustedHand = false;

        int aces = 0;

        IEnumerable<CardModel> cards = Hand;

        if (HoleCard is not null) cards = cards.Append(HoleCard);   // counted, not added to Hand

        foreach (var card in cards)
        {
            if (card.Value is null)    // ace — only aces have null Value, per CardModel
            {
                HandHasAce = true;
                aces++;
                HandValue += 11;
            }
            else
            {
                HandValue += card.Value.Value;
            }
        }

        while (HandValue > 21 && aces > 0)   // demote one ace at a time, minimum needed
        {
            HandValue -= 10;
            aces--;
            LowAceHand = true;
        }

        if (HandValue > 21)
            BustedHand = true;
    }

    public List<CardModel> Hand { get; private set; } = new List<CardModel>();
    public CardModel HoleCard { get; private set; }
    public int HandValue { get; set; } = 0;

    public bool BustedHand { get; private set; } = false;
    public bool HandHasAce { get; private set; } = false;
    public bool LowAceHand { get; private set; } = false;
}
