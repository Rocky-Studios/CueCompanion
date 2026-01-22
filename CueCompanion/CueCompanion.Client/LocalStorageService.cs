using System.Text.Json;
using Microsoft.JSInterop;

namespace CueCompanion.Client;

// csharp
// File: CueCompanion.Client/Services/LocalStorageService.cs

public class LocalStorageService
{
    private readonly IJSRuntime _js;

    public LocalStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public ValueTask SetItemAsync(string key, string value)
    {
        return _js.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public ValueTask<string?> GetItemAsync(string key)
    {
        return _js.InvokeAsync<string?>("localStorage.getItem", key);
    }

    public ValueTask RemoveItemAsync(string key)
    {
        return _js.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public ValueTask ClearAsync()
    {
        return _js.InvokeVoidAsync("localStorage.clear");
    }

    public async Task SetObjectAsync<T>(string key, T obj)
    {
        await SetItemAsync(key, JsonSerializer.Serialize(obj));
    }

    public async Task<T?> GetObjectAsync<T>(string key)
    {
        string? json = await GetItemAsync(key);
        return json is null ? default : JsonSerializer.Deserialize<T?>(json);
    }
}

// Registration (add to your Program.cs)
// builder.Services.AddScoped<LocalStorageService>();

// Usage in a Razor component:
// @inject LocalStorageService LocalStorage
// ...
// await LocalStorage.SetItemAsync("myKey", "myValue");
// var value = await LocalStorage.GetItemAsync("myKey");
// await LocalStorage.SetObjectAsync("prefs", new { Theme = "dark" });
// var prefs = await LocalStorage.GetObjectAsync<dynamic>("prefs");