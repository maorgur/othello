namespace othello;

public class MiniMaxV2(string name = "MiniMaxV2", int depth=8) : Player(playerName:name)
{
    /*
     * MiniMax algorithm with alpha-beta pruning
     * plays greedy when reached a leaf
     * basic static weight matrix
     */
    
    private readonly Greedy _greedyPlayer = new(name); //have a greedy player to play when the search limit is reached
    private int[,] _weights = new int[8, 8];
    private bool _initializedWeights;

    public override Position GetMove(Board board)
    {
        GenerateWeights(board); //make sure it exists
        (Position result, double eval, int evaluatedStates) = BestMove(board, Id, Id == 1, depth,
            int.MinValue, int.MaxValue);
        eval /= 100;
        Console.WriteLine($"{Name} thinks the evaluation is {eval}, it explored {evaluatedStates} states.");
        return result;

    }

    private (Position Move, int eval, int evaluatedStates) BestMove(Board board, int currentPlayerId,
        bool preferPositive, int limit, int bestEvalForPlayer1, int bestEvalForPlayer2)
    {
        //alpha is the best move that blue has to offer, beta is the best move red has to offer. GLOBALLY ON ALL THE TREE!!!
        Position result = new Position(0, 0);
        Dictionary<Position, HashSet<Position>> positions = board.GetFlippableDiscsPerPlacement(currentPlayerId);
        
        Board tempBoard;
        int evaluatedStates = 0;
        if (limit <= 0 || positions.Count == 0)
        {
            //if reached depth limit, use greedy player to play
            if (positions.Count > 0)
            {
                _greedyPlayer.Id = currentPlayerId;
                result = _greedyPlayer.GetMove(board);
                tempBoard = board.Clone(true);
                tempBoard.PlaceDisc(result, currentPlayerId, true);
                return (result, tempBoard.GetEvaluation(_weights), 1);
            }
            return (new Position(0, 0), board.GetEvaluation(_weights), 1);
        }

        foreach (var potentialPlacement in positions)
        {
            tempBoard = board.Clone();
            tempBoard.PlaceDisc(potentialPlacement.Key, currentPlayerId, true);
            int newStates;
            int newEval;
            if (preferPositive)
            {
                (_, newEval, newStates) =
                    BestMove(tempBoard, 2, false, limit - 1, bestEvalForPlayer1, bestEvalForPlayer2);
                evaluatedStates += newStates;
                if (newEval > bestEvalForPlayer1) //update the best move that blue can do
                {
                    bestEvalForPlayer1 = newEval;
                    result = potentialPlacement.Key;
                }

                if (bestEvalForPlayer2 <= bestEvalForPlayer1)
                    //red wouldn't want to go that path and let blue do this new move,
                    //it would prefer taking other route that he has that wouldn't let blue play that good.
                    //the other move will force blue to play a shittier move (beta)
                    //therefor, we can give up because there is no chance it will go down to this entire node.
                {
                    return (result, bestEvalForPlayer1,
                        evaluatedStates); //we will still return alpha, even though it doesn't have a real effect.
                }

            }
            else
            {
                (_, newEval, newStates) =
                    BestMove(tempBoard, 1, true, limit - 1, bestEvalForPlayer1, bestEvalForPlayer2);
                evaluatedStates += newStates;
                if (newEval < bestEvalForPlayer2) //red found a new better move
                {
                    bestEvalForPlayer2 = newEval;
                    result = potentialPlacement.Key;
                }

                if (bestEvalForPlayer1 >= bestEvalForPlayer2)
                {
                    //blue wouldn't play that path, it has other path that will give him better result (alpha)
                    return (result, bestEvalForPlayer2, evaluatedStates);
                }
            }

        }


        return (result, preferPositive ? bestEvalForPlayer1 : bestEvalForPlayer2, evaluatedStates + 1);
    }

    private static int PosMultiplier(Position pos, Board exampleBoard, double multiplier = 1)
    {
        int result = 100;
        int y = pos.Y;
        int x = pos.X;
        

        Position center = new Position(3,3);
        if ((x == center.X && y == center.Y) || (x == center.X+1 && y == center.Y) || (x == center.X && y == center.Y+1) || (x == center.X+1 && y == center.Y+1))
        {
            result = 105;
        }
        
        
        if ((x == 0 || x == 7) || (y == 0 || y == 7))
        {
            if ((x == 0 || x == 7) && (y == 0 || y == 7))
            {
                result = 200; //corners
            }
            else if ((x <= 1 || x >= 6) && (y <= 1 || y >= 62))
            {
                result = 75; //near corners (C discs)
            } else  if (y == x || y == (7 - x)) //X discs
            {
                result = 50;
            } else {
            
                result = 115; //edges
            }
        }
        
        result = (int)(1 + (result - 1) * multiplier);
        return result;
    }

    private void GenerateWeights(Board exampleBoard)
    {
        if (_initializedWeights)
        {
            return;
        }
        _weights =  new int[8, 8];
        
        Position pos = new Position(0, 0);
        for (int x = 0; x < 8; x++)
        {
            pos.X = x;
            for (int y = 0; y < 8; y++)
            {
                pos.Y = y;
                _weights[x, y] = PosMultiplier(pos, exampleBoard);
                
            }
        }
        _initializedWeights = true;
    }

    
}