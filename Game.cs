namespace othello;

public class Game
{
    private readonly Player _player1;
    private readonly Player _player2;

    private int _currentPlayerId = 1;
    private long _player1Clock, _player2Clock;
    public Board Board { get; }
    
    public Game(Player[] players)
    {
        if (players[0].Id == 0 || players[1].Id == 0)
        {
            throw new ArgumentException("Players must have a valid ID.");
        }
        _player1 = players[0];
        _player2 = players[1];
        Board = new Board();
    }
    

    // Returns true if a move was made; false if game cannot continue
    public bool MakeMove()
    {
        if (Board.IsFinished()) return false;

        var currentPlayer = _currentPlayerId == 1 ? _player1 : _player2;
        if (Board.GetFlippableDiscsPerPlacement(currentPlayer.Id).Count == 0) //if there is no move that will flip any enemy discs, skip a turn
        {
            Console.WriteLine("SKIPPED TURN!");
            _currentPlayerId = _currentPlayerId == 1 ? 2 : 1;
            return false;
        }

        var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var move = currentPlayer.GetMove(Board);
        var elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime;
        if (currentPlayer.Id == 1)
        {
            _player1Clock += elapsedTime;
        }
        else
        {
            _player2Clock += elapsedTime;
        }
        
        var success = Board.PlaceDisc(move, currentPlayer.Id, true);
        if (!success)
        {
            Console.WriteLine($"Invalid move at {move} by player {currentPlayer.Id}.");
            return false;
        }
        Console.WriteLine($"{currentPlayer.Name} placed at {move}.");
        

        _currentPlayerId = _currentPlayerId == 1 ? 2 : 1;
        return true;
    }

    public (long player1Clock, long player2Clock) GetClock()
    {
        return (_player1Clock, _player2Clock);
    }

    public new string ToString()
    {
        var result = $"Basic evaluation: {Board.GetEvaluation()}\n";
        var currentPlayer = _currentPlayerId == 1 ? _player1 : _player2;
        result += $"Current player: {currentPlayer}\n\n";
        result += "  ";
        for (var i = 0; i < 8; i++)
        {
            result += $" {i.ToString()}";
        }

        result += "\n";
        var row = 0;
        foreach (var line in Board.ToString(_player1, _player2, _currentPlayerId).Split("\n"))
        {
            result += $"{row} {line}\n";
            row++;
        }
        return result;
    }
}