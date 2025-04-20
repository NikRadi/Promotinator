namespace Promotinator.Engine;

internal static class SearchDebug {
    private static string _filePath = "SearchLog.txt";
    private static object _fileLock = new();

    public static void Log(string message) {
        lock (_fileLock) {
            try {
                File.AppendAllText(_filePath, message + Environment.NewLine);
            }
            catch (Exception e) {
                Console.WriteLine($"Error while writing to {_filePath}: {e.Message}");
            }
        }
    }

    public static void ClearLog() {
        lock (_fileLock) {
            try {
                if (File.Exists(_filePath)) {
                    File.WriteAllText(_filePath, string.Empty);
                }
            }
            catch (Exception e) {
                Console.WriteLine($"Error while clearing {_filePath}: {e.Message}");
            }
        }
    }
}
