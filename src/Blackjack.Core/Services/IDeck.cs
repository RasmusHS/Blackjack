using Blackjack.Core.Models;

namespace Blackjack.Core.Services;

public interface IDeck
{
    int Count { get; }
    CardModel Draw();
}
