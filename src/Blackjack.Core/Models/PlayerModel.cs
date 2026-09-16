namespace Blackjack.Core.Models;

public class PlayerModel
{
    public PlayerModel(Guid gameId, string name, int points)
    {
        Id = Guid.NewGuid();
        GameId = gameId;
        Name = name;
        Points = points;
    }

    public void InitializeHand(CardModel card)
    {
        SplitPossible = true;
        HasSplit = false;
        EndedTurn = false;
        BustedHand = false;
        BustedSplit = false;
        HandHasAce = false;
        LowAceHand = false;
        SplitHasAce = false;
        LowAceSplit = false;

        HandValue = 0;
        SplitHandValue = 0;

        if (Hand.Count < 2)
            Hand.Add(card);

        if (Hand.Count == 2 && Hand[0].Name.Contains("Ace") && Hand[1].Name.Contains("Ace"))
        {
            HandHasAce = true;
            SplitHasAce = true;

            //Force the player to split and take 2 cards, one for each hand
            HasSplit = true;
            SplitHand.Add(Hand[1]);
            Hand.RemoveAt(1);

            Hand.Add(Game.Deck.ShuffledDeck.Pop());
            SplitHand.Add(Game.Deck.ShuffledDeck.Pop());

            CalculateHandValue();
            Stand(); // End the turn since the player has no choice but to split aces and take 2 cards, one for each hand
        }
        else if (Hand.Count == 2 && (Hand[0].Name.Contains("Ace") || Hand[1].Name.Contains("Ace")))
        {
            HandHasAce = true;
            CalculateHandValue();
        }
        else if (Hand.Count == 2)
            CalculateHandValue();
        else if (Hand.Count > 2)
            throw new InvalidOperationException("Player hand cannot have more than 2 cards at the start of the game.");
    }

    public void Hit(CardModel card, CardModel? splitCard)
    {
        if (!HasSplit)
        {
            Hand.Add(card);
            CalculateHandValue();

            SplitPossible = false; // After hitting, the player can no longer split their hand
        }
        else
        {
            Hand.Add(card);
            SplitHand.Add(splitCard!);

            CalculateHandValue();
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
            Bet *= 2; // Double the bet for the hand
            Points -= Points - (Bet / 2); // Subtract the additional bet from the player's points

            Hand.Add(card);

            CalculateHandValue();
            Stand(); // End the turn after doubling down
        }
        else
        {
            Bet *= 2;
            SplitBet *= 2;
            var totalBet = Bet + SplitBet; // Calculate the total bet for both hands
            Points -= Points - (totalBet / 2); // Subtract the additional bet from the player's points

            Hand.Add(card);
            SplitHand.Add(splitCard!);

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
            SplitPossible = false; // After splitting, the player can no longer split their hand again
        }
    }

    private void CalculateHandValue()
    {
        HandValue = 0; // Reset hand value before calculating

        foreach (var card in Hand) // For each card in the hand, add its value to the hand value
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

        if (HasSplit) // If the player has split, calculate the value of the split hand as well
        {
            SplitHandValue = 0; // Reset split hand value before calculating
            foreach (var card in SplitHand)
            {
                if (card.Name.Contains("Ace"))
                {
                    SplitHasAce = true;
                    SplitHandValue += 11;
                }
                else
                {
                    SplitHandValue += card.Value ?? 0;
                }
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
            BustedHand = true; // If the hand value exceeds 21 and we have already counted an Ace as 1, the player is busted
        }
        else if (HandValue > 21 && !HandHasAce)
        {
            BustedHand = true; // If the hand value exceeds 21 and we don't have an Ace to reduce from 11 to 1, the player is busted
        }


        // If the split hand value exceeds 21 and the split hand contains an Ace, reduce the split hand value by 10 (counting the Ace as 1 instead of 11),
        // but only if we haven't already done so (LowAceSplit is false)
        if (HasSplit && SplitHandValue > 21 && SplitHasAce && !LowAceSplit)
        {
            foreach (var card in SplitHand) // For each ace in the split hand, reduce the split hand value by 10 (counting the Ace as 1 instead of 11)
            {
                if (card.Name.Contains("Ace")) // Check if the card is an Ace
                {
                    SplitHandValue -= 10; // Reduce the split hand value by 10 (counting the Ace as 1 instead of 11)
                }
            }
            LowAceSplit = true; // Set LowAceSplit to true since we are now counting each Ace as 1
        }
        else if (HasSplit && SplitHandValue > 21 && SplitHasAce && LowAceSplit)
        {
            BustedSplit = true; // If the split hand value exceeds 21 and we have already counted an Ace as 1, the player is busted
        }
        else if (HasSplit && SplitHandValue > 21 && !SplitHasAce)
        {
            BustedSplit = true; // If the split hand value exceeds 21 and we don't have an Ace to reduce from 11 to 1, the player is busted
        }

        if (HasSplit && BustedHand && BustedSplit)
        {
            EndedTurn = true; // If both hands are busted, the player has ended their turn
        }
        else if (!HasSplit && BustedHand)
        {
            EndedTurn = true; // If the player is busted and has not split, they have ended their turn
        }
    }

    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }

    public string Name { get; private set; }

    public List<CardModel> Hand { get; private set; } = new List<CardModel>();
    public int HandValue { get; set; } = 0;
    public List<CardModel> SplitHand { get; private set; } = new List<CardModel>();
    public int SplitHandValue { get; set; } = 0;

    public int Points { get; set; } = 1000;
    public int Bet { get; set; } = 0;
    public int SplitBet { get; set; } = 0;

    public GameModel Game { get; set; }

    public bool SplitPossible { get; set; } = true;
    public bool HasSplit { get; set; } = false;
    public bool EndedTurn { get; set; } = false;
    public bool BustedHand { get; set; } = false;
    public bool BustedSplit { get; set; } = false;
    public bool HandHasAce { get; set; } = false;
    public bool LowAceHand { get; set; } = false;
    public bool SplitHasAce { get; set; } = false;
    public bool LowAceSplit { get; set; } = false;
}
