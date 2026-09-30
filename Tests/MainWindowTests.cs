using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using LabOH.Views;
using Xunit;

namespace LabOH.Tests;

public class MainWindowTests
{
    [Fact]
    public void WindowLoadsWithFormAndJournalLocation()
    {
        OnUiThread(() =>
        {
            var window = new MainWindow();
            try
            {
                window.Measure(new Size(1000, 700));
                window.Arrange(new Rect(0, 0, 1000, 700));
                Assert.True(((Button)window.FindName("GenerateButton")).IsEnabled);
                Assert.False(((Button)window.FindName("NextButton")).IsEnabled);
                Assert.Equal(Path.GetFullPath("journal.xlsx"), ((TextBlock)window.FindName("JournalPathText")).Text);
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void EmptyFormShowsValidationWithoutGeneratingTicket()
    {
        OnUiThread(() =>
        {
            var window = new MainWindow();
            try
            {
                ((Button)window.FindName("GenerateButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Contains("Введите фамилию", ((TextBlock)window.FindName("StatusText")).Text);
                Assert.Equal("—", ((TextBlock)window.FindName("TicketText")).Text);
                Assert.True(((TextBox)window.FindName("LastNameBox")).IsEnabled);
            }
            finally { window.Close(); }
        });
    }

    private static void OnUiThread(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { failure = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)), "UI test timed out.");
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
