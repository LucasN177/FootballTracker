using Microsoft.JSInterop;

namespace FootballTracker.Services;

public class LocalStorageService(IJSRuntime jsRuntime)
{
    public async Task<string?> GetAsync(string key)
    {
        try
        {
            return await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (JSException)
        {
            return null;
        }
    }

    public async Task SetAsync(string key, string value)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (JSException ex)
        {
            Console.WriteLine($"localStorage write failed: {ex.Message}");
        }
    }
}
