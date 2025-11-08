namespace othello;

public class Greedy(string name = "Greedy") : Player(playerName:name)
{
    //Greedy will play the move that will flip the most amount of discs.
    public override Position GetMove(Board board)
    {
        Dictionary<Position, HashSet<Position>> positions = board.GetFlippableDiscsPerPlacement(Id);
        Position bestPos = new Position(0, 0);
        int bestScore = 0;

        foreach (var pos in positions)
        {
            if (pos.Value.Count > bestScore)
            {
                bestPos = pos.Key;
                bestScore = pos.Value.Count;
            }
        }
        return bestPos;
    }
}