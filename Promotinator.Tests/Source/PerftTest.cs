using Promotinator.Engine;
using Xunit.Abstractions;

namespace Promotinator.Tests;

public class PerftTest {
    private readonly ITestOutputHelper _output;

    // https://www.chessprogramming.org/Perft_Results
    private const string FENInitialPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    private const string FENPosition2 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -";
    private const string FENPosition3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";
    private const string FENPosition4 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
    private const string FENPosition5 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";
    private const string FENPosition6 = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10";

    public PerftTest(ITestOutputHelper output) {
        _output = output;
    }

    [Theory]
    [InlineData([1, 20, FENInitialPosition])]
    [InlineData([2, 400, FENInitialPosition])]
    [InlineData([3, 8_902, FENInitialPosition])]
    [InlineData([4, 197_281, FENInitialPosition])]
    [InlineData([5, 4_865_609, FENInitialPosition])]
    public void InitialPosition(int depth, int expected, string fen) {
        TestPosition(depth, expected, fen);
    }

    [Theory]
    [InlineData(1, 48, FENPosition2)]
    [InlineData(2, 2_039, FENPosition2)]
    [InlineData(3, 97_862, FENPosition2)]
    [InlineData(4, 4_085_603, FENPosition2)]
    public void Position2(int depth, int expected, string fen) {
        TestPosition(depth, expected, fen);
    }

    [Theory]
    [InlineData(1, 14, FENPosition3)]
    [InlineData(2, 191, FENPosition3)]
    [InlineData(3, 2_812, FENPosition3)]
    [InlineData(4, 43_238, FENPosition3)]
    [InlineData(5, 674_624, FENPosition3)]
    public void Position3(int depth, int expected, string fen) {
        TestPosition(depth, expected, fen);
    }

    [Theory]
    [InlineData(1, 6, FENPosition4)]
    [InlineData(2, 264, FENPosition4)]
    [InlineData(3, 9_467, FENPosition4)]
    [InlineData(4, 422_333, FENPosition4)]
    public void Position4(int depth, int expected, string fen) {
        TestPosition(depth, expected, fen);
    }

    [Theory]
    [InlineData(1, 44, FENPosition5)]
    [InlineData(2, 1_486, FENPosition5)]
    [InlineData(3, 62_379, FENPosition5)]
    [InlineData(4, 2_103_487, FENPosition5)]
    public void Position5(int depth, int expected, string fen) {
        TestPosition(depth, expected, fen);
    }

    [Theory]
    [InlineData(1, 46, FENPosition6)]
    [InlineData(2, 2_079, FENPosition6)]
    [InlineData(3, 89_890, FENPosition6)]
    [InlineData(4, 3_894_594, FENPosition6)]
    public void Position6(int depth, int expected, string fen) {
        TestPosition(depth, expected, fen);
    }

    [Fact]
    public void Debug() {
        TestPosition(1, 0, "r3k2r/p1ppqpb1/1n2pnp1/1b1PN3/1p2P3/5Q1p/PPPBBPPP/RN2K2R w KQkq - 0 1");
    }

    private void TestPosition(int depth, int expected, string fen) {
        Board board = new(fen);
        int moves = CountMoves(board, depth);
        Assert.Equal(expected, moves);
    }

    private int CountMoves(Board board, int depth, bool debug = true) {
        if (depth == 0) {
            return 1;
        }

        int numMoves = 0;
        List<Move> moves = MoveGenerator.GenerateMoves(board);
        Console.WriteLine($"CountMoves depth:{depth}");
        foreach (Move move in moves) {
            Coord? lastEnPassantSquare = board.EnPassantSquare;
            CastlingRights lastCastlingRights = board.CastlingRights;

            board.MakeMove(move);

            int count = CountMoves(board, depth - 1, false);
            numMoves += count;

            if (debug) {
                _output.WriteLine($"{move}: {count}");
            }

            board.UndoMove(move, lastEnPassantSquare, lastCastlingRights);
        }

        return numMoves;
    }
}
