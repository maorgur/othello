namespace othello;

public class MiniMaxV4(string name = "MiniMaxV4", int depth=8) : Player(playerName:name)
{
    /*
     * MiniMax algorithm with alpha-beta pruning
     * plays smart greedy when reached a leaf
     * 3 weight matrix, each one for different game stage (early, middle, end)
     * when going down the tree, it will search the better paths best, by the evaluation in the next step
     * gives bonus evaluation points if a board states gives the current player more move options than the opponent
     */
    
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
                return (result, board.GetEvaluation(GenerateWeights(board.PlacedDiscs)), 1);
            }
        }

        if (positions.Count == 1) { limit++; }

        if (limit <= 0)
        {
            var bestPos = positions.OrderByDescending(p => p.Value.Count).First().Key;
            Board tempBoard = board.Clone(true);
            tempBoard.PlaceDisc(bestPos, currentPlayerId, true);

            return (bestPos, tempBoard.GetEvaluation(GenerateWeights(board.PlacedDiscs)) + MobilityScore(tempBoard), 1);
        }

        
        var newBestEval = currentPlayerId == 1 ? int.MinValue : int.MaxValue;
        foreach (Position potentialPlacement in SortPositions(positions, board.PlacedDiscs))
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

    private List<Position> SortPositions(Dictionary<Position, HashSet<Position>> positions, int movesCount)
    {
        //will sort moves, from best to worse. based on their rank from RankMove
        List<Position> sortedPositions = new List<Position>(positions.Count);
        if (positions.Count == 1)
        {
            sortedPositions.Add(positions.First().Key);
            return sortedPositions;
        }
        
        //if there is no move to play, it means that it got to a pos where the other player cannot play, prioritize those moves
        foreach (var entry in positions.OrderByDescending(p => RankMove(p.Key, p.Value, movesCount))) //like py's lambda
        {
            sortedPositions.Add(entry.Key);
        }
        return sortedPositions;
    }

    private int RankMove(Position move, HashSet<Position> positions, int movesCount)
    {
        int[,] weights = GenerateWeights(movesCount);
        int result = 0;
        foreach (Position pos in positions)
        {
            result += weights[pos.X, pos.Y]*2; //flipping a disc doubles the eval
        }
        
        result += weights[move.X, move.Y];

        return result;
    }

    private int MobilityScore(Board board, int? player = null)
    {
        //mobility score is player1.moves.count - player2.moves.count
        int multiplier;
        if (board.PlacedDiscs < 20){multiplier = 12;}
        else if (board.PlacedDiscs < 50)
        {
            multiplier = 8;
        }
        else
        {
            return 0;
            
        }

        int result;
        if (player is 1 or null)
        {
            result = board.GetFlippableDiscsPerPlacement(1).Count - board.GetFlippableDiscsPerPlacement(2).Count;
        }
        else
        {
            result = -board.GetFlippableDiscsPerPlacement(2).Count;
            result += board.GetFlippableDiscsPerPlacement(1).Count;
        }

        return result * multiplier;
    }

    private static int[,] GenerateWeights(int placedDiscs)
    {
        int[,] early = {
            {240, -90, 120, 100, 100, 120, -90, 240},
            {-90, -140,  20,  40,  40,  20, -140, -90},
            {120,  20, 110,  90,  90, 110,  20, 120},
            {100,  40,  90,  85,  85,  90,  40, 100},
            {100,  40,  90,  85,  85,  90,  40, 100},
            {120,  20, 110,  90,  90, 110,  20, 120},
            {-90, -140, 20,  40,  40,  20, -140, -90},
            {240, -90, 120, 100, 100, 120, -90, 240}
        };
        
        int[,] mid = {
            {260, -40, 130, 110, 110, 130, -40, 260},
            {-40,  32,  80,  95,  95,  80,  32, -40},
            {130,  80, 120, 105, 105, 120,  80, 130},
            {110,  95, 105, 100, 100, 105, 95, 110},
            {110,  95, 105, 100, 100, 105, 95, 110},
            {130,  80, 120, 105, 105, 120, 80, 130},
            {-40,  32,  80,  95,  95,  80, 32, -40},
            {260, -40, 130, 110, 110, 130, -40, 260}
        };
        
        int[,] end = {
            {220,  60, 120, 105, 105, 120, 60, 220},
            {60,  80,  95, 100, 100, 95,  80,  60},
            {120, 95, 115, 103, 103, 115, 95, 120},
            {105, 100, 103, 104, 104, 103, 100, 105},
            {105, 100, 103, 104, 104, 103, 100, 105},
            {120, 95, 115, 103, 103, 115, 95, 120},
            {60,  80,  95, 100, 100, 95,  80,  60},
            {220, 60, 120, 105, 105, 120, 60, 220}
        };
        switch (placedDiscs)
        {
            case < 20:
                return early;
            case < 50:
                return mid;
            default:
                return end;
        }
    }

    
}