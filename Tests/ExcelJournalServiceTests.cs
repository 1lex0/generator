using ClosedXML.Excel;
using LabOH.Models;
using LabOH.Services;
using Xunit;

namespace LabOH.Tests;

public sealed class ExcelJournalServiceTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "LabOH-tests-" + Guid.NewGuid().ToString("N"));
    private string JournalPath => Path.Combine(directory, "journal.xlsx");
    public ExcelJournalServiceTests() => Directory.CreateDirectory(directory);

    private static StudentRecord Student(int number) => new()
    {
        LastName = $"Фамилия{number}", FirstName = "Имя", TicketNumber = number,
        CreatedAt = new DateTime(2026, 9, 30, 12, 30, 0)
    };

    [Fact]
    public void CreatesHeaderAndImmediatelyPersistsTypedRecord()
    {
        new ExcelJournalService(JournalPath).Append(Student(7));
        using var workbook = new XLWorkbook(JournalPath);
        var sheet = workbook.Worksheet(1);
        Assert.Equal(2, sheet.LastRowUsed()!.RowNumber());
        Assert.Equal("Last name", sheet.Cell(1, 1).GetString());
        Assert.Equal("First name", sheet.Cell(1, 2).GetString());
        Assert.Equal("Номер билета", sheet.Cell(1, 3).GetString());
        Assert.Equal("Дата и время", sheet.Cell(1, 4).GetString());
        Assert.Equal("Фамилия7", sheet.Cell(2, 1).GetString());
        Assert.Equal("Имя", sheet.Cell(2, 2).GetString());
        Assert.Equal(7, sheet.Cell(2, 3).GetValue<int>());
        Assert.Equal(Student(7).CreatedAt, sheet.Cell(2, 4).GetDateTime());
    }

    [Fact]
    public void RestartAppendsWithoutChangingEarlierRows()
    {
        var journal = new ExcelJournalService(JournalPath);
        for (var i = 1; i <= 3; i++) journal.Append(Student(i));
        var restarted = new ExcelJournalService(JournalPath);
        for (var i = 4; i <= 5; i++) restarted.Append(Student(i));
        using var workbook = new XLWorkbook(JournalPath);
        var sheet = workbook.Worksheet(1);
        Assert.Equal(6, sheet.LastRowUsed()!.RowNumber());
        for (var i = 1; i <= 5; i++)
        {
            Assert.Equal(Student(i).LastName, sheet.Cell(i + 1, 1).GetString());
            Assert.Equal(Student(i).FirstName, sheet.Cell(i + 1, 2).GetString());
            Assert.Equal(i, sheet.Cell(i + 1, 3).GetValue<int>());
            Assert.Equal(Student(i).CreatedAt, sheet.Cell(i + 1, 4).GetDateTime());
        }
    }

    [Theory]
    [InlineData(FileShare.None)]
    [InlineData(FileShare.Read)]
    public void LockedFileRemainsIntactAndRetryAddsExactlyOneRecord(FileShare share)
    {
        var journal = new ExcelJournalService(JournalPath);
        journal.Append(Student(1));
        var original = File.ReadAllBytes(JournalPath);
        using (var locked = new FileStream(JournalPath, FileMode.Open, FileAccess.Read, share))
        {
            var error = Record.Exception(() => journal.Append(Student(2)));
            Assert.True(error is IOException or UnauthorizedAccessException,
                $"Expected a file access error, got: {error}");
        }
        Assert.Equal(original, File.ReadAllBytes(JournalPath));
        Assert.Empty(Directory.GetFiles(directory, ".journal-*.xlsx"));
        journal.Append(Student(2));
        using var workbook = new XLWorkbook(JournalPath);
        Assert.Equal(3, workbook.Worksheet(1).LastRowUsed()!.RowNumber());
        Assert.Equal(2, workbook.Worksheet(1).Cell(3, 3).GetValue<int>());
    }

    public void Dispose()
    {
        foreach (var file in Directory.GetFiles(directory)) File.Delete(file);
        Directory.Delete(directory);
    }
}
