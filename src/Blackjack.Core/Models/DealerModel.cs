namespace Blackjack.Core.Models;

public class DealerModel
{
    public DealerModel()
    {

    }

    public void InitializeHand(CardModel card)
    {
        Hand.Add(card);
    }

    public void InitializeHoleCard(CardModel card)
    {
        HoleCard = card;
    }

    public void Hit(CardModel card)
    {
        Hand.Add(card);
        CalculateHandValue();

        if (HandValue > 21)
            BustedHand = true;
    }

    public void CalculateHandValue()
    {
        HandValue = 0;
        Hand.Add(HoleCard);

        foreach (var card in Hand)
        {
            if (card.Name.Contains("Ace"))
            {
                HandHasAce = true;
                HandValue += 11;
            }
            else
            {
                HandValue += card.Value ?? 0;
            }
        }

        // If the hand value exceeds 21 and the hand contains an Ace, reduce the hand value by 10 (counting the Ace as 1 instead of 11),
        // but only if we haven't already done so (LowAceHand is false)
        if (HandValue > 21 && HandHasAce && !LowAceHand)
        {
            foreach (var card in Hand) // For each ace in the hand, reduce the hand value by 10 (counting the Ace as 1 instead of 11)
            {
                if (card.Name.Contains("Ace")) // Check if the card is an Ace
                {
                    HandValue -= 10; // Reduce the hand value by 10 (counting the Ace as 1 instead of 11)
                }
            }
            LowAceHand = true; // Set LowAceHand to true since we are now counting each Ace as 1
        }
        else if (HandValue > 21 && HandHasAce && LowAceHand)
        {
            BustedHand = true; // If the hand value exceeds 21 and we have already counted an Ace as 1, the dealer is busted
        }
        else if (HandValue > 21 && !HandHasAce)
        {
            BustedHand = true; // If the hand value exceeds 21 and we don't have an Ace to reduce from 11 to 1, the dealer is busted
        }
    }

    public List<CardModel> Hand { get; private set; } = new List<CardModel>();
    public CardModel HoleCard { get; private set; }
    public int HandValue { get; set; } = 0;

    public bool BustedHand { get; set; } = false;
    public bool HandHasAce { get; set; } = false;
    public bool LowAceHand { get; set; } = false;
}
