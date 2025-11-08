namespace othello;

public class Human(string name = "Human") : Player(playerName:name)
{
    public override Position GetMove(Board board)
    {
        Position pos = new Position(0, 0);

        do {
            Console.Write("Enter Row (0-7): ");
            pos.X = int.Parse(Console.ReadLine() ?? string.Empty);
            Console.Write("Enter Column (0-7): ");
            pos.Y = int.Parse(Console.ReadLine() ?? string.Empty);
        } while (board.GetCell(pos) != 0);
        return pos;
    }
}