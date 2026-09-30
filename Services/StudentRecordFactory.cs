using LabOH.Models;

namespace LabOH.Services;

public class StudentRecordFactory
{
    public StudentRecord Create(string lastName, string firstName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Введите фамилию студента.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Введите имя студента.", nameof(firstName));

        return new StudentRecord
        {
            LastName = lastName.Trim(),
            FirstName = firstName.Trim(),
            TicketNumber = new TicketGenerator().Generate(),
            CreatedAt = DateTime.Now
        };
    }
}
