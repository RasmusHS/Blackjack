using Blackjack.Core.Models;

namespace Blackjack.Core.Test.Models;

public class CardModelTests
{
    [Theory]
    [InlineData("Two of Clubs", 2, "2")]
    [InlineData("Queen of Diamonds", 10, "Queen")]
    [InlineData("Ace of Hearts", null, "Ace")]
    public void Ctor_AssignsProperties_AcrossCardKinds(string name, int? value, string type)
    {
        var card = new CardModel(name, value, type);

        Assert.Equal(name, card.Name);
        Assert.Equal(value, card.Value);
        Assert.Equal(type, card.Type);
    }
}
