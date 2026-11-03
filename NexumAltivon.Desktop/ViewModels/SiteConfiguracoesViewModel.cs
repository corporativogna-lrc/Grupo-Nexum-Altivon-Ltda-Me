/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Sistema de gestão: GenesisGest.Net
 * Versão: 1.1.5
 */

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop.ViewModels;

public sealed class SiteConfiguracoesViewModel : ViewModelBase
{
    private readonly DesktopApiClient _apiClient;
    private readonly TerminalProfile _profile;
    private readonly string _token;

    private bool _isLoading;
    private string? _statusMessage;

    public ObservableCollection<DesktopSiteConfiguracao> Configuracoes { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand CarregarConfiguracoesCommand { get; }
    public ICommand SalvarConfiguracoesCommand { get; }

    public SiteConfiguracoesViewModel(DesktopApiClient apiClient, TerminalProfile profile, string token)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _token = token ?? throw new ArgumentNullException(nameof(token));

        CarregarConfiguracoesCommand = new RelayCommand(async _ => await CarregarAsync());
        SalvarConfiguracoesCommand = new RelayCommand(async _ => await SalvarAsync());
    }

    public async Task CarregarAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        StatusMessage = "Carregando configurações do site...";

        try
        {
            var resultado = await _apiClient.GetSiteConfigurationsAsync(_profile, _token);
            Configuracoes.Clear();

            if (resultado.Success && resultado.Data is not null)
            {
                foreach (var cfg in resultado.Data)
                {
                    Configuracoes.Add(cfg);
                }
                StatusMessage = $"Carregadas {Configuracoes.Count} configurações com sucesso.";
            }
            else
            {
                StatusMessage = resultado.Detail ?? "Erro ao carregar configurações do servidor.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao carregar: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SalvarAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        StatusMessage = "Salvando alterações...";

        try
        {
            var payload = new DesktopSiteConfiguracaoUpdateRequest(
                Configuracoes.Select(c => new DesktopSiteConfiguracaoUpdate(
                    c.Chave,
                    c.Valor,
                    c.Tipo,
                    c.Descricao,
                    c.Grupo,
                    c.Editavel)).ToList());

            var resultado = await _apiClient.SaveSiteConfigurationsAsync(_profile, _token, payload);
            if (resultado.Success)
            {
                StatusMessage = "Configurações salvas e aplicadas no servidor com sucesso.";
            }
            else
            {
                StatusMessage = resultado.Detail ?? "Erro ao salvar alterações no servidor.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao salvar: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}