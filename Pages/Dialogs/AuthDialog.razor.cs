using FootballTracker.Core.Interfaces.Services;
using FootballTracker.Core.Models.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace FootballTracker.Pages.Dialogs;

public partial class AuthDialog : ComponentBase
{
    public static DialogOptions Options { get; } = new()
    {
        CloseOnEscapeKey = true,
        CloseButton = true,
        FullWidth = true,
        MaxWidth = MaxWidth.ExtraSmall
    };

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private int _activeTab;
    private bool IsLoginMode => _activeTab == 0;

    private MudForm? _loginForm;
    private MudForm? _registerForm;
    private bool _loginValid;
    private bool _registerValid;

    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _name = string.Empty;
    private string _lastName = string.Empty;
    private string _username = string.Empty;
    private bool _acceptTermsAndConditions;

    private bool _isLoading;
    private bool _showLoginPassword;
    private bool _showRegPassword;
    private bool _showConfirmPassword;
    private bool _registerSuccess;
    private string? _loginError;
    private string? _registerError;
    private string? _infoMessage;

    private async Task OnLoginKeyUp(KeyboardEventArgs args)
    {
        if (args.Key == "Enter" && _loginValid && !_isLoading)
            await HandleLogin();
    }

    private async Task HandleLogin()
    {
        await _loginForm!.Validate();
        if (!_loginValid) return;

        _isLoading = true;
        _loginError = null;
        _infoMessage = null;

        try
        {
            var result = await AuthService.Login(_email.Trim(), _password);
            if (result is { IsSuccess: true, Data: not null })
            {
                Snackbar.Add("Erfolgreich angemeldet", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                return;
            }
            _loginError = result.ErrorMessage ?? "Login fehlgeschlagen";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task HandleForgotPassword()
    {
        _loginError = null;
        _infoMessage = null;
        if (string.IsNullOrWhiteSpace(_email))
        {
            _loginError = "Bitte gib zuerst deine E-Mail-Adresse ein.";
            return;
        }

        var result = await AuthService.ResetPassword(_email.Trim());
        if (result.IsSuccess)
            _infoMessage = "Falls ein Konto existiert, wurde eine E-Mail zum Zurücksetzen des Passworts versendet.";
        else
            _loginError = result.ErrorMessage;
    }

    private async Task HandleRegister()
    {
        await _registerForm!.Validate();
        if (!_registerValid) return;

        _isLoading = true;
        _registerError = null;
        _registerSuccess = false;

        try
        {
            var metadata = new UserDto
            {
                Username = _username.Trim(),
                Vorname = _name.Trim(),
                Nachname = _lastName.Trim()
            };
            var result = await AuthService.Register(_email.Trim(), _password, metadata);
            if (result is not { IsSuccess: true, Data: not null })
            {
                _registerError = result.ErrorMessage ?? "Registrierung fehlgeschlagen";
                return;
            }

            if (AuthService.IsLoggedIn)
            {
                // E-Mail-Bestätigung ist deaktiviert: Profil direkt speichern und Dialog schließen.
                await AuthService.InsertUserMetadata(metadata);
                Snackbar.Add($"Willkommen, {metadata.Vorname}!", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                return;
            }

            _registerSuccess = true;
            _activeTab = 0;
            _infoMessage = "Registrierung erfolgreich! Bitte bestätige deine E-Mail-Adresse und melde dich anschließend an.";
        }
        finally
        {
            _isLoading = false;
        }
    }
}
