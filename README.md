# Blackjack
This project is the game Blackjack in CLI. 

## Usage

1. Make sure docker and wsl is installed.
    ```pwsh
    wsl.exe --install
    ```
    [Docker Desktop](https://www.docker.com/products/docker-desktop/)
2. Clone or download this project. And unzip if downloaded.
3. Make sure Docker is running.
4. To play:
    - On Windows: Double click "play.cmd"
    - On Linux and Mac: Double click "play.sh"

<br>
The 2 scripts runs the following line in your terminal: 

```
docker compose run --rm blackjack
```

## Controls
- Hit: ```H```
- Stand: ```S```
- Double Down: ```D```
- Split: ```P```
- Continue: ```C```
- New Game: ```N```
- Quit: ```Q```

## Requirements

- [x] At least 1 player besides the dealer.
- [x] Calculate a player's hand value everytime they get a card.
- [x] Automatically decide whether aces are an 11 or a 1.
- [x] Classic Blackjack moves:
    - [x] Hit: Take another card to add to your total.
    - [x] Stand/Stay: Keep your current total. End your turn.
    - [x] Double Down: Double your bet, take one final card, then stand.
    - [x] Split: If you have a pair, split into two hands by placing a second bet equal to your first.
- [x] Cards dealt out 1 at a time until all players have 2 card and dealer has 1 face-up card and 1 hole card.
- [x] 95% test coverage (block based) to make the manager happy. Also gives devs something to point at to deny that the software is buggy and unstable to avoid having to deal with legacy code.

## Documentation

### Class Diagram

![V2]()

### Entity Relation Diagram

![V2]()
