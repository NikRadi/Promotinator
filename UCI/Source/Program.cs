using Promotinator.Engine;

public class Program {
    private const string InitialPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    private static Board _board = new(InitialPosition);

    public static void Main(string[] args) {
        while (true) {
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) {
                continue;
            }

            try {
                ProcessInput(input);
            }
            catch (Exception e) {
                Console.WriteLine($"Error while processing input: {e}");
            }
        }
    }

    private static void ProcessInput(string input) {
        string[] parts = input.Split(" ");
        string cmd = parts[0];

        if (cmd == "quit") {
            Environment.Exit(0);
        }
        else if (cmd == "isready") {
            Console.WriteLine("readyok");
        }
        else if (cmd == "go") {
            string[] args = parts.Skip(1).ToArray();
            HandleGo(args);
        }
    }

    private static void HandleGo(string[] args) {
        int maxMilliseconds = 0;

        for (int i = 0; i < args.Length; ++i) {
            var arg = args[i];

            if (arg == "movetime") {
                i += 1;
                arg = args[i];
                maxMilliseconds = int.Parse(arg);
            }
        }

        var move = Search.FindBestMove(_board, maxMilliseconds);
        Console.WriteLine($"bestmove {move}");
    }
}
