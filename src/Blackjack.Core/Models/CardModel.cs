namespace Blackjack.Core.Models;

public class CardModel
{
    public CardModel(string name, int? value, string type)
    {
        bool isAce = type == "Ace";

        if (isAce && value is not null)
            throw new ArgumentException("An Ace must have a null value.", nameof(value));
        if (!isAce && value is null)
            throw new ArgumentException("Only an Ace may have a null value.", nameof(value));

        Name = name;
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Full name of the card, like "King of Hearts".
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// A cards value.
    /// Only the Ace cards has a null value, as they can be either 1 or 11 depending on the hand.
    /// </summary>
    public int? Value { get; private set; }

    /// <summary>
    /// The type of card like "King" or "Ace"
    /// </summary>
    public string Type { get; private set; }
}
