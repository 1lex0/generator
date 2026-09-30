namespace LabOH.Models;

public class StudentRecord
{
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public int TicketNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}
