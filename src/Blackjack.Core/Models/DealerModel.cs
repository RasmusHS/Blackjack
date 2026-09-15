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
        if (card.Name.Contains("Ace"))
            Ace();
        else
            HandValue += card.Value ?? 0;

        if (HandValue > 21)
            HasBusted = true;
    }

    private void Ace()
    {
        if (HandValue + 11 > 21)
        {
            HandValue += 1;
        }
        else
        {
            HandValue += 11;
        }
    }

    public void CalculateHandValue()
    {
        HandValue = 0;
        Hand.Add(HoleCard);

        foreach (var card in Hand)
        {
            if (card.Name.Contains("Ace"))
            {
                if (HandValue + 11 > 21)
                {
                    HandValue += 1;
                }
                else
                {
                    HandValue += 11;
                }
            }
            else
            {
                HandValue += card.Value ?? 0;
            }
        }
    }

    public List<CardModel> Hand { get; private set; } = new List<CardModel>();
    public CardModel HoleCard { get; private set; }
    public int HandValue { get; set; } = 0;

    public GameModel Game { get; private set; }

    public bool HasBusted { get; set; } = false;
    public bool LowAceHand { get; set; } = false;
    public bool LowAceSplit { get; set; } = false;
}
