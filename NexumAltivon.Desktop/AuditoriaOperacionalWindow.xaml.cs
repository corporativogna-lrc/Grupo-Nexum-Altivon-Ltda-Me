/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7203
 * Origem funcional: adaptação auditada do LURCO432 para o Desktop WPF
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class AuditoriaOperacionalWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly CancellationTokenSource _lifetime = new();
    private readonly TerminalProfile? _terminal;
    private string _modulo = string.Empty;
    private string _tabela = string.Empty;
    private string _usuarioTexto = string.Empty;
    private string _dataInicioTexto = string.Empty;
    private string _dataFimTexto = string.Empty;
    private string _status = "Informe os filtros e consulte a API oficial.";

    public AuditoriaOperacionalWindow(TerminalProfile? terminal)
    {
        _terminal = terminal;
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<DesktopAuditoriaOperacional> Registros { get; } = [];

    public string Modulo
    {
        get => _modulo;
        set => SetField(ref _modulo, value);
    }

    public string Tabela
    {
        get => _tabela;
        set => SetField(ref _tabela, value);
    }

    public string UsuarioTexto
    {
        get => _usuarioTexto;
        set => SetField(ref _usuarioTexto, value);
    }

    public string DataInicioTexto
    {
        get => _dataInicioTexto;
        set => SetField(ref _dataInicioTexto, value);
    }

    public string DataFimTexto
    {
        get => _dataFimTexto;
        set => SetField(ref _dataFimTexto, value);
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string Resumo => $"{Registros.Count} registro(s) carregado(s)";

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Consultar_Click(object sender, RoutedEventArgs e)
    {
        if (_terminal is null)
        {
            Status = "Terminal não inicializado. A consulta não foi enviada.";
            return;
        }

        if (!TryParseUsuario(out var usuario))
        {
            return;
        }

        if (!TryParseData(DataInicioTexto, false, out var dataInicio)
            || !TryParseData(DataFimTexto, true, out var dataFim))
        {
            return;
        }

        Status = "Consultando /api/auditoria...";
        try
        {
            var result = await _api.GetAuditoriaFiltradaAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                Modulo,
                Tabela,
                usuario,
                dataInicio,
                dataFim,
                _lifetime.Token);

            Registros.Clear();
            if (!result.Success || result.Data is null)
            {
                OnPropertyChanged(nameof(Resumo));
                Status = $"A API não confirmou a consulta: {result.Detail}";
                return;
            }

            foreach (var registro in result.Data)
            {
                Registros.Add(registro);
            }

            OnPropertyChanged(nameof(Resumo));
            Status = $"Consulta confirmada pela API oficial. Limite operacional: 500 registros.";
        }
        catch (OperationCanceledException)
        {
            Status = "Consulta cancelada pelo fechamento da janela.";
        }
        catch (Exception ex)
        {
            Status = $"Falha na consulta: {ex.Message}";
        }
    }

    private void Limpar_Click(object sender, RoutedEventArgs e)
    {
        Modulo = string.Empty;
        Tabela = string.Empty;
        UsuarioTexto = string.Empty;
        DataInicioTexto = string.Empty;
        DataFimTexto = string.Empty;
        Registros.Clear();
        OnPropertyChanged(nameof(Resumo));
        Status = "Filtros e resultados limpos.";
    }

    private bool TryParseUsuario(out int? usuario)
    {
        usuario = null;
        if (string.IsNullOrWhiteSpace(UsuarioTexto))
        {
            return true;
        }

        if (int.TryParse(UsuarioTexto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0)
        {
            usuario = parsed;
            return true;
        }

        Status = "O usuário deve ser um número inteiro positivo.";
        return false;
    }

    private bool TryParseData(string texto, bool fimDoDia, out DateTime? data)
    {
        data = null;
        if (string.IsNullOrWhiteSpace(texto))
        {
            return true;
        }

        if (DateTime.TryParse(texto, CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.AllowWhiteSpaces, out var parsed))
        {
            data = fimDoDia ? parsed.Date.AddDays(1).AddTicks(-1) : parsed.Date;
            return true;
        }

        Status = $"Data inválida: {texto}. Use o formato dd/MM/aaaa.";
        return false;
    }

    protected override void OnClosed(EventArgs e)
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
        base.OnClosed(e);
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
