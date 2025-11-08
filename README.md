# Othello

Play [Othello (Reversi)](https://www.worldothello.org/about/about-othello/othello-rules/official-rules/english) or watch bots play against each other.

This is a simple C# implementation of Othello.

Players available:
- Human: Prompts the user to choose a move each turn.
- RandomChoice: Chooses a random valid move.
- Greedy: Chooses the move that flips the most opponent discs.
- MiniMaxV1: A bot that searches for the move sequence that maximizes its score using the MiniMax algorithm.
- MiniMaxV2: Like MiniMaxV1 but uses a position-weight matrix to prefer more valuable squares.
- MiniMaxV3: Faster and smarter, with an improved weight matrix and speed optimizations.
- MiniMaxV4: The strongest bot so far; uses a dynamic weight matrix and a mobility bonus.
Note that weaker bots can occasionally beat stronger ones, especially when playing second.

## Usage
### How to install

Download using Git:
```bash
git clone https://github.com/maorgur/othello.git
cd othello
dotnet build
```

### How to run
```bash
dotnet run --project othello.csproj -- <player1>[-depth] <player2>[-depth]
```
Depth is an optional integer telling a bot how many moves ahead to search (default: 7).

Examples:
```bash
dotnet run --project othello.csproj -- Human Greedy
dotnet run --project othello.csproj -- MiniMaxV2 MiniMaxV4
dotnet run --project othello.csproj -- MiniMaxV1-8 MiniMaxV4-5
```
