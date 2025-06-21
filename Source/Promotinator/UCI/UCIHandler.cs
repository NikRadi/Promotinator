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
            Console.WriteLine("id name Promotinator v001.1");
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
            else if (parts[1] == "fen" && parts.Length >= 7) {
                string fen = string.Join(" ", parts.Skip(2));
                _board = new(fen);
            }
            else {
                Console.WriteLine($"Unknown position command: {input}");
            }

            int idx = Array.FindIndex(parts, x => x == "moves");

            if (idx >= 0) {
                for (int i = idx + 1; i < parts.Length; ++i) {
                    var moves = MoveGenerator.GenerateMoves(_board);
                    var move = moves.Find(m => m.ToString() == parts[i]);
                    _board.MakeMove(move);
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
