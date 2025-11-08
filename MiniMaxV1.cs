namespace othello;

public class MiniMaxV1(string name = "MiniMaxV1", int depth = 8) : Player(playerName: name)
{
    /*
     * MiniMax algorithm with alpha-beta pruning
     * plays greedy when reached a leaf
     */

    private readonly Greedy _greedyPlayer = new(name); //have a greedy player to play when the search limit is reached

    public override Position GetMove(Board board)
    {

        (Position result,int eval, int evaluatedStates) = BestMove(board, Id, Id == 1, depth, (int) Math.Pow(8, 2)*-1 - 1, (int) Math.Pow(8, 2) + 1);
        Console.WriteLine($"{Name} thinks the evaluation is {eval}, it explored {evaluatedStates} states.");
        return result;

    }

    private (Position Move, int eval, int evaluatedStates) BestMove(Board board, int currentPlayerId, bool preferPositive, int limit, int bestEvalForPlayer1, int bestEvalForPlayer2)
    {
        //alpha is the best move that blue has to offer, beta is the best move red has to offer. GLOBALLY ON ALL THE TREE!!!
        Position result = new Position(0, 0);
        Dictionary<Position, HashSet<Position>> positions = board.GetFlippableDiscsPerPlacement(currentPlayerId);

        Board tempBoard;
        int evaluatedStates = 0;
        if (positions.Count == 0 || limit <= 0)
        {
            //if reached depth limit, use greedy player to play
            _greedyPlayer.Id = currentPlayerId;
            result = _greedyPlayer.GetMove(board);
            tempBoard = board.Clone();
            tempBoard.PlaceDisc(result, currentPlayerId, true);
            return (result, tempBoard.GetEvaluation(), 1);
        }

        foreach (var potentialPlacement in positions)
        {
            tempBoard = board.Clone();
            tempBoard.PlaceDisc(potentialPlacement.Key, currentPlayerId, true);
            int newStates;
            int newEval;
            if (preferPositive)
            {
                (_, newEval, newStates) = BestMove(tempBoard, 2, false, limit-1, bestEvalForPlayer1, bestEvalForPlayer2);
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
                    return (result, bestEvalForPlayer1, evaluatedStates); //we will still return alpha, even though it doesn't have a real effect.
                }
                
            }
            else
            {
                (_, newEval, newStates) = BestMove(tempBoard, 1, true, limit-1, bestEvalForPlayer1, bestEvalForPlayer2);
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

        if (evaluatedStates < 0)
        {
            Console.WriteLine($"For some reason the evaluated states is {evaluatedStates}...");
        }
        return (result, preferPositive ? bestEvalForPlayer1:bestEvalForPlayer2, evaluatedStates+1);
    }
}