using CueCompanion.Components;
using MudBlazor;

namespace CueCompanion.Services;

public class SimpleDialogService
{
    public SimpleDialogService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    private readonly IDialogService           _dialogService;
    private          List<(string, DateTime)> _recentDialogs = [];

    public Task Show(string title, string text, string windowTitle = "Error")
    {
        DateTime now = DateTime.Now;
        if (_recentDialogs.Any(x => x.Item1 == text && (now - x.Item2).TotalSeconds < 1))
        {
            // Don't show the same error multiple times within 2 seconds
            return Task.CompletedTask;
        }

        _recentDialogs.Add((text, now));

        // Clean up
        _recentDialogs = _recentDialogs.Where(x => (now - x.Item2).TotalSeconds < 2).ToList();
        DialogOptions options = new()
        {
            CloseOnEscapeKey = true,
        };
        DialogParameters<ErrorDialog> parameters = new()
        {
            { x => x.Title, title },
            { x => x.Text, text },
        };

        return _dialogService.ShowAsync<ErrorDialog>(windowTitle, parameters, options);
    }
}