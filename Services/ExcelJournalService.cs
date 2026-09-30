using System.IO;
using ClosedXML.Excel;
using LabOH.Models;

namespace LabOH.Services;

public class ExcelJournalService
{
    public string FilePath { get; }

    public ExcelJournalService(string filePath = "journal.xlsx")
    {
        FilePath = Path.GetFullPath(filePath);
    }

    public void Append(StudentRecord record)
    {
        using var workbook = File.Exists(FilePath)
            ? new XLWorkbook(FilePath)
            : new XLWorkbook();

        var sheet = workbook.Worksheets.FirstOrDefault()
            ?? workbook.Worksheets.Add("Journal");

        if (sheet.LastRowUsed() is null)
        {
            sheet.Cell(1, 1).Value = "Last name";
            sheet.Cell(1, 2).Value = "First name";
            sheet.Cell(1, 3).Value = "Номер билета";
            sheet.Cell(1, 4).Value = "Дата и время";
            sheet.Range(1, 1, 1, 4).Style.Font.Bold = true;
        }

        var row = (sheet.LastRowUsed()?.RowNumber() ?? 1) + 1;
        sheet.Cell(row, 1).Value = record.LastName;
        sheet.Cell(row, 2).Value = record.FirstName;
        sheet.Cell(row, 3).Value = record.TicketNumber;
        sheet.Cell(row, 4).Value = record.CreatedAt;
        sheet.Cell(row, 4).Style.DateFormat.Format = "dd.MM.yyyy HH:mm:ss";

        // Сначала сохраняем отдельный файл: ошибка записи не повредит старый журнал.
        var temporaryPath = Path.Combine(
            Path.GetDirectoryName(FilePath)!,
            $".journal-{Guid.NewGuid():N}.xlsx");
        try
        {
            workbook.SaveAs(temporaryPath);
            File.Move(temporaryPath, FilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
