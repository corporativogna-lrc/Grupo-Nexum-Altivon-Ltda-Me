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

public partial class FiscalRoutingWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly LocalOutboxService _outbox = new();
    private readonly TerminalProfile _terminal = new();
    private string _codigo = string.Empty;
    private string _tipo;
    private string _empresaCandidata = "Grupo Nexum Altivon";
    private string _regimeFiscal = "Simples Nacional";
    private string _ufDestino = "SP";
    private string _cfop = "5102";
    private string _naturezaOperacao = "Venda de mercadoria";
    private string _valorOperacao = "0,00";
    private string _custoFiscalEstimado = "0,00";
    private string _custoLogisticoEstimado = "0,00";
    private string _margemBrutaEstimada = "0,00";
    private string _rankingEmpresas = "Informe valor e margem para calcular a melhor empresa emissora.";
    private string _observacoes = string.Empty;
    private string _status = "Pronto para calcular regra fiscal local.";

    public FiscalRoutingWindow(string operationType)
    {
        _tipo = operationType;
        OperationTypes = new ObservableCollection<string>
        {
            "Empresa emissora automática",
            "Regras fiscais do grupo",
            "SPED / EFD / REINF",
            "Tributos e retenções"
        };
        Empresas = new ObservableCollection<string>
        {
            "Grupo Nexum Altivon",
            "Geração Top Mais",
            "Chronnus Relojoaria",
            "Gran Festas",
            "Moda Mim",
            "Estrutural Line"
        };
        Regimes = new ObservableCollection<string> { "Simples Nacional", "Lucro Presumido", "Lucro Real", "MEI / contingência teste" };

        NovoDocumento();
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<string> OperationTypes { get; }
    public ObservableCollection<string> Empresas { get; }
    public ObservableCollection<string> Regimes { get; }
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

    public string EmpresaCandidata
    {
        get => _empresaCandidata;
        set => SetField(ref _empresaCandidata, value);
    }

    public string RegimeFiscal
    {
        get => _regimeFiscal;
        set => SetField(ref _regimeFiscal, value);
    }

    public string UfDestino
    {
        get => _ufDestino;
        set => SetField(ref _ufDestino, value.ToUpperInvariant());
    }

    public string Cfop
    {
        get => _cfop;
        set => SetField(ref _cfop, value);
    }

    public string NaturezaOperacao
    {
        get => _naturezaOperacao;
        set => SetField(ref _naturezaOperacao, value);
    }

    public string ValorOperacao
    {
        get => _valorOperacao;
        set
        {
            if (SetField(ref _valorOperacao, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string CustoFiscalEstimado
    {
        get => _custoFiscalEstimado;
        set
        {
            if (SetField(ref _custoFiscalEstimado, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string CustoLogisticoEstimado
    {
        get => _custoLogisticoEstimado;
        set
        {
            if (SetField(ref _custoLogisticoEstimado, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string MargemBrutaEstimada
    {
        get => _margemBrutaEstimada;
        set
        {
            if (SetField(ref _margemBrutaEstimada, value))
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

    public string ResultadoLabel => $"R$ {ResultadoEstimado:N2}";
    public string Decisao => ResultadoEstimado >= 0
        ? $"Emitir por {EmpresaCandidata}: margem preservada após custo fiscal/logístico."
        : $"Revisar {EmpresaCandidata}: operação abaixo da margem mínima após custos.";
    public string RankingEmpresas
    {
        get => _rankingEmpresas;
        set => SetField(ref _rankingEmpresas, value);
    }

    private decimal ResultadoEstimado => ParseDecimal(MargemBrutaEstimada) - ParseDecimal(CustoFiscalEstimado) - ParseDecimal(CustoLogisticoEstimado);

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (ParseDecimal(ValorOperacao) <= 0 || ParseDecimal(MargemBrutaEstimada) <= 0)
        {
            Status = "Informe valor da operação e margem bruta antes de salvar a regra fiscal.";
            return;
        }

        var draft = new FiscalRoutingDraft
        {
            Codigo = Codigo,
            Tipo = Tipo,
            EmpresaCandidata = EmpresaCandidata,
            RegimeFiscal = RegimeFiscal,
            UfDestino = UfDestino,
            Cfop = Cfop.Trim(),
            NaturezaOperacao = NaturezaOperacao.Trim(),
            ValorOperacao = ParseDecimal(ValorOperacao),
            CustoFiscalEstimado = ParseDecimal(CustoFiscalEstimado),
            CustoLogisticoEstimado = ParseDecimal(CustoLogisticoEstimado),
            MargemBrutaEstimada = ParseDecimal(MargemBrutaEstimada),
            Decisao = Decisao,
            Observacoes = Observacoes.Trim(),
            Status = "Regra fiscal local registrada"
        };

        var submit = await _api.SubmitOperationAsync(_terminal, "fiscal", Codigo, draft);
        if (submit.Success)
        {
            Status = $"Regra fiscal gravada no servidor: {submit.ServerReference ?? submit.Detail}";
            return;
        }

        var path = await _outbox.SaveOperationAsync("fiscal", Codigo, draft);
        Status = $"API indisponível. Regra fiscal salva em contingência local: {path}. Motivo: {submit.Detail}";
    }

    private void CalcularMelhorEmpresa_Click(object sender, RoutedEventArgs e)
    {
        var valor = ParseDecimal(ValorOperacao);
        var margem = ParseDecimal(MargemBrutaEstimada);
        if (valor <= 0 || margem <= 0)
        {
            Status = "Cálculo bloqueado: informe valor da operação e margem bruta estimada.";
            return;
        }

        var candidatos = Empresas
            .Select(empresa => CalcularCandidato(empresa, valor, margem, UfDestino))
            .OrderByDescending(x => x.Resultado)
            .ThenBy(x => x.CustoFiscal + x.CustoLogistico)
            .ToList();
        var melhor = candidatos[0];

        EmpresaCandidata = melhor.Empresa;
        RegimeFiscal = melhor.Regime;
        CustoFiscalEstimado = melhor.CustoFiscal.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
        CustoLogisticoEstimado = melhor.CustoLogistico.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
        RankingEmpresas = string.Join(Environment.NewLine, candidatos.Select((x, index) =>
            $"{index + 1}. {x.Empresa}: resultado R$ {x.Resultado:N2}, fiscal R$ {x.CustoFiscal:N2}, logística R$ {x.CustoLogistico:N2}"));
        Status = $"Melhor decisão calculada: {melhor.Empresa}, resultado estimado R$ {melhor.Resultado:N2}.";
        AtualizarResumo();
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
        Codigo = $"FISC-{NormalizeCode(Tipo)}-{DateTime.Now:yyyyMMdd-HHmmss}";
        if (clearFields)
        {
            ValorOperacao = "0,00";
            CustoFiscalEstimado = "0,00";
            CustoLogisticoEstimado = "0,00";
            MargemBrutaEstimada = "0,00";
            Observacoes = string.Empty;
        }

        Status = "Novo cálculo fiscal pronto para preenchimento.";
        AtualizarResumo();
    }

    private void AtualizarResumo()
    {
        OnPropertyChanged(nameof(ResultadoLabel));
        OnPropertyChanged(nameof(Decisao));
    }

    private static FiscalCandidateResult CalcularCandidato(string empresa, decimal valor, decimal margem, string ufDestino)
    {
        var perfil = empresa switch
        {
            "Chronnus Relojoaria" => new FiscalProfile("Simples Nacional", 0.062m, 19m),
            "Gran Festas" => new FiscalProfile("Simples Nacional", 0.071m, 16m),
            "Moda Mim" => new FiscalProfile("Simples Nacional", 0.068m, 18m),
            "Estrutural Line" => new FiscalProfile("Lucro Presumido", 0.112m, 24m),
            "Geração Top Mais" => new FiscalProfile("Simples Nacional", 0.059m, 14m),
            _ => new FiscalProfile("Simples Nacional", 0.065m, 15m)
        };
        var foraSp = string.Equals(ufDestino?.Trim(), "SP", StringComparison.OrdinalIgnoreCase) ? 0m : 0.012m;
        var custoFiscal = decimal.Round(valor * (perfil.AliquotaEfetiva + foraSp), 2);
        var custoLogistico = decimal.Round(perfil.CustoLogisticoBase + valor * 0.015m, 2);
        return new FiscalCandidateResult(empresa, perfil.Regime, custoFiscal, custoLogistico, margem - custoFiscal - custoLogistico);
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

    private sealed record FiscalProfile(string Regime, decimal AliquotaEfetiva, decimal CustoLogisticoBase);

    private sealed record FiscalCandidateResult(
        string Empresa,
        string Regime,
        decimal CustoFiscal,
        decimal CustoLogistico,
        decimal Resultado);
}
