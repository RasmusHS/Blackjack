using Blackjack.Core.Models;

namespace Blackjack.Core.Test.Models;

public class DeckModelTests
{
    private readonly DeckModel _deck = new DeckModel();

    private static int? ExpectedValue(string type) => type switch
    {
        "Ace" => (int?)null,
        "Jack" or "Queen" or "King" => 10,
        _ => int.Parse(type)          // "2".."10"
    };

    [Fact]
    public void UnshuffledDeck_Has52Cards()
    {
        Assert.Equal(52, _deck.UnshuffledDeck.Count);
    }

    [Fact]
    public void UnshuffledDeck_HasFourOfEachRank()
    {
        Assert.Equal(13, _deck.UnshuffledDeck.Select(c => c.Type).Distinct().Count());
        foreach (var rank in _deck.UnshuffledDeck.GroupBy(c => c.Type))
            Assert.Equal(4, rank.Count());
    }

    [Fact]
    public void UnshuffledDeck_AllNamesDistinct()
    {
        Assert.Equal(52, _deck.UnshuffledDeck.Select(c => c.Name).Distinct().Count());
    }

    [Fact]
    public void UnshuffledDeck_ValueMatchesType()
    {
        foreach (var card in _deck.UnshuffledDeck)
            Assert.Equal(ExpectedValue(card.Type), card.Value);
    }

    [Fact]
    public void ShuffledDeck_Has52Cards()
    {
        Assert.Equal(52, _deck.ShuffledDeck.Count);
    }

    [Fact]
    public void ShuffledDeck_IsPermutationOfUnshuffled()
    {
        var expected = _deck.UnshuffledDeck.Select(c => c.Name).OrderBy(n => n);
        var actual = _deck.ShuffledDeck.Select(c => c.Name).OrderBy(n => n);
        Assert.Equal(expected, actual);   // same multiset ⇒ shuffle is a permutation, not a filter
    }

    [Fact]
    public void Draw_RemovesAndReturnsTopCard()
    {
        var top = _deck.ShuffledDeck.Peek();

        var drawn = _deck.Draw();

        Assert.Same(top, drawn);            // returned the top
        Assert.Equal(51, _deck.Count);      // and removed it
        Assert.Equal(51, _deck.ShuffledDeck.Count);
    }
}
