namespace Promotinator.Benchmarks;

public struct BenchmarkData {
    public int MinDepth;
    public int LongDepth; // To run full suite (takes long time).
    public int ShortDepth; // To run short suite.
    public string FEN;
    public string Name;
}
