using Promotinator.UCI;

namespace Promotinator;

public class Program {
    public static void Main(string[] args) {
        UCIHandler handler = new();
        handler.Start();
    }
}
