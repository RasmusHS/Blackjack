namespace Blackjack.Core.Models;

public class CardModel
{
    public CardModel(string name, int? value, string type)
    {
        Name = name;
        Value = value;
        Type = type;
    }

    public string Name { get; private set; }

    /// <summary>
    /// A cards value.
    /// Only the Ace cards has a null value, as they can be either 1 or 11 depending on the hand.
    /// </summary>
    public int? Value { get; private set; }
    public string Type { get; private set; }
}
