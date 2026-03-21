using CueCompanion.Components;
using MudBlazor;

namespace CueCompanion.Services;

public class SimpleDialogService
{
    private readonly IDialogService _dialogService;

    public SimpleDialogService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public Task ShowError(string title, string text, string? windowTitle = null)
    {
        DialogOptions options = new() { CloseOnEscapeKey = true };
        DialogParameters<ErrorDialog> parameters = new()
        {
            { x => x.Title, title },
            { x => x.Text, text }
        };

        return _dialogService.ShowAsync<ErrorDialog>(windowTitle ?? "Error", parameters, options);
    }
}