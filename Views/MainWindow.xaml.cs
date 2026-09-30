using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LabOH.Models;
using LabOH.Services;

namespace LabOH.Views;

public partial class MainWindow : Window
{
    private readonly ExcelJournalService journal = new();
    private readonly StudentRecordFactory factory = new();
    private StudentRecord? pendingRecord;
    private bool isSaving;

    public MainWindow()
    {
        InitializeComponent();
        JournalPathText.Text = journal.FilePath;
        Loaded += (_, _) => LastNameBox.Focus();
    }

    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
        if (isSaving || !GenerateButton.IsEnabled) return;
        try
        {
            pendingRecord ??= factory.Create(LastNameBox.Text, FirstNameBox.Text);
        }
        catch (ArgumentException exception)
        {
            SetStatus(exception.Message, true);
            if (exception.ParamName == "lastName") LastNameBox.Focus();
            else FirstNameBox.Focus();
            return;
        }

        var record = pendingRecord;
        isSaving = true;
        GenerateButton.IsEnabled = false;
        LastNameBox.IsEnabled = FirstNameBox.IsEnabled = false;
        TicketText.Text = record.TicketNumber.ToString("00");
        StudentText.Text = $"{record.LastName} {record.FirstName}";
        SavedText.Text = "Сохраняем в журнал…";
        SetStatus("Сохранение записи…");
        try
        {
            await Task.Run(() => journal.Append(record));
            pendingRecord = null;
            SavedText.Text = $"Сохранено · {record.CreatedAt:dd.MM.yyyy HH:mm}";
            SetStatus("Готово! Запись сохранена в Excel.");
            NextButton.IsEnabled = true;
            NextButton.Focus();
        }
        catch (Exception exception)
        {
            SavedText.Text = "Запись пока не сохранена";
            SetStatus(exception is IOException or UnauthorizedAccessException
                ? "Не удалось записать журнал. Закройте Excel и проверьте доступ к папке, затем повторите сохранение."
                : $"Не удалось сохранить журнал: {exception.Message}", true);
            GenerateButton.Content = "Повторить сохранение";
            GenerateButton.IsEnabled = true;
        }
        finally { isSaving = false; }
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        LastNameBox.Clear();
        FirstNameBox.Clear();
        LastNameBox.IsEnabled = FirstNameBox.IsEnabled = true;
        GenerateButton.IsEnabled = true;
        GenerateButton.Content = "Получить билет →";
        NextButton.IsEnabled = false;
        TicketText.Text = "—";
        StudentText.Text = "Здесь появится ваш номер";
        SavedText.Text = "Случайный выбор от 1 до 20";
        SetStatus("Enter — получить билет · Esc — выход");
        LastNameBox.Focus();
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.GetDirectoryName(journal.FilePath)!,
                UseShellExecute = true
            });
        }
        catch (Exception exception) { SetStatus($"Не удалось открыть папку: {exception.Message}", true); }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { e.Handled = true; Close(); }
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (isSaving)
        {
            e.Cancel = true;
            SetStatus("Дождитесь завершения сохранения.");
        }
        else if (pendingRecord is not null)
        {
            e.Cancel = MessageBox.Show(this,
                "Текущий билет ещё не сохранён. Выйти и потерять эту запись?",
                "Несохранённый билет", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                != MessageBoxResult.Yes;
        }
    }

    private void SetStatus(string text, bool isError = false)
    {
        StatusText.Text = text;
        StatusText.Foreground = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(isError ? "#B42336" : "#52627B"));
    }
}
