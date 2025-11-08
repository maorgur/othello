namespace othello;

public class RandomChoice(string name="Random") : Player(playerName:name)
{
    private readonly Random _random = new();

    public override Position GetMove(Board board)
    {
        var positions = board.GetFlippableDiscsPerPlacement(Id);
        return positions.ElementAt(_random.Next(0, positions.Count)).Key;
    }
}