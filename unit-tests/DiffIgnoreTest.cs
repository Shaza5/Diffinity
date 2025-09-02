using Diffinity;

public class IgnoreFileTests : IDisposable
{
    private readonly string _filePath = ".diffignore";

    public IgnoreFileTests()
    {
        // Ensure a clean state before each test
        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }

    [Fact]
    public void LoadIgnoredObjects_ReturnsEmptySet_WhenFileDoesNotExist()
    {
        // No file exists
        var result = DiffIgnoreLoader.LoadIgnoredObjects();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void LoadIgnoredObjects_ReturnsIgnoredLines_ExcludingCommentsAndWhitespace()
    {
        // Arrange
        File.WriteAllLines(_filePath, new[]
        {
            "Table1",
            "  Table2  ",
            "# This is a comment",
            "",
            "Proc1"
        });

        // Act
        var result = DiffIgnoreLoader.LoadIgnoredObjects();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("Table1", result);
        Assert.Contains("Table2", result); // trimmed spaces
        Assert.Contains("Proc1", result);
        Assert.DoesNotContain("# This is a comment", result);
        Assert.DoesNotContain("", result);
    }

    [Fact]
    public void LoadIgnoredObjects_IsCaseInsensitive()
    {
        File.WriteAllLines(_filePath, new[] { "TableA", "tableb" });

        var result = DiffIgnoreLoader.LoadIgnoredObjects();

        Assert.True(result.Contains("TABLEA"));
        Assert.True(result.Contains("TABLEB"));
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }
}
