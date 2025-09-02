using Diffinity;
using Diffinity.HtmlHelper;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

public class HtmlReportWriterTests : IDisposable
{
    private readonly string _tempFolder;

    public HtmlReportWriterTests()
    {
        // Create a temporary folder for writing reports
        _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempFolder);
    }

    [Fact]
    public void WriteIndexSummary_CreatesHtmlFile_WithCorrectPlaceholdersReplaced()
    {
        // Arrange
        string sourceConn = @"Server=localhost;Database=SrcDb;User Id=sa;Password=pass;";
        string destConn = @"Server=localhost;Database=DestDb;User Id=sa;Password=pass;";

        // Act
        string indexPath = HtmlReportWriter.WriteIndexSummary(sourceConn, destConn, _tempFolder);

        // Assert
        Assert.True(File.Exists(indexPath));
        string html = File.ReadAllText(indexPath);
        Assert.Contains("SrcDb", html);
        Assert.Contains("DestDb", html);
        Assert.Contains("Database Comparison Summary", html);
    }

    [Fact]
    public void WriteBodyHtml_CreatesHtmlFile_WithTitleAndBody()
    {
        // Arrange
        string filePath = Path.Combine(_tempFolder, "proc.html");
        string title = "TestProc";
        string body = "SELECT 1;";
        string returnPage = "index.html";

        // Act
        HtmlReportWriter.WriteBodyHtml(filePath, title, body, returnPage);

        // Assert
        Assert.True(File.Exists(filePath));
        string html = File.ReadAllText(filePath);
        Assert.Contains(title, html);
        Assert.Contains("SELECT 1;", html);
        Assert.Contains(returnPage, html);
    }

    [Fact]
    public void DifferencesWriter_CreatesHtmlFile_WithDiffContent()
    {
        // Arrange
        string filePath = Path.Combine(_tempFolder, "diff.html");
        string source = "SrcProc";
        string dest = "DestProc";
        string sourceBody = "SELECT 1;";
        string destBody = "SELECT 2;";
        string title = "TestDiff";
        string name = "TestProc";
        string returnPage = "index.html";

        // Act
        HtmlReportWriter.DifferencesWriter(filePath, source, dest, sourceBody, destBody, title, name, returnPage);

        // Assert
        Assert.True(File.Exists(filePath));
        string html = File.ReadAllText(filePath);
        Assert.Contains("TestDiff", html);
        Assert.Contains("SELECT 1", html);
        Assert.Contains("SELECT 2", html);
        Assert.Contains(returnPage, html);
    }

    [Fact]
    public void WriteIgnoredReport_CreatesHtmlFile_WithIgnoredObjects()
    {
        // Arrange
        var ignored = new HashSet<string> { "Table1", "Proc1" };

        // Act
        var report = HtmlReportWriter.WriteIgnoredReport(_tempFolder, ignored, Run.All);

        // Assert
        Assert.True(File.Exists(report.fullPath));
        string html = File.ReadAllText(report.fullPath);
        foreach (var item in ignored)
        {
            Assert.Contains(item, html);
        }
    }

    public void Dispose()
    {
        // Cleanup temp folder
        if (Directory.Exists(_tempFolder))
            Directory.Delete(_tempFolder, true);
    }
}
