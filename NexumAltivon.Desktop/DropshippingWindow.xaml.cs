/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class DropshippingWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _profile;
    private string? _token;
    private bool _busy;

    public DropshippingWindow(TerminalProfile profile)
    {
        _profile = profile;
        ChannelTypes = new ObservableCollection<string>(["CJDropshipping", "Shopify", "AliExpress", "Dropi", "Cartpanda", "Nuvemshop", "Outro"]);
        InitializeComponent();
        DataContext = this;
        ResetForm();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DesktopDropshippingCanal> Channels { get; } = [];
    public ObservableCollection<string> ChannelTypes { get; }
    public DesktopDropshippingCanal? SelectedChannel { get; set; }
    public string LoginEmail { get; set; } = string.Empty;
    public string MfaCode { get; set; } = string.Empty;
    public bool IsAuthenticated { get; private set; }
    public string SessionLabel { get; private set; } = "Sessao administrativa nao autenticada";
    public string EndpointLabel { get; private set; } = "API oficial obrigatoria";
    public string Status { get; private set; } = "Autentique-se para consultar os canais do tenant atual.";
    public int? ChannelId { get; set; }
    public string? ChannelRowVersion { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelSlug { get; set; } = string.Empty;
    public string ChannelType { get; set; } = "CJDropshipping";
    public string ChannelEndpoint { get; set; } = string.Empty;
    public bool ChannelActive { get; set; }
    public string CredentialStatus { get; set; } = "Aguardando leitura da API";
    public string CredentialDetail { get; set; } = "Credenciais permanecem exclusivamente no servidor.";

    private async void Authenticate_Click(object sender, RoutedEventArgs e)
    {
        if (_busy)
        {
            return;
        }

        SetBusy(true, "Autenticando na API oficial...");
        try
        {
            var result = await _api.AuthenticateAsync(_profile, LoginEmail, LoginPassword.Password, MfaCode);
            if (!result.Success || string.IsNullOrWhiteSpace(result.Token) || result.User is null)
            {
                _token = null;
                IsAuthenticated = false;
                Status = $"Autenticacao recusada: {result.Detail}";
                RefreshBindings();
                return;
            }

            _token = result.Token;
            LoginPassword.Clear();
            IsAuthenticated = true;
            SessionLabel = $"{result.User.Nome} / {result.User.Perfil}";
            EndpointLabel = _profile.ApiBaseUrl;
            await ReloadAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ReloadAsync()
    {
        if (string.IsNullOrWhiteSpace(_token))
        {
            return;
        }

        var result = await _api.GetDropshippingChannelsAsync(_profile, _token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Falha ao carregar canais: {result.Detail}";
            RefreshBindings();
            return;
        }

        Channels.Clear();
        foreach (var channel in result.Data)
        {
            Channels.Add(channel);
        }

        Status = $"API confirmou {Channels.Count} canal(is) de dropshipping no tenant atual.";
        RefreshBindings();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_busy || string.IsNullOrWhiteSpace(_token))
        {
            return;
        }

        if (!TryBuildRequest(out var request, out var validation))
        {
            Status = validation;
            RefreshBindings();
            return;
        }

        SetBusy(true, "Salvando canal na API...");
        try
        {
            var result = await _api.SaveDropshippingChannelAsync(_profile, _token, request!, ChannelId);
            if (!result.Success || result.Data is null || result.Data.Id <= 0 || string.IsNullOrWhiteSpace(result.Data.RowVersion))
            {
                Status = $"Canal nao confirmado: {result.Detail}";
                RefreshBindings();
                return;
            }

            FillForm(result.Data);
            await ReloadAsync();
            Status = $"Canal {result.Data.Nome} persistido com ID {result.Data.Id}.";
            RefreshBindings();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_busy || string.IsNullOrWhiteSpace(_token) || SelectedChannel is null)
        {
            return;
        }

        if (MessageBox.Show($"Excluir o canal {SelectedChannel.Nome}?", "Confirmar exclusao", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        SetBusy(true, "Excluindo canal na API...");
        try
        {
            var result = await _api.DeleteDropshippingChannelAsync(_profile, _token, SelectedChannel.Id, SelectedChannel.RowVersion);
            Status = result.Success ? "Soft-delete do canal confirmado pela API." : $"Exclusao recusada: {result.Detail}";
            if (result.Success)
            {
                ResetForm();
                await ReloadAsync();
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void Reload_Click(object sender, RoutedEventArgs e)
    {
        if (!_busy)
        {
            await ReloadAsync();
        }
    }

    private void New_Click(object sender, RoutedEventArgs e) => ResetForm();

    private void ChannelSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedChannel is not null)
        {
            FillForm(SelectedChannel);
        }
    }

    private bool TryBuildRequest(out DesktopDropshippingCanalRequest? request, out string validation)
    {
        request = null;
        validation = string.Empty;
        var name = ChannelName.Trim();
        var slug = ChannelSlug.Trim();
        var endpoint = ChannelEndpoint.Trim();
        if (name.Length is < 3 or > 100)
        {
            validation = "Nome deve ter entre 3 e 100 caracteres.";
            return false;
        }

        if (slug.Length is < 2 or > 50)
        {
            validation = "Slug deve ter entre 2 e 50 caracteres.";
            return false;
        }

        if (endpoint.Length > 0 && (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps))
        {
            validation = "Endpoint deve ser uma URL HTTPS absoluta.";
            return false;
        }

        request = new DesktopDropshippingCanalRequest(name, slug, ChannelType, endpoint.Length == 0 ? null : endpoint, ChannelActive, ChannelRowVersion);
        return true;
    }

    private void FillForm(DesktopDropshippingCanal channel)
    {
        ChannelId = channel.Id;
        ChannelRowVersion = channel.RowVersion;
        ChannelName = channel.Nome;
        ChannelSlug = channel.Slug;
        ChannelType = channel.Tipo;
        ChannelEndpoint = channel.ApiEndpoint ?? string.Empty;
        ChannelActive = channel.Ativo;
        CredentialStatus = channel.StatusCredenciais;
        CredentialDetail = channel.DetalheCredenciais;
        RefreshBindings();
    }

    private void ResetForm()
    {
        ChannelId = null;
        ChannelRowVersion = null;
        ChannelName = string.Empty;
        ChannelSlug = string.Empty;
        ChannelType = "CJDropshipping";
        ChannelEndpoint = string.Empty;
        ChannelActive = false;
        CredentialStatus = "Aguardando leitura da API";
        CredentialDetail = "Credenciais permanecem exclusivamente no servidor.";
        SelectedChannel = null;
        RefreshBindings();
    }

    private void SetBusy(bool value, string? message = null)
    {
        _busy = value;
        if (!string.IsNullOrWhiteSpace(message))
        {
            Status = message;
        }
        RefreshBindings();
    }

    private void RefreshBindings()
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
}
