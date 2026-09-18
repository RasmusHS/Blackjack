using Blackjack.Core.Models;

namespace Blackjack.Core.Services;

public static class HandCalculator
{
    public static (int value, bool hasAce, bool lowAce, bool busted) Evaluate(IEnumerable<CardModel> cards)
    {
        int value = 0, aces = 0;
        bool hasAce = false;

        foreach (var card in cards)
        {
            if (card.Value is null) 
            { 
                hasAce = true; 
                aces++; 
                value += 11; 
            }
            else 
            { 
                value += card.Value.Value; 
            }
        }

        bool lowAce = false;
        while (value > 21 && aces > 0)
        {
            value -= 10;
            aces--;
            lowAce = true;
        }

        return (value, hasAce, lowAce, value > 21);
    }
}
