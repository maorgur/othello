// ReSharper disable NonReadonlyMemberInGetHashCode
namespace othello;

public class Board
{
    private int[,] _cells = new int[8, 8];

    //Cache which moves can be played at the current state for each player
    private Dictionary<Position, HashSet<Position>> _flippableDiscsAtEachPosForPlayer1 = new Dictionary<Position, HashSet<Position>>(7); //7 feels like a good number
    private Dictionary<Position, HashSet<Position>> _flippableDiscsAtEachPosForPlayer2 = new Dictionary<Position, HashSet<Position>>(7);
    public int PlacedDiscs {get; private set;}
    public Board(bool setUpDiscs = true)
    {
        if (setUpDiscs)
        {
            //optimization: if we clone a board, no need to set the starting discs
            Reset();
        }

        _flippableDiscsAtEachPosForPlayer1.Clear();
        _flippableDiscsAtEachPosForPlayer2.Clear();
    }
    
    public bool PlaceDisc(Position pos, int player, bool doFlips = false)
    {
        _cells[pos.X, pos.Y] = player;
        PlacedDiscs++;

        if (doFlips)
        {
            foreach (Position flippingPos in GetFlippableDiscs(_cells, pos.X, pos.Y, player == 1 ? _flippableDiscsAtEachPosForPlayer1 : _flippableDiscsAtEachPosForPlayer2))
            {
                _cells[flippingPos.X, flippingPos.Y] = player;
            }
        }
        //new board, new moves. clean cache
        _flippableDiscsAtEachPosForPlayer1.Clear();
        _flippableDiscsAtEachPosForPlayer2.Clear();
        return true;
    }

    
    public int GetCell(Position pos)
    {
        return _cells[pos.X, pos.Y];
    }
    
    public bool IsFinished()
    {
        //there are 3 ways to end a game, this function will only check for 2 that are fast to find: one of the players doesn't have any discs, or the board is full
        if (CountDiscs(1) == 0 || CountDiscs(2) == 0){return true;}

        return PlacedDiscs >= 64;
    }

    public int CountDiscs(int player)
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (_cells[i, j] == player)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public int GetEvaluation()
    {
        //simple func to get the current evaluation of the board, if one of the players doesn't have any discs at all, return as the other one has infinite points.
         int player1Discs = CountDiscs(1);
         int player2Discs = CountDiscs(2);

         if (player1Discs == 0)
         {
             return int.MinValue;
         }

         if (player2Discs == 0)
         {
             return int.MaxValue;
         }
         
         return (player1Discs - player2Discs);
    }
    
    public int GetEvaluation(int[,] weights)
    {
        //better func to find the current eval, will give different weight to each disc based on the weights matrix
        int player1Discs = 0;
        int player2Discs = 0;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (_cells[x,y] == 1)
                {
                    player1Discs += weights[x, y];
                } else if (_cells[x,y] == 2)
                {
                    player2Discs += weights[x, y];
                }
            }
        }

        if (player1Discs == 0)
        {
            return int.MinValue;
        } else if (player2Discs == 0)
        {
            return int.MaxValue;
        }
         
        return player1Discs - player2Discs;
    }
    
    public int GetWinner()
    {
        int player1Discs = CountDiscs(1);
        int player2Discs = CountDiscs(2);
        if (player1Discs > player2Discs)
        {
            return 1;
        }
        if (player2Discs > player1Discs)
        {
            return 2;
        }
        return 0; // Draw
    }

    private void Reset()
    {
        _cells = new int[8, 8];
        _cells[3, 3] = 2;
        _cells[4, 4] = 2;
        _cells[3, 4] = 1;
        _cells[4, 3] = 1;

        _flippableDiscsAtEachPosForPlayer1.Clear();
        _flippableDiscsAtEachPosForPlayer2.Clear();

        PlacedDiscs = 4;
    }

    public Board Clone(bool cloneCache = false)
    {
        Board board = new Board(false);
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                board._cells[x,y] =  _cells[x,y];
            }
        }

        board.PlacedDiscs = PlacedDiscs;

        if (cloneCache)
        {
            //deep copy
            board._flippableDiscsAtEachPosForPlayer1 = new Dictionary<Position, HashSet<Position>>(_flippableDiscsAtEachPosForPlayer1.Capacity);
            board._flippableDiscsAtEachPosForPlayer2 = new Dictionary<Position, HashSet<Position>>(_flippableDiscsAtEachPosForPlayer2.Capacity);
            foreach (var kvp in _flippableDiscsAtEachPosForPlayer1)
            {
                board._flippableDiscsAtEachPosForPlayer1.Add(kvp.Key.Clone(), [..kvp.Value]);
            }
            foreach (var kvp in _flippableDiscsAtEachPosForPlayer2)
            {
                board._flippableDiscsAtEachPosForPlayer2.Add(kvp.Key.Clone(), [..kvp.Value]);
            }

        }
        return board;
    }

    public Dictionary<Position, HashSet<Position>> GetFlippableDiscsPerPlacement(int player)
    {
        Dictionary<Position, HashSet<Position>> cache = player == 1 ? _flippableDiscsAtEachPosForPlayer1 : _flippableDiscsAtEachPosForPlayer2; //shallow copy
        if (cache.Count > 0)
        {
            return cache;
        }
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (_cells[x,y] == 0)
                {
                    _cells[x, y] = player;
                    HashSet<Position> flippableDiscs = GetFlippableDiscs(_cells, x,y);
                    _cells[x, y] = 0;
                    if (flippableDiscs.Count > 0)
                    {
                        cache.Add(new Position(x,y), [..flippableDiscs]);
                    }
                }
            }
        }

        return cache;
    }

    private HashSet<Position> GetFlippableDiscsAtPos(Position pos, int player)
    {
        foreach (var flippableDisc in GetFlippableDiscsPerPlacement(player))
        {
            if (flippableDisc.Key.Equals(pos))
            {
                return flippableDisc.Value;
            }
        }
        return new HashSet<Position>();

    }
    
    private static HashSet<Position> GetFlippableDiscs(int[,] board, int changedDiscX, int changedDiscY, Dictionary<Position, HashSet<Position>>? cache = null)
    {
        if (cache != null)
        {
            //search the cache first
            if (cache.TryGetValue(new Position(changedDiscX, changedDiscX), out var discs))
            {
                return discs;
            }
        }
        
        var result = new HashSet<Position>(8); //8 is a good number, worst case it was a too high of a number
        
        //X axis
        int[] globalRow = new int[8]; //optimization, one list for all
        int x = 0, y = 0;
        for (; x < 8; x++)
        {
            globalRow[x] = board[x,changedDiscY];
        }
        foreach (int addedX in CheckLine(globalRow, changedDiscX))
        {
            result.Add(new Position(addedX, changedDiscY));
        }
        
        //Y axis
        for (; y < 8; y++)
        {
            globalRow[y] = board[changedDiscX, y];
        }
        foreach (int addedY in CheckLine(globalRow, changedDiscY))
        {
            result.Add(new Position(changedDiscX, addedY));
        }


        //first diagonal
        x = changedDiscX;
        y = changedDiscY;
        
        while (x > 0 && y > 0) //first, reset the pos to the start of the diagonal
        {
            x--;
            y--;
        } 
        var xOffset = x;
        var yOffset = y;

        //now, go through the diagonal and add to the list
        while (x < 8 && y < 8)
        {
            globalRow[x - xOffset] = board[x,y];
            x++;
            y++;
        }

        for(;x - xOffset < 8;x++) //fill in zeroes
        {
            globalRow[x - xOffset] = 0;
            
        }
        
        foreach (int addedX in CheckLine(globalRow, changedDiscX -  xOffset)) 
        {
            result.Add(new Position(addedX + xOffset, addedX + yOffset));
        }
        
        //second diagonal
        x = changedDiscX;
        y = changedDiscY;
        while (x > 0 && y < 8) //first, reset the pos to the start of the diagonal
        {
            x--;
            y++;
        } 
        xOffset = x;
        yOffset = 8 - y;
        //now, go through the diagonal and add to the list
        while (x < 8 && y >= 0)
        {
            if (y < 8)
            {
                globalRow[x - xOffset] = board[x, y];
                x++;
                y--;
            }
            else { break; }
        }

        for (;x - xOffset < 8; x++)
        {
            globalRow[x - xOffset] = 0;
        }
        foreach (int addedX in CheckLine(globalRow, changedDiscX -  xOffset)) 
        {
            result.Add(new Position(addedX + xOffset, 8 - addedX - yOffset));
        }
        
        return result;
    }
    
    private static List<int> CheckLine(int[] line, int changedIndex)
    {
        //will check if a flipped disc is encapsulated in a line by creating 2 pointers, one to the left and one to the right, and moving them until they find a closing disc.
        //returns an updated line
        var result = new List<int>(line.Length);
        int currentPlayer = line[changedIndex];
        for (int leftIndex = changedIndex-1; leftIndex >= 0; leftIndex--)
        {
            if (line[leftIndex] == 0) {break;} //not encapsulated, because there is a gap or reached the border
            
            if (line[leftIndex] == currentPlayer)
            {
                for (int i = changedIndex - 1; i > leftIndex; i--) //adds all the ones in the middle
                {
                    result.Add(i);
                }

                break;
            }
        }

        for (int rightIndex = changedIndex + 1; rightIndex < 8; rightIndex++)
        {
            if (line[rightIndex] == 0){break;}

            if (line[rightIndex] == currentPlayer)
            {
                for (int i = changedIndex + 1; i < rightIndex; i++)
                {
                    result.Add(i);
                }
                break;
            }
        }
        return result;
    }

    public bool Equals(Board board2)
    {

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (_cells[x, y] != board2._cells[x, y])
                {
                    return false;
                }
            }
        }
        return true;

    }

    private static readonly int[] RandForHashCode = [17, 31, 73, 97, 193, 257, 389, 613];
    public override int GetHashCode()
    {

        int hash = 12345;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                hash = hash * RandForHashCode[(x + y) % RandForHashCode.Length] + _cells[x, y];
            }
        }

        return hash;

    }

    public String ToString(Player player1, Player player2, int playerIndex)
    {
        String playerSymbol = " ";
        String boardString = "|";
        
        /*
          0 1 2 3 4 5 6 7
          0 | | | | | | | | |
          1 | | | | | | | | |
          2 | | | |1| | | | |
          3 | | |1|O|X| | | |
          4 | | | |X|O|1| | |
          5 | | | | |1| | | |
          6 | | | | | | | | |
          7 | | | | | | | | |

         */
        Position pos = new Position(0, 0);
        for (int x = 0; x < 8; x++)
        {
            pos.X = x;
            for (int y = 0; y < 8; y++)
            {
                pos.Y = y;
                if (GetCell(pos) == 0)
                {
                    var flippablesAtCurrentPos = GetFlippableDiscsAtPos(pos, playerIndex).Count;
                    playerSymbol = flippablesAtCurrentPos > 0 ? flippablesAtCurrentPos.ToString() : " ";
                }
                else if (GetCell(pos) == 1)
                {
                    playerSymbol = player1.Symbol;
                }
                else if (GetCell(pos) == 2)
                {
                    playerSymbol = player2.Symbol;
                }
                boardString += playerSymbol + "|";
            }
            boardString += "\n|";
        }
        return boardString.Substring(0, boardString.Length - 2); // Remove last "|"
    }
}