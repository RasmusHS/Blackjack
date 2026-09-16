namespace Blackjack.Core.Models;

public class DeckModel
{
    public DeckModel()
    {
        ShuffledDeck = new Stack<CardModel>(UnshuffledDeck.Shuffle());
    }

    public Stack<CardModel> ShuffledDeck { get; private set; }

    public List<CardModel> UnshuffledDeck { get; } = new List<CardModel>
    {
        new CardModel("Ace of Spades", null, "Ace"),
        new CardModel("2 of Spades", 2, "2"),
        new CardModel("3 of Spades", 3, "3"),
        new CardModel("4 of Spades", 4, "4"),
        new CardModel("5 of Spades", 5, "5"),
        new CardModel("6 of Spades", 6, "6"),
        new CardModel("7 of Spades", 7, "7"),
        new CardModel("8 of Spades", 8, "8"),
        new CardModel("9 of Spades", 9, "9"),
        new CardModel("10 of Spades", 10, "10"),
        new CardModel("Jack of Spades", 10, "Jack"),
        new CardModel("Queen of Spades", 10, "Queen"),
        new CardModel("King of Spades", 10, "King"),
        new CardModel("Ace of Diamonds", null, "Ace"),
        new CardModel("2 of Diamonds", 2, "2"),
        new CardModel("3 of Diamonds", 3, "3"),
        new CardModel("4 of Diamonds", 4, "4"),
        new CardModel("5 of Diamonds", 5, "5"),
        new CardModel("6 of Diamonds", 6, "6"),
        new CardModel("7 of Diamonds", 7, "7"),
        new CardModel("8 of Diamonds", 8, "8"),
        new CardModel("9 of Diamonds", 9, "9"),
        new CardModel("10 of Diamonds", 10, "10"),
        new CardModel("Jack of Diamonds", 10, "Jack"),
        new CardModel("Queen of Diamonds", 10, "Queen"),
        new CardModel("King of Diamonds", 10, "King"),
        new CardModel("King of Clubs", 10, "King"),
        new CardModel("Queen of Clubs", 10, "Queen"),
        new CardModel("Jack of Clubs", 10, "Jack"),
        new CardModel("10 of Clubs", 10, "10"),
        new CardModel("9 of Clubs", 9, "9"),
        new CardModel("8 of Clubs", 8, "8"),
        new CardModel("7 of Clubs", 7, "7"),
        new CardModel("6 of Clubs", 6, "6"),
        new CardModel("5 of Clubs", 5, "5"),
        new CardModel("4 of Clubs", 4, "4"),
        new CardModel("3 of Clubs", 3, "3"),
        new CardModel("2 of Clubs", 2, "2"),
        new CardModel("Ace of Clubs", null, "Ace"),
        new CardModel("King of Hearts", 10, "King"),
        new CardModel("Queen of Hearts", 10, "Queen"),
        new CardModel("Jack of Hearts", 10, "Jack"),
        new CardModel("10 of Hearts", 10, "10"),
        new CardModel("9 of Hearts", 9, "9"),
        new CardModel("8 of Hearts", 8, "8"),
        new CardModel("7 of Hearts", 7, "7"),
        new CardModel("6 of Hearts", 6, "6"),
        new CardModel("5 of Hearts", 5, "5"),
        new CardModel("4 of Hearts", 4, "4"),
        new CardModel("3 of Hearts", 3, "3"),
        new CardModel("2 of Hearts", 2, "2"),
        new CardModel("Ace of Hearts", null, "Ace")
    };
}
