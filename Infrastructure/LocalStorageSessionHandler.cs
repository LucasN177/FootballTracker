using Microsoft.JSInterop;
using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace FootballTracker.Infrastructure;

/// <summary>
/// Speichert die Supabase-Session im localStorage, damit Nutzer nach einem Reload eingeloggt bleiben.
/// Gotrue erwartet eine synchrone Schnittstelle, daher wird die In-Process-Runtime von WebAssembly genutzt.
/// </summary>
public class LocalStorageSessionHandler(IJSRuntime jsRuntime) : IGotrueSessionPersistence<Session>
{
    private const string StorageKey = "ft.supabase.session";

    private IJSInProcessRuntime? Js => jsRuntime as IJSInProcessRuntime;

    public void SaveSession(Session session)
    {
        try
        {
            Js?.InvokeVoid("localStorage.setItem", StorageKey, JsonConvert.SerializeObject(session));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Session could not be saved: {ex.Message}");
        }
    }

    public void DestroySession()
    {
        try
        {
            Js?.InvokeVoid("localStorage.removeItem", StorageKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Session could not be removed: {ex.Message}");
        }
    }

    public Session? LoadSession()
    {
        try
        {
            var json = Js?.Invoke<string?>("localStorage.getItem", StorageKey);
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<Session>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Session could not be loaded: {ex.Message}");
            return null;
        }
    }
}
