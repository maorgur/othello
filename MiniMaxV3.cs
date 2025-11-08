namespace othello;

public class MiniMaxV3(string name = "MiniMaxV3", int depth = 8) : Player(playerName:name)
{
    /*
     * MiniMax algorithm with alpha-beta pruning
     * plays smart greedy when reached a leaf
     * better static weight matrix
     * when going down the tree, it will search the better paths best, by the evaluation in the next step
     */
    
    private readonly int[,] _weights = new[,]
    {
        {220, 70 , 120, 105 , 105 , 120, 70 , 220},
        {70 , 60 , 95 , 95  , 95  , 95 , 60 , 70},
        {120, 95 , 115, 103 , 103 , 115, 95 , 120},
        {105, 95 , 103, 103 , 103 , 103, 95 , 105},
        {105, 95 , 103, 103 , 103 , 103, 95 , 105},
        {120, 95 , 115, 103 , 103 , 115, 95 , 120},
        {70 , 60 , 95 , 95  , 95  , 95 , 60 , 70},
        {220, 70 , 120, 105 , 105 , 120, 70 , 220},
    };

    public override Position GetMove(Board board)
    {
        (Position result, double eval, int evaluatedStates) = BestMove(board, Id, Id == 1, depth,
            int.MinValue, int.MaxValue);
        eval /=100;
        Console.WriteLine($"{Name} thinks the evaluation is {eval}, it explored {evaluatedStates} states.");
        return result;

    }

    private (Position Move, int eval, int evaluatedStates) BestMove(Board board, int currentPlayerId,
        bool preferPositive, int limit, int bestEvalForPlayer1, int bestEvalForPlayer2)
    {
        
        //alpha is the best move that blue has to offer, beta is the best move red has to offer. GLOBALLY ON ALL THE TREE!!!
        Position result = new Position(0, 0);

        var positions =
            board.GetFlippableDiscsPerPlacement(currentPlayerId);

        int evaluatedStates = 0;
        if (positions.Count == 0) //2 options, 1. skipped turn, 2. game ended. will try to check if it's a skip turn, if it cannot play for both players then quit
        {
            currentPlayerId = currentPlayerId == 1 ? 2 : 1;
            preferPositive = !preferPositive;
            positions = board.GetFlippableDiscsPerPlacement(currentPlayerId);
            if (positions.Count == 0)
            {
                return (result, board.GetEvaluation(_weights), 1);
            }
        }

        if (positions.Count == 1) { limit++; }

        if (limit <= 0)
        {
            var bestPos = positions.OrderByDescending(p => p.Value.Count).First().Key;
            var bestPositions = positions[bestPos];
            var evalChange = RankMove(bestPos, bestPositions);
            if (!preferPositive) { evalChange *= -1; }

            return (bestPos, board.GetEvaluation(_weights) + evalChange, 1);
        }

        
        var newBestEval = currentPlayerId == 1 ? int.MinValue : int.MaxValue;
        foreach (Position potentialPlacement in SortPositions(positions))
        {
            var tempBoard = board.Clone(true);
            tempBoard.PlaceDisc(potentialPlacement, currentPlayerId, true);
            int newStates;
            int newEval;
            
            
            if (preferPositive)
            {
                (_, newEval, newStates) =
                    BestMove(tempBoard, 2, false, limit-1, bestEvalForPlayer1, bestEvalForPlayer2);
                
                evaluatedStates += newStates;
                
                if (newEval > bestEvalForPlayer1) //update the best move that blue can do
                {
                    bestEvalForPlayer1 = newEval;
                    newBestEval = newEval;
                    result = potentialPlacement;
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
                    BestMove(tempBoard, 1, true, limit-1, bestEvalForPlayer1, bestEvalForPlayer2);
                
                evaluatedStates += newStates;
                if (newEval < bestEvalForPlayer2) //red found a new better move
                {
                    bestEvalForPlayer2 = newEval;
                    newBestEval = newEval;
                    result = potentialPlacement;
                }

                if (bestEvalForPlayer1 >= bestEvalForPlayer2)
                {
                    //blue wouldn't play that path, it has other path that will give him better result (alpha)
                    return (result, bestEvalForPlayer2, evaluatedStates);
                }
            }
        }

        return (result, newBestEval, evaluatedStates + 1);
    }

    private List<Position> SortPositions(Dictionary<Position, HashSet<Position>> positions)
    {
        //will sort moves, from best to worse. based on their rank from RankMove
        List<Position> sortedPositions = new List<Position>(positions.Count);
        if (positions.Count == 1)
        {
            sortedPositions.Add(positions.First().Key);
            return sortedPositions;
        }
        
        //if there is no move to play, it means that it got to a pos where the other player cannot play, prioritize those moves
        foreach (var entry in positions.OrderByDescending(p => RankMove(p.Key, p.Value))) //like py's lambda
        {
            sortedPositions.Add(entry.Key);
        }
        return sortedPositions;
    }

    private int RankMove(Position move, HashSet<Position> positions)
    {
        int result = 0;
        foreach (Position pos in positions)
        {
            result += _weights[pos.X, pos.Y]*2; //flipping a disc doubles the eval
        }
        
        result += _weights[move.X, move.Y];

        return result;
    }

    
}