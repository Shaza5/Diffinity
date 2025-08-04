namespace Diffinity;
public static class DiffIgnoreLoader
{
    public static HashSet<string> LoadIgnoredObjects()
    {
        string filePath = "diffignore.txt";

        if (!File.Exists(filePath))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var lines = File.ReadAllLines(filePath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return lines;
    }
}