using FootballTracker.Core.Interfaces.Services;
using FootballTracker.Core.Models.Database;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FootballTracker.Pages.Dialogs;

public partial class AuthDialog : ComponentBase
{
    
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IAuthService AuthService { get; set; } = null!;
    
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

    private async Task HandleLogin()
    {
        await _loginForm!.Validate();
        if (!_loginValid) return;

        _isLoading = true;
        _loginError = null;

        try
        {
            var result = await AuthService.Login(_email, _password);
            if (result is { IsSuccess: true, Data: not null })
            {
                
            }
            MudDialog.Close();
        }
        catch (Exception ex)
        {
            _loginError = ex.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task HandleRegister()
    {
        await _registerForm!.Validate();
        if (!_registerValid) return;

        _isLoading = true;
        _registerError = null;

        try
        {
            var result = await AuthService.Register(_email, _password);
            await AuthService.Login(_email, _password);
            if (result is { IsSuccess: true, Data: not null })
            {
                var model = new UserDto()
                {
                    Username = _username,
                    Vorname = _name,
                    Nachname = _lastName
                };
                await AuthService.InsertUserMetadata(model);
            }
            _registerSuccess = true;
            MudDialog.Close();
        }
        catch (Exception ex)
        {
            _registerError = ex.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }
}