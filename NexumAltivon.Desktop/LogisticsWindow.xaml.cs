/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class LogisticsWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly LocalOutboxService _outbox = new();
    private readonly TerminalProfile _terminal = new();
    private string _codigo = string.Empty;
    private string _tipo;
    private string _pedidoReferencia = string.Empty;
    private string _clienteDestino = string.Empty;
    private string _transportadora = string.Empty;
    private string _origem = "Loja / Centro de distribuição";
    private string _destino = string.Empty;
    private string _statusEntrega = "Aguardando separação";
    private string _custoFrete = "0,00";
    private DateTime _previsaoColeta = DateTime.Today;
    private DateTime _previsaoEntrega = DateTime.Today.AddDays(2);
    private bool _notificarCliente = true;
    private string _canalNotificacao = "WhatsApp e e-mail";
    private string _observacoes = string.Empty;
    private string _status = "Pronto para registrar logística local.";

    public LogisticsWindow(string operationType)
    {
        _tipo = operationType;
        OperationTypes = new ObservableCollection<string>
        {
            "Coleta",
            "Entrega",
            "Rastreamento",
            "Transportadoras",
            "Status do pedido",
            "Comunicação ao cliente"
        };
        StatusOptions = new ObservableCollection<string>
        {
            "Aguardando separação",
            "Pronto para coleta",
            "Coletado",
            "Em trânsito",
            "Saiu para entrega",
            "Entregue",
            "Ocorrência"
        };
        NotificationChannels = new ObservableCollection<string>
        {
            "WhatsApp e e-mail",
            "WhatsApp",
            "E-mail",
            "Sem notificação"
        };

        NovoDocumento();
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<string> OperationTypes { get; }
    public ObservableCollection<string> StatusOptions { get; }
    public ObservableCollection<string> NotificationChannels { get; }
    public string WindowTitle => $"GenesisGest.Net - {Tipo}";

    public string Codigo
    {
        get => _codigo;
        set => SetField(ref _codigo, value);
    }

    public string Tipo
    {
        get => _tipo;
        set
        {
            if (SetField(ref _tipo, value))
            {
                NovoDocumento(false);
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
    }

    public string PedidoReferencia
    {
        get => _pedidoReferencia;
        set => SetField(ref _pedidoReferencia, value);
    }

    public string ClienteDestino
    {
        get => _clienteDestino;
        set => SetField(ref _clienteDestino, value);
    }

    public string Transportadora
    {
        get => _transportadora;
        set => SetField(ref _transportadora, value);
    }

    public string Origem
    {
        get => _origem;
        set => SetField(ref _origem, value);
    }

    public string Destino
    {
        get => _destino;
        set => SetField(ref _destino, value);
    }

    public string StatusEntrega
    {
        get => _statusEntrega;
        set => SetField(ref _statusEntrega, value);
    }

    public string CustoFrete
    {
        get => _custoFrete;
        set => SetField(ref _custoFrete, value);
    }

    public DateTime PrevisaoColeta
    {
        get => _previsaoColeta;
        set
        {
            if (SetField(ref _previsaoColeta, value))
            {
                AtualizarResumo();
            }
        }
    }

    public DateTime PrevisaoEntrega
    {
        get => _previsaoEntrega;
        set
        {
            if (SetField(ref _previsaoEntrega, value))
            {
                AtualizarResumo();
            }
        }
    }

    public bool NotificarCliente
    {
        get => _notificarCliente;
        set
        {
            if (SetField(ref _notificarCliente, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string CanalNotificacao
    {
        get => _canalNotificacao;
        set
        {
            if (SetField(ref _canalNotificacao, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string Observacoes
    {
        get => _observacoes;
        set => SetField(ref _observacoes, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public string PrazoLabel => $"D+{Math.Max(0, (PrevisaoEntrega.Date - PrevisaoColeta.Date).Days)}";
    public string ComunicaçãoLabel => NotificarCliente
        ? $"Cliente será avisado por {CanalNotificacao} na sincronização."
        : "Notificação automática desativada para esta operação.";

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        var draft = new LogisticsOperationDraft
        {
            Codigo = Codigo,
            Tipo = Tipo,
            PedidoReferencia = PedidoReferencia.Trim(),
            ClienteDestino = ClienteDestino.Trim(),
            Transportadora = Transportadora.Trim(),
            Origem = Origem.Trim(),
            Destino = Destino.Trim(),
            StatusEntrega = StatusEntrega.Trim(),
            CustoFrete = ParseDecimal(CustoFrete),
            PrevisaoColeta = PrevisaoColeta,
            PrevisaoEntrega = PrevisaoEntrega,
            NotificarCliente = NotificarCliente,
            CanalNotificacao = CanalNotificacao.Trim(),
            Observacoes = Observacoes.Trim()
        };

        var submit = await _api.SubmitOperationAsync(_terminal, "logistica", Codigo, draft);
        if (submit.Success)
        {
            Status = $"Operação logística gravada no servidor: {submit.ServerReference ?? submit.Detail}";
            return;
        }

        var path = await _outbox.SaveOperationAsync("logistica", Codigo, draft);
        Status = $"API indisponível. Operação logística salva em contingência local: {path}. Motivo: {submit.Detail}";
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        NovoDocumento();
    }

    private void Fechar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void NovoDocumento(bool clearFields = true)
    {
        Codigo = $"LOG-{NormalizeCode(Tipo)}-{DateTime.Now:yyyyMMdd-HHmmss}";
        if (clearFields)
        {
            PedidoReferencia = string.Empty;
            ClienteDestino = string.Empty;
            Transportadora = string.Empty;
            Origem = "Loja / Centro de distribuição";
            Destino = string.Empty;
            StatusEntrega = "Aguardando separação";
            CustoFrete = "0,00";
            PrevisaoColeta = DateTime.Today;
            PrevisaoEntrega = DateTime.Today.AddDays(2);
            NotificarCliente = true;
            CanalNotificacao = "WhatsApp e e-mail";
            Observacoes = string.Empty;
        }

        Status = "Nova operação logística pronta para preenchimento.";
        AtualizarResumo();
    }

    private void AtualizarResumo()
    {
        OnPropertyChanged(nameof(PrazoLabel));
        OnPropertyChanged(nameof(ComunicaçãoLabel));
    }

    private static decimal ParseDecimal(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out var pt))
        {
            return pt;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariant)
            ? invariant
            : 0m;
    }

    private static string NormalizeCode(string value)
    {
        var chars = value.ToUpperInvariant().Where(char.IsLetterOrDigit).Take(10).ToArray();
        return chars.Length == 0 ? "DOC" : new string(chars);
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
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
