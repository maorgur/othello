namespace othello;

public abstract class Player(int playerId=0, string playerName="Player", string playerSymbol="X") //Default placeholders
{
    public int Id {get; set;} = playerId;
    public string Name { get; set;} = playerName;
    public string Symbol {get; set;} = playerSymbol;

    public abstract Position GetMove(Board board);

    public override string ToString()
    {
        return $"{Name}: {Symbol} (ID: {Id})";
    }
}