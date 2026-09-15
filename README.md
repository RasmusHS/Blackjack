# Blackjack
This project is the game Blackjack in CLI. 

## Requirements

### Must-Have
- At least 1 player besides the dealer.
- Calculate a player's hand value everytime they get a card.
- Automatically decide whether aces are an 11 or a 1.
- Classic Blackjack moves:
    - Hit: Take another card to add to your total.
    - Stand/Stay: Keep your current total. End your turn.
    - Double Down: Double your bet, take one final card, then stand.
    - Split: If you have a pair, split into two hands by placing a second bet equal to your first.
    - Insurance: Offered when dealer’s face-up card is an ace.
        - Player’s may place an insurance bet equal to half of your original bet.
        - This bet only pays out if the dealer’s hidden card is a face card. 
        - The outcome of the insurance bet is separate from the outcome of the players’ main hand.
- Let player's moves be combined like in real life (like split then double).
- Cards dealt out 1 at a time until all players have 2 card and dealer has 1 face-up card and 1 hole card.
- 95% test coverage (block based) to make the manager happy. Also gives devs something to point at to deny that the software is buggy and unstable to avoid having to deal with legacy code.

### Should-Have
- Persistence store.
    - A db for storing past games and their highscores.
    - Seperate class library for reduced coupling, as well as a some flexibility with choice of db store and type.
- Update ruleset to prevent double or hits on split aces, as well as other common casino rules.
- (Dumb) AI players that just take random moves until they stand, double, or bust.
    - Human player can select how many AI players will be present up to a certain maximum.

### Could-Have
- Game statistics based on data from persistence store.
- Multiple human players, playing over local network.
    - Host can set amount of AI players.
    - Dealer lives on host's machine.

### Won't'Have
- Visual UI: Far beyond scope and intend of the exercise.
    - Reworked to use Godot .NET would have been the path.
    - Would have used a collection of free assets from an asset store.

