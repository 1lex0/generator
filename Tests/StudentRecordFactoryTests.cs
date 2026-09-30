using LabOH.Services;
using Xunit;

namespace LabOH.Tests;

public class StudentRecordFactoryTests
{
    [Theory]
    [InlineData("", "Иван")]
    [InlineData("   ", "Иван")]
    [InlineData("Иванов", "")]
    [InlineData("Иванов", " \t ")]
    public void RejectsBlankNames(string lastName, string firstName)
    {
        Assert.Throws<ArgumentException>(() => new StudentRecordFactory().Create(lastName, firstName));
    }

    [Fact]
    public void TrimsNamesAndAssignsTicketAndTimestamp()
    {
        var before = DateTime.Now;
        var record = new StudentRecordFactory().Create("  Иванов  ", " Иван ");
        Assert.Equal("Иванов", record.LastName);
        Assert.Equal("Иван", record.FirstName);
        Assert.InRange(record.TicketNumber, 1, 20);
        Assert.InRange(record.CreatedAt, before, DateTime.Now);
    }

    [Fact]
    public void GeneratedTicketsStayWithinExamRange()
    {
        var generator = new TicketGenerator();
        for (var i = 0; i < 10000; i++) Assert.InRange(generator.Generate(), 1, 20);
    }
}
