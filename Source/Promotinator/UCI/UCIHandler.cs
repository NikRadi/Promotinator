using Promotinator.Engine;

namespace Promotinator.UCI;

public class UCIHandler {
    private Board _board;

    public void Start() {
        while (true) {
            string? input = Console.ReadLine();

            if (string.IsNullOrEmpty(input)) {
                continue;
            }

            HandleInput(input);
        }
    }

    private void HandleInput(string input) {
        string[] parts = input.Split(" ");
        string cmd = parts[0];

        if (cmd == "quit") {
            Environment.Exit(0);
        }
        else if (cmd == "stop") {
            Environment.Exit(0);
        }
        else if (cmd == "isready") {
            Console.WriteLine("readyok");
        }
        else if (cmd == "uci") {
            Console.WriteLine("id name Promotinator v001");
            Console.WriteLine("id author Nik Radi");
            Console.WriteLine("uciok");
        }
        else if (cmd == "ucinewgame") {
            _board = new();
        }
        else if (cmd == "position") {
            if (parts[1] == "startpos") {
                _board = new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            }
            else {
                Console.WriteLine($"Unknown position command: {input}");
            }

            if (parts.Length > 2) {
                if (parts[2] == "moves") {
                    for (int i = 3; i < parts.Length; ++i) {
                        var moves = MoveGenerator.GenerateMoves(_board);
                        var move = moves.Find(m => m.ToString() == parts[i]);
                        _board.MakeMove(move);
                    }
                }
            }
        }
        else if (cmd == "go") {
            var m = Search.FindBestMove(_board);
            Console.WriteLine($"bestmove {m}");
        }
        else {
            Console.WriteLine($"Unknown command: {input}");
        }
    }
}
