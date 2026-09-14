namespace Blackjack.Core.Models;

public class PlayerModel
{
    public PlayerModel(string name, int order, int points)
    {
        Id = Guid.NewGuid();
        Name = name;
        Order = order;
        Points = points;
    }

    public void InitializeHand(CardModel card)
    {
        Hand.Add(card);
        if (Hand.Count == 2)
            CalculateHandValue();
    }

    public void Hit(CardModel card, CardModel? splitCard)
    {
        if (!HasSplit)
        {
            if (!card.Name.Contains("Ace"))
            {
                Hand.Add(card);
                HandValue += card.Value ?? 0;
            }  
            else
            {
                Hand.Append(card);
                Ace();
            }
                
        }
        else
        {
            Hand.Add(card);
            SplitHand.Add(splitCard!);

            if (card.Name.Contains("Ace") || splitCard!.Name.Contains("Ace"))
                Ace();
            else
            {
                HandValue += card.Value ?? 0;
                SplitHandValue += splitCard!.Value ?? 0;
            }
        }

        if (HandValue > 21 && !HasSplit)
        {
            HasBusted = true;
            EndedTurn = true;
        }
        else if (HasSplit)
        {
            if (HandValue > 21 && SplitHandValue > 21)
            {
                HasBusted = true;
                EndedTurn = true;
            }
        }
    }

    public void Stand()
    {
        EndedTurn = true;
    }

    public void DoubleDown(CardModel card, CardModel? splitCard)
    {
        if (!HasSplit)
        {
            Bet *= 2;
            Points -= Points - (Bet / 2);

            Hand.Add(card);

            if (card.Name.Contains("Ace"))
                Ace();
            else
                HandValue += card.Value ?? 0;
        }
        else 
        {
            Bet *= 2;
            SplitBet *= 2;
            var totalBet = Bet + SplitBet;
            Points -= Points - (totalBet / 2);

            Hand.Add(card);
            SplitHand.Add(splitCard!);

            if (card.Name.Contains("Ace") || splitCard!.Name.Contains("Ace"))
                Ace();
            else
            {
                HandValue += card.Value ?? 0;
                SplitHandValue += splitCard!.Value ?? 0;
            }
        }
    }

    public void Split(CardModel card1, CardModel card2)
    {
        SplitHand.Add(Hand[1]);
        Hand.RemoveAt(1);

        Hand.Add(card1);
        SplitHand.Add(card2);

        HasSplit = true;

        if (card1.Name.Contains("Ace") || card2.Name.Contains("Ace"))
            Ace();
        else
        {
            HandValue += card1.Value ?? 0;
            SplitHandValue += card2.Value ?? 0;
        }

        SplitBet = Bet;
        Points -= Bet;
    }

    private void Ace()
    {
        if (!HasSplit)
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
            if (HandValue + 11 > 21)
            {
                HandValue += 1;
            }
            else
            {
                HandValue += 11;
            }

            if (SplitHandValue + 11 > 21)
            {
                SplitHandValue += 1;
            }
            else
            {
                SplitHandValue += 11;
            }
        }
    }

    private void CalculateHandValue()
    {
        if (!HasSplit)
        {
            HandValue = 0;
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

        HandValue = 0;
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

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public List<CardModel> Hand { get; private set; } = new List<CardModel>();
    public int HandValue { get; set; } = 0;
    public List<CardModel> SplitHand { get; private set; } = new List<CardModel>();
    public int SplitHandValue { get; set; } = 0;

    public int Order { get; private set; } = 0;

    public int Points { get; set; } = 1000;
    public int Bet { get; set; } = 0;
    public int SplitBet { get; set; } = 0;

    public bool HasSplit { get; set; } = false;
    public bool EndedTurn { get; set; } = false;
    public bool HasBusted { get; set; } = false;
}
