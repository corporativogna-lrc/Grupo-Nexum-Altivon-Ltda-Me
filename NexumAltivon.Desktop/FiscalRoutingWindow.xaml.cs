/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7205
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
    private readonly TerminalProfile _profile;
    private string _codigo = string.Empty;
    private string _tipo;
    private string _empresaCandidata = string.Empty;
    private string _regimeFiscal = string.Empty;
    private string _ufOrigem = string.Empty;
    private string _ufDestino = string.Empty;
    private string _cfop = "5102";
    private string _naturezaOperacao = "Venda de mercadoria";
    private string _valorOperacao = "0,00";
    private string _custoFiscalEstimado = "0,00";
    private string _custoLogisticoEstimado = "0,00";
    private string _margemBrutaEstimada = "0,00";
    private string _rankingEmpresas = "Informe UF de origem, UF de destino e valor para consultar o motor fiscal oficial.";
    private string _decisao = "Nenhum roteamento oficial foi consultado.";
    private string _observacoes = string.Empty;
    private string _status = "Pronto para consultar o roteamento fiscal oficial.";

    public FiscalRoutingWindow(string operationType, TerminalProfile profile)
    {
        _profile = profile;
        _tipo = operationType;
        OperationTypes = new ObservableCollection<string>
        {
            "Empresa emissora automática",
            "Regras fiscais do grupo",
            "SPED / EFD / REINF",
            "Tributos e retenções"
        };
        NovoDocumento();
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<string> OperationTypes { get; }
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

    public string UfOrigem
    {
        get => _ufOrigem;
        set => SetField(ref _ufOrigem, value.ToUpperInvariant());
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

    public string ResultadoLabel => string.IsNullOrWhiteSpace(EmpresaCandidata)
        ? "Aguardando consulta"
        : EmpresaCandidata;
    public string Decisao => _decisao;
    public string RankingEmpresas
    {
        get => _rankingEmpresas;
        set => SetField(ref _rankingEmpresas, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void CalcularMelhorEmpresa_Click(object sender, RoutedEventArgs e)
    {
        if (!string.Equals(Tipo, "Empresa emissora automática", StringComparison.Ordinal))
        {
            Status = "Consulta bloqueada: esta janela só possui contrato oficial para empresa emissora automática.";
            return;
        }

        var valor = ParseDecimal(ValorOperacao);
        if (valor <= 0 || !IsUf(UfOrigem) || !IsUf(UfDestino))
        {
            Status = "Consulta bloqueada: informe valor positivo e UFs válidas com duas letras.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_profile.DesktopAccessToken))
        {
            Status = "Consulta bloqueada: o terminal não possui token autenticado.";
            return;
        }

        try
        {
            Status = "Consultando o motor fiscal oficial...";
            var request = new DesktopFiscalRoutingSimulationRequest(
                string.Equals(UfOrigem, UfDestino, StringComparison.OrdinalIgnoreCase)
                    ? "VendaInterna"
                    : "VendaInterestadual",
                valor,
                0m,
                UfOrigem,
                UfDestino,
                null,
                null,
                string.IsNullOrWhiteSpace(NaturezaOperacao) ? null : NaturezaOperacao.Trim(),
                false,
                false,
                true,
                false);
            var result = await _api.SimulateFiscalRoutingAsync(_profile, _profile.DesktopAccessToken, request);
            if (!result.Success || result.Data is null)
            {
                Status = $"Roteamento não confirmado: {result.Detail}";
                return;
            }

            var decision = result.Data;
            if (!decision.Sucesso || string.IsNullOrWhiteSpace(decision.RazaoSocialSelecionada))
            {
                Status = $"O motor fiscal não selecionou empresa: {decision.Resumo}";
                EmpresaCandidata = string.Empty;
                RankingEmpresas = decision.Resumo;
                _decisao = decision.Resumo;
                AtualizarResumo();
                return;
            }

            EmpresaCandidata = decision.RazaoSocialSelecionada;
            RegimeFiscal = decision.Ranking.FirstOrDefault(item =>
                string.Equals(item.RazaoSocial, decision.RazaoSocialSelecionada, StringComparison.OrdinalIgnoreCase))?.RegimeTributario ?? string.Empty;
            var selected = decision.Ranking.FirstOrDefault(item =>
                string.Equals(item.RazaoSocial, decision.RazaoSocialSelecionada, StringComparison.OrdinalIgnoreCase));
            CustoFiscalEstimado = selected?.CustoTributarioEstimado.ToString("N2", CultureInfo.GetCultureInfo("pt-BR")) ?? "0,00";
            CustoLogisticoEstimado = selected?.CustoOperacionalEstimado.ToString("N2", CultureInfo.GetCultureInfo("pt-BR")) ?? "0,00";
            MargemBrutaEstimada = selected?.MargemEstimadaPercentual.ToString("N2", CultureInfo.GetCultureInfo("pt-BR")) ?? "0,00";
            RankingEmpresas = string.Join(Environment.NewLine, decision.Ranking.Select((item, index) =>
                $"{index + 1}. {item.RazaoSocial}: total R$ {item.CustoTotalEstimado:N2}, fiscal R$ {item.CustoTributarioEstimado:N2}, operacional R$ {item.CustoOperacionalEstimado:N2}, margem {item.MargemEstimadaPercentual:N2}%"));
            _decisao = decision.Resumo;
            Status = $"Roteamento oficial confirmado: {decision.RazaoSocialSelecionada}.";
            AtualizarResumo();
        }
        catch (Exception ex)
        {
            Status = $"Roteamento não confirmado e nenhum dado foi gravado: {ex.Message}";
        }
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
            UfOrigem = string.Empty;
            UfDestino = string.Empty;
            EmpresaCandidata = string.Empty;
            RegimeFiscal = string.Empty;
            RankingEmpresas = "Informe UF de origem, UF de destino e valor para consultar o motor fiscal oficial.";
            _decisao = "Nenhum roteamento oficial foi consultado.";
            Observacoes = string.Empty;
        }

        Status = "Nova consulta fiscal pronta para preenchimento.";
        AtualizarResumo();
    }

    private void AtualizarResumo()
    {
        OnPropertyChanged(nameof(ResultadoLabel));
        OnPropertyChanged(nameof(Decisao));
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

    private static bool IsUf(string value)
    {
        return value.Length == 2 && value.All(char.IsLetter);
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
