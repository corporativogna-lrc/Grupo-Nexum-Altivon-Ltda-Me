/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Sistema: GenesisGest.Net / Nexum Altivon
 * Versão: 1.1.5
 */

using System.Windows;
using System.Windows.Input;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class DesktopLoginWindow : Window
{
    private const string CredentialScope = "desktop/entry";
    private readonly TerminalProfile _terminal;
    private readonly DesktopApiClient _api = new();
    private bool _busy;
    private bool _initializing;
    private bool _passwordVisible;

    public DesktopLoginWindow(TerminalProfile terminal)
    {
        _terminal = terminal;
        InitializeComponent();
        _initializing = true;
        EmailBox.Text = Environment.GetEnvironmentVariable("NEXUM_DESKTOP_EMAIL")?.Trim() ?? string.Empty;
        try
        {
            var saved = DesktopCredentialStore.Load(CredentialScope);
            if (saved is not null)
            {
                EmailBox.Text = saved.Email;
                PasswordInput.Password = saved.Password;
                KeepPasswordCheckBox.IsChecked = true;
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Não foi possível ler a senha salva do Gerenciador de Credenciais: {ex.Message}";
        }
        _initializing = false;
        EmailBox.Focus();
    }

    private async void Entrar_Click(object sender, RoutedEventArgs e) => await AuthenticateAsync();
    private async void PasswordInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        await AuthenticateAsync();
    }

    private async void VisiblePasswordInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        await AuthenticateAsync();
    }

    private void TogglePassword_Click(object sender, RoutedEventArgs e)
    {
        _passwordVisible = !_passwordVisible;
        if (_passwordVisible)
        {
            VisiblePasswordInput.Text = PasswordInput.Password;
            PasswordInput.Visibility = Visibility.Collapsed;
            VisiblePasswordInput.Visibility = Visibility.Visible;
            TogglePasswordButton.Content = "Ocultar";
            VisiblePasswordInput.Focus();
            VisiblePasswordInput.CaretIndex = VisiblePasswordInput.Text.Length;
        }
        else
        {
            PasswordInput.Password = VisiblePasswordInput.Text;
            VisiblePasswordInput.Visibility = Visibility.Collapsed;
            PasswordInput.Visibility = Visibility.Visible;
            TogglePasswordButton.Content = "Mostrar";
            PasswordInput.Focus();
        }
    }

    private void KeepPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        try { DesktopCredentialStore.Delete(CredentialScope); }
        catch (Exception ex)
        {
            _initializing = true;
            KeepPasswordCheckBox.IsChecked = true;
            _initializing = false;
            StatusText.Text = $"Não foi possível remover a senha salva: {ex.Message}";
        }
    }

    private async Task AuthenticateAsync()
    {
        if (_busy) return;
        var password = _passwordVisible ? VisiblePasswordInput.Text : PasswordInput.Password;
        if (string.IsNullOrWhiteSpace(EmailBox.Text) || string.IsNullOrEmpty(password))
        {
            StatusText.Text = "Informe o e-mail e a senha da sua conta.";
            return;
        }

        _busy = true;
        EntrarButton.IsEnabled = false;
        StatusText.Text = "Validando as credenciais na API local e pública...";
        try
        {
            var result = await _api.AuthenticateAsync(_terminal, EmailBox.Text, password, MfaBox.Text);
            if (!result.Sucesso || result.Usuario is null || string.IsNullOrWhiteSpace(result.Token))
            {
                ClearPassword();
                StatusText.Text = $"Acesso não confirmado. {result.Mensagem}";
                return;
            }

            _terminal.DesktopAccessToken = result.Token;
            _terminal.OperatorName = result.Usuario.Nome;
            string? warning = null;
            try
            {
                if (KeepPasswordCheckBox.IsChecked == true)
                    DesktopCredentialStore.Save(CredentialScope, EmailBox.Text.Trim(), password);
                else
                    DesktopCredentialStore.Delete(CredentialScope);
            }
            catch (Exception ex) { warning = $"A sessão foi autenticada, mas a preferência de senha não foi aplicada: {ex.Message}"; }

            ClearPassword();
            MfaBox.Clear();
            if (warning is not null) MessageBox.Show(warning, "Senha salva", MessageBoxButton.OK, MessageBoxImage.Warning);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ClearPassword();
            StatusText.Text = $"Falha ao autenticar: {ex.Message}";
        }
        finally
        {
            _busy = false;
            if (DialogResult != true) EntrarButton.IsEnabled = true;
        }
    }

    private void ClearPassword()
    {
        PasswordInput.Clear();
        VisiblePasswordInput.Clear();
    }

    private void Sair_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
