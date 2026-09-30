namespace LabOH.Services;

public class TicketGenerator
{
    public int Generate() => Random.Shared.Next(1, 21);
}
