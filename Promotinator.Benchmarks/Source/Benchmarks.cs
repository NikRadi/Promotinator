using System.Diagnostics;
using Promotinator.Engine;

namespace Promotinator.Benchmarks;

public static class Benchmarks {
    private static readonly List<BenchmarkData> BenchmarkDatas = [
        new() { Name = "Initial Position", MinDepth = 4, ShortDepth = 4, LongDepth = 4, FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" },
        new() { Name = "Position2",        MinDepth = 3, ShortDepth = 3, LongDepth = 4, FEN = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -" },
        new() { Name = "Position3",        MinDepth = 4, ShortDepth = 5, LongDepth = 7, FEN = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1" },
        new() { Name = "Position4",        MinDepth = 3, ShortDepth = 4, LongDepth = 5, FEN = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1" },
        new() { Name = "Position5",        MinDepth = 3, ShortDepth = 3, LongDepth = 5, FEN = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8" },
        new() { Name = "Position6",        MinDepth = 3, ShortDepth = 3, LongDepth = 5, FEN = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10" },
        new() { Name = "KingAndRooks1",    MinDepth = 6, ShortDepth = 7, LongDepth = 9, FEN = "8/8/8/8/8/8/6k1/4K2R b K - 0 1" },
        new() { Name = "KingAndPawns1",    MinDepth = 6, ShortDepth = 7, LongDepth = 9, FEN = "3k4/3pp3/8/8/8/8/3PP3/3K4 w - - 0 1" },
    ];

    public static void RunShortSuite() {
        Console.WriteLine("Warming up JIT compiler...");
        WarmupCompiler();

        Console.WriteLine("Running benchmarks...");
        Stopwatch sw = new Stopwatch();

        foreach (var benchmark in BenchmarkDatas) {
            for (int depth = benchmark.MinDepth; depth <= benchmark.ShortDepth; ++depth) {
                Console.WriteLine($"== {benchmark.Name} (depth {depth}) ==");

                int runs = 10;
                long elapsedMilliseconds = 0;
                long nodesSearched = 0;

                for (int run = 0; run < runs; ++run) {
                    sw.Reset();
                    sw.Start();

                    Board board = new(benchmark.FEN);
                    int numMoves = CountMoves(board, depth);

                    sw.Stop();

                    elapsedMilliseconds += sw.ElapsedMilliseconds;
                    nodesSearched += numMoves;
                }

                double totalSeconds = elapsedMilliseconds / 1000d;
                double avgSeconds = totalSeconds / runs;
                Console.WriteLine($"  Avg Time: {double.Round(avgSeconds, 2)}s");
                Console.WriteLine($"  Avg NPS: {(int) (nodesSearched / totalSeconds)} nodes/s");
            }
        }
    }

    public static void RunFullSuite() {
        Stopwatch sw = new Stopwatch();

        foreach (var benchmark in BenchmarkDatas) {
            Console.WriteLine($"== {benchmark.Name} ==");
            for (int depth = benchmark.MinDepth; depth <= benchmark.LongDepth; ++depth) {
                Console.Write($"Depth {depth}: ");
                sw.Reset();
                sw.Start();

                Board board = new(benchmark.FEN);
                int numMoves = CountMoves(board, depth);

                sw.Stop();

                double seconds = sw.ElapsedMilliseconds / 1000d;
                Console.WriteLine($"({seconds}s)");
            }
        }
    }

    private static int CountMoves(Board board, int depth) {
        if (depth == 0) {
            return 1;
        }

        int numMoves = 0;
        List<Move> moves = MoveGenerator.GenerateMoves(board);
        foreach (Move move in moves) {
            Coord? lastEnPassantSquare = board.EnPassantSquare;
            CastlingRights lastCastlingRights = board.CastlingRights;

            board.MakeMove(move);

            int count = CountMoves(board, depth - 1);
            numMoves += count;

            board.UndoMove(move, lastEnPassantSquare, lastCastlingRights);
        }

        return numMoves;
    }

    private static void WarmupCompiler() {
        string position4 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
        int numMoves = CountMoves(new(position4), 4);
        numMoves += CountMoves(new(position4), 4);
    }
}
