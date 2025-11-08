namespace othello;

public static class Program
{
    public static void Main(String[] args)
    {
        const string help = "Play Othello or watch bots play against each other.\n" +
                            "Rules: https://www.worldothello.org/about/about-othello/othello-rules/official-rules/english " +
                            "(Note: this implementation uses blue and red discs instead of black and white.)\n" +
                            "\nOn the board, empty cells show how many discs would be flipped if you place a disc there.\n\n" +
                            "There are seven player types:\n" +
                            "- Human:        \tPrompts the user to choose a move each turn.\n" +
                            "- RandomChoice: \tChooses a random valid move.\n" +
                            "- Greedy:       \tChooses the move that flips the most opponent discs.\n" +
                            "- MiniMaxV1:    \tA bot that searches for the move sequence that maximizes its score using the MiniMax algorithm.\n" +
                            "- MiniMaxV2:    \tLike MiniMaxV1 but uses a position-weight matrix to prefer more valuable squares.\n" +
                            "- MiniMaxV3:    \tFaster and smarter, with an improved weight matrix and speed optimizations.\n" +
                            "- MiniMaxV4:    \tThe strongest bot so far; uses a dynamic weight matrix and a mobility bonus.\n" +
                            "Note: Weaker bots can occasionally beat stronger ones, especially when playing second.\n\n" +
                            "HOW TO RUN:\n" +
                            "The format is: othello <player1>[-depth] <player2>[-depth] (case-sensitive)\n" +
                            "Depth is an optional integer telling a bot how many moves ahead to search (default: 7).\n" +
                            "The depth parameter is parsed only for bot players.\n" +
                            "Examples:\n" +
                            "othello Human Greedy             \tRuns a game between a human and the Greedy bot.\n" +
                            "othello MiniMaxV2 MiniMaxV4      \tRuns a game between MiniMaxV2 and MiniMaxV4.\n" +
                            "othello MiniMaxV1-8 MiniMaxV4-5  \tRuns MiniMaxV1 with depth 8 versus MiniMaxV4 with depth 5.";

        if (args.Length == 0)
        {
            Console.WriteLine(help);
            return;
        }
        foreach (var arg in args)
        {
            if (arg.Contains("help") || arg.Contains('?') || arg.Contains("-h"))
            {
                Console.WriteLine(help);
                return;
            }
        }
        
        if (args.Length != 2)
        {
            Console.WriteLine("You should parse as command parameters the type of the players." + 
                                        "\tAvailable Players: Human, RandomChoice, Greedy, MiniMaxV1, MiniMaxV2, MiniMaxV3, MiniMaxV4.\nExample: Human MiniMaxV3.\n" +
                                        "Run `othello help` for more information");
            Environment.Exit(1);
            return;

        }
        var players= new Player[2];
        for (var argCount = 0; argCount < 2; argCount++)
        {
            string[] playerArgs = args[argCount].Split('-');
            int depth = -1;
            if (playerArgs.Length == 2)
            {
                if (!int.TryParse(playerArgs[1], out depth))
                {
                    Console.WriteLine($"Unknown depth \"{playerArgs[1]}\". Please enter a number as depth." +
                                      $"\nExample: MiniMax4-7 MiniMaxV2-5\nRun `othello help` for more information");
                    Environment.Exit(1);
                    return;
                }
                if (depth < 0)
                {
                    Console.WriteLine($"Unknown search depth {depth}. depth must be greater than or equal to zero." +
                                      $"\nExample: MiniMax4-7 MiniMaxV2-5\nRun `othello help` for more information");
                    Environment.Exit(1);
                    return;
                }
            } else if (playerArgs.Length > 2)
            {
                Console.WriteLine("Too many player args, parse only the player type, and optionally the depth." +
                                  "\nExamples:\nMiniMaxV4-3 MiniMaxV1-5\nGreedy Human\nRun `othello help` for more information");
                Environment.Exit(1);
                return;
            }
            switch (playerArgs[0])
            {
                case "Human":
                    if (depth >= 0)
                    {
                        Console.WriteLine("Human player doesn't support the depth option\nRun `othello help` for more information.");
                        Environment.Exit(1);
                    }
                    players[argCount] = new Human();
                    break;
                // ReSharper disable once StringLiteralTypo
                case "RandomChoice":
                    if (depth >= 0)
                    {
                        Console.WriteLine("RandomChoice player doesn't support the depth option\nRun `othello help` for more information.");
                        Environment.Exit(1);
                    }
                    players[argCount] = new RandomChoice();
                    break;
                case "Greedy":
                    if (depth >= 0)
                    {
                        Console.WriteLine("Greedy player doesn't support the depth option\nRun `othello help` for more information.");
                        Environment.Exit(1);

                    }
                    players[argCount] = new Greedy();
                    break;
                case "MiniMaxV1":
                    if (depth < 0)
                    {
                        players[argCount] = new MiniMaxV1();
                        depth = 8;
                    }
                    else
                    {
                        players[argCount] = new MiniMaxV1(depth:depth);
                    }
                    players[argCount].Name += $" with depth {depth}";

                    break;
                case "MiniMaxV2":
                    if (depth < 0)
                    {
                        players[argCount] = new MiniMaxV2();
                        depth = 8;
                    }
                    else
                    {
                        players[argCount] = new MiniMaxV2(depth:depth);
                    }
                    players[argCount].Name += $" with depth {depth}";
                    break;
                case "MiniMaxV3":
                    if (depth < 0)
                    {
                        players[argCount] = new MiniMaxV3();
                        depth = 8;
                    }
                    else
                    {
                        players[argCount] = new MiniMaxV3(depth:depth);
                    }
                    players[argCount].Name += $" with depth {depth}";
                    break;
                case "MiniMaxV4":
                    if (depth < 0)
                    {
                        players[argCount] = new MiniMaxV4();
                        depth = 8;
                    }
                    else
                    {
                        players[argCount] = new MiniMaxV4(depth:depth);
                    }
                    players[argCount].Name += $" with depth {depth}";
                    break;
                default:
                    Console.WriteLine(
                        $"Unknown player {args[argCount]}." +
                        $"\nAvailable Players: Human, RandomChoice, Greedy, MiniMaxV1, MiniMaxV2, MiniMaxV3, MiniMaxV4. (Case sensitive)\nExample: Human MiniMaxV3." +
                        "\nRun `othello help` for more information.");
                    Environment.Exit(1);
                    break;
                    
            }
        }

        players[0].Id = 1;
        players[0].Symbol = "\u001b[34mX\u001b[0m";
        players[1].Id = 2;
        players[1].Symbol = "\u001b[31mO\u001b[0m";
        
        Game game = new Game(players);

        Console.WriteLine(game.ToString());
        
        bool failedLastMove = false;
        int moveCount = 0;
        // Simple loop: keep making moves until MakeMove returns false
        while (!game.Board.IsFinished())
        {
            var failedThisMove = !game.MakeMove();
            Console.Write("\u001b[2J\u001b[H"); //clear screen
            if (failedLastMove && failedThisMove)
            {
                Console.WriteLine("Game Over, both players cannot move");
                break;
            }
            failedLastMove = failedThisMove;
            moveCount++;
            
            Console.WriteLine(game.ToString());

            // safety guard to avoid infinite loops in case of a bug
            if (moveCount > Math.Pow(8, 2)*2)
            {
                Console.WriteLine("Too many moves, aborting.");
                Environment.Exit(1);
                return;
            }
        }
        Console.WriteLine("---------------");
        Console.WriteLine("Game finished. Final board:");
        Console.WriteLine(game.ToString());
        Console.WriteLine($"\n{players[0].Name} has {game.Board.CountDiscs(1)} discs.\t\t{players[1].Name} has {game.Board.CountDiscs(2)} discs.");
        (long player1Clock, long player2Clock) = game.GetClock();
        Console.WriteLine($"{players[0].Name} clock: {player1Clock}ms.\t\t{players[1].Name} clock: {player2Clock}ms.");
        

        int winner = game.Board.GetWinner();
        if (winner == 0)
        {
            Console.WriteLine("\nResult: Draw.");
        }
        else
        {
            String winnerName = winner == 1 ? players[0].Name : players[1].Name;
            Console.WriteLine($"\nWinner: {winnerName}.");
        }
    }
}
