using Blackjack.Core.Models;
using Blackjack.Core.Services;

namespace Blackjack.Core.Test.Services;

public class HandCalculatorTests
{
    private static CardModel Card(int v) => new($"{v} of Spades", v, v.ToString());
    private static CardModel Ace() => new("Ace of Spades", null, "Ace");

    [Fact]
    public void Hard_SumsValues()
        => Assert.Equal(19, HandCalculator.Evaluate(new[] { Card(10), Card(9) }).value);

    [Fact]
    public void SoftUnder21_KeepsAceHigh()
    {
        var (value, _, lowAce, _) = HandCalculator.Evaluate(new[] { Ace(), Card(6) });
        Assert.Equal(17, value);
        Assert.False(lowAce);
    }

    [Fact]
    public void SoftOver21_DemotesOneAce()
    {
        var (value, _, lowAce, busted) = HandCalculator.Evaluate(new[] { Ace(), Card(6), Card(10) });
        Assert.Equal(17, value);        // 27 -> 17
        Assert.True(lowAce);
        Assert.False(busted);
    }

    [Fact]
    public void TwoAces_DemotesOnlyOne()
        => Assert.Equal(21, HandCalculator.Evaluate(new[] { Ace(), Ace(), Card(9) }).value);

    [Fact]
    public void AllAcesDemoted_StillBusts()
    {
        var (value, _, _, busted) = HandCalculator.Evaluate(new[] { Card(10), Card(10), Ace(), Ace() });
        Assert.Equal(22, value);        // 10+10+1+1
        Assert.True(busted);
    }

    [Fact]
    public void HasAce_SetForAnyAce()
        => Assert.True(HandCalculator.Evaluate(new[] { Ace(), Card(5) }).hasAce);

    [Fact]
    public void EmptyHand_IsZeroAndNotBusted()
        => Assert.Equal((0, false, false, false), HandCalculator.Evaluate(Array.Empty<CardModel>()));
}
