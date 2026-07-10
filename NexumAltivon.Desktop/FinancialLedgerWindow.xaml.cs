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

public partial class FinancialLedgerWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly LocalOutboxService _outbox = new();
    private readonly TerminalProfile _terminal = new();
    private string _codigo = string.Empty;
    private string _tipo;
    private string _empresa = "Grupo Nexum Altivon";
    private string _pessoa = string.Empty;
    private string _documento = string.Empty;
    private string _centroCusto = "Operacional";
    private string _contaFinanceira = "Caixa / Banco principal";
    private string _valor = "0,00";
    private string _desconto = "0,00";
    private string _jurosMulta = "0,00";
    private DateTime _vencimento = DateTime.Today;
    private string _aprovacao = "Operador local";
    private string _nivelAlcada = "Operacional";
    private string _aprovadorResponsavel = string.Empty;
    private bool _bloquearPagamentoSemAprovacao = true;
    private string _observacoes = string.Empty;
    private string _status = "Pronto para registrar lançamento financeiro local.";

    public FinancialLedgerWindow(string documentType)
    {
        _tipo = documentType;
        DocumentTypes = new ObservableCollection<string>
        {
            "Contas a pagar",
            "Contas a receber",
            "Caixa e conciliação",
            "Tesouraria",
            "Alçadas de aprovação",
            "Conciliação bancária"
        };
        ApprovalLevels = new ObservableCollection<string> { "Operacional", "Gerencial", "Diretoria", "Dupla diretoria" };

        NovoDocumento();
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<string> DocumentTypes { get; }
    public ObservableCollection<string> ApprovalLevels { get; }
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
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
    }

    public string Empresa
    {
        get => _empresa;
        set => SetField(ref _empresa, value);
    }

    public string Pessoa
    {
        get => _pessoa;
        set => SetField(ref _pessoa, value);
    }

    public string Documento
    {
        get => _documento;
        set => SetField(ref _documento, value);
    }

    public string CentroCusto
    {
        get => _centroCusto;
        set => SetField(ref _centroCusto, value);
    }

    public string ContaFinanceira
    {
        get => _contaFinanceira;
        set => SetField(ref _contaFinanceira, value);
    }

    public string Valor
    {
        get => _valor;
        set
        {
            if (SetField(ref _valor, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string Desconto
    {
        get => _desconto;
        set
        {
            if (SetField(ref _desconto, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string JurosMulta
    {
        get => _jurosMulta;
        set
        {
            if (SetField(ref _jurosMulta, value))
            {
                AtualizarResumo();
            }
        }
    }

    public DateTime Vencimento
    {
        get => _vencimento;
        set => SetField(ref _vencimento, value);
    }

    public string Aprovacao
    {
        get => _aprovacao;
        set => SetField(ref _aprovacao, value);
    }

    public string NivelAlcada
    {
        get => _nivelAlcada;
        set
        {
            if (SetField(ref _nivelAlcada, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string AprovadorResponsavel
    {
        get => _aprovadorResponsavel;
        set
        {
            if (SetField(ref _aprovadorResponsavel, value))
            {
                AtualizarResumo();
            }
        }
    }

    public bool BloquearPagamentoSemAprovacao
    {
        get => _bloquearPagamentoSemAprovacao;
        set
        {
            if (SetField(ref _bloquearPagamentoSemAprovacao, value))
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

    public string TotalLiquidoLabel => $"R$ {TotalLiquido:N2}";
    public string IndicadorAlcada => TotalLiquido >= 50000m
        ? "Exige dupla aprovação diretiva."
        : TotalLiquido >= 10000m
            ? "Exige aprovação gerencial."
            : "Alçada operacional simples.";
    public string StatusPagamento => BloquearPagamentoSemAprovacao && string.IsNullOrWhiteSpace(AprovadorResponsavel)
        ? "Pagamento bloqueado até aprovação registrada."
        : $"Pagamento liberável no nível {NivelAlcada}.";

    private decimal TotalLiquido => Math.Max(0m, ParseDecimal(Valor) - ParseDecimal(Desconto) + ParseDecimal(JurosMulta));

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        var draft = new FinancialLedgerDraft
        {
            Codigo = Codigo,
            Tipo = Tipo,
            Empresa = Empresa.Trim(),
            Pessoa = Pessoa.Trim(),
            Documento = Documento.Trim(),
            CentroCusto = CentroCusto.Trim(),
            ContaFinanceira = ContaFinanceira.Trim(),
            Valor = ParseDecimal(Valor),
            Desconto = ParseDecimal(Desconto),
            JurosMulta = ParseDecimal(JurosMulta),
            Vencimento = Vencimento,
            Aprovacao = Aprovacao.Trim(),
            NivelAlcada = NivelAlcada.Trim(),
            AprovadorResponsavel = AprovadorResponsavel.Trim(),
            BloquearPagamentoSemAprovacao = BloquearPagamentoSemAprovacao,
            Observacoes = Observacoes.Trim(),
            Status = BloquearPagamentoSemAprovacao && string.IsNullOrWhiteSpace(AprovadorResponsavel)
                ? "Financeiro local registrado aguardando aprovação"
                : "Financeiro local registrado"
        };

        var submit = await _api.SubmitOperationAsync(_terminal, "financeiro", Codigo, draft);
        if (submit.Success)
        {
            Status = $"Lançamento gravado no servidor: {submit.ServerReference ?? submit.Detail}";
            return;
        }

        var path = await _outbox.SaveOperationAsync("financeiro", Codigo, draft);
        Status = $"API indisponível. Lançamento salvo em contingência local: {path}. Motivo: {submit.Detail}";
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        NovoDocumento();
    }

    private void Fechar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void NovoDocumento()
    {
        Codigo = $"FIN-{NormalizeCode(Tipo)}-{DateTime.Now:yyyyMMdd-HHmmss}";
        Pessoa = string.Empty;
        Documento = string.Empty;
        Valor = "0,00";
        Desconto = "0,00";
        JurosMulta = "0,00";
        Vencimento = DateTime.Today;
        AprovadorResponsavel = string.Empty;
        Observacoes = string.Empty;
        Status = "Novo lançamento financeiro pronto para preenchimento.";
        AtualizarResumo();
    }

    private void AtualizarResumo()
    {
        OnPropertyChanged(nameof(TotalLiquidoLabel));
        OnPropertyChanged(nameof(IndicadorAlcada));
        OnPropertyChanged(nameof(StatusPagamento));
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
