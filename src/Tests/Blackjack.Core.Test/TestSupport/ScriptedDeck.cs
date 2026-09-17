using Blackjack.Core.Models;
using Blackjack.Core.Services;

namespace Blackjack.Core.Test.TestSupport;

public class ScriptedDeck : IDeck
{
    private readonly Queue<CardModel> _cards;

    public ScriptedDeck(IEnumerable<CardModel> cards) => _cards = new(cards);

    public int Count => _cards.Count;

    public CardModel Draw() => _cards.Dequeue();
}
