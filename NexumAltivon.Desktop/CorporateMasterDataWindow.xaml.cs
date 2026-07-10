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

public partial class CorporateMasterDataWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly LocalOutboxService _outbox = new();
    private readonly TerminalProfile _terminal = new();
    private string _codigo = string.Empty;
    private string _tipoCadastro;
    private string _nomeRazaoSocial = string.Empty;
    private string _documentoFiscal = string.Empty;
    private string _inscricaoEstadualMunicipal = string.Empty;
    private string _email = string.Empty;
    private string _telefone = string.Empty;
    private string _cep = string.Empty;
    private string _enderecoCompleto = string.Empty;
    private string _regimeFiscal = "Não aplicável";
    private string _contaContabil = string.Empty;
    private string _centroCusto = string.Empty;
    private string _departamento = string.Empty;
    private string _cargoFuncao = string.Empty;
    private string _tipoContrato = "Não aplicável";
    private string _salarioBase = "0,00";
    private DateTime? _dataAdmissao;
    private DateTime? _dataDesligamento;
    private string _nivelAcesso = "Operacional";
    private string _regrasFiscaisComerciais = string.Empty;
    private string _observacoes = string.Empty;
    private string _status = "Pronto para registrar cadastro corporativo.";

    public CorporateMasterDataWindow(string cadastroType)
    {
        _tipoCadastro = cadastroType;
        CadastroTypes = new ObservableCollection<string>
        {
            "Clientes",
            "Fornecedores",
            "Produtos e serviços",
            "Categorias e subcategorias",
            "Empresas do grupo",
            "Centros de custo",
            "Usuários, perfis e permissões",
            "Colaboradores / RH / folha"
        };
        FiscalRegimes = new ObservableCollection<string>
        {
            "Não aplicável",
            "Simples Nacional",
            "Lucro Presumido",
            "Lucro Real",
            "MEI",
            "Consumidor final",
            "Fornecedor estrangeiro"
        };
        AccessLevels = new ObservableCollection<string>
        {
            "Operacional",
            "Supervisor",
            "Gerencial",
            "Diretoria",
            "Administrador",
            "Cliente sem acesso ao Genesis"
        };
        ContractTypes = new ObservableCollection<string>
        {
            "Não aplicável",
            "CLT",
            "MEI/PJ",
            "Temporário",
            "Estágio",
            "Terceirizado",
            "Sócio/administrador"
        };

        NovoDocumento();
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<string> CadastroTypes { get; }
    public ObservableCollection<string> FiscalRegimes { get; }
    public ObservableCollection<string> AccessLevels { get; }
    public ObservableCollection<string> ContractTypes { get; }
    public string WindowTitle => $"GenesisGest.Net - {TipoCadastro}";

    public string Codigo
    {
        get => _codigo;
        set => SetField(ref _codigo, value);
    }

    public string TipoCadastro
    {
        get => _tipoCadastro;
        set
        {
            if (SetField(ref _tipoCadastro, value))
            {
                NovoDocumento(false);
                OnPropertyChanged(nameof(WindowTitle));
                AtualizarResumo();
            }
        }
    }

    public string NomeRazaoSocial
    {
        get => _nomeRazaoSocial;
        set
        {
            if (SetField(ref _nomeRazaoSocial, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string DocumentoFiscal
    {
        get => _documentoFiscal;
        set
        {
            if (SetField(ref _documentoFiscal, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string InscricaoEstadualMunicipal
    {
        get => _inscricaoEstadualMunicipal;
        set => SetField(ref _inscricaoEstadualMunicipal, value);
    }

    public string Email
    {
        get => _email;
        set => SetField(ref _email, value);
    }

    public string Telefone
    {
        get => _telefone;
        set => SetField(ref _telefone, value);
    }

    public string Cep
    {
        get => _cep;
        set => SetField(ref _cep, value);
    }

    public string EnderecoCompleto
    {
        get => _enderecoCompleto;
        set
        {
            if (SetField(ref _enderecoCompleto, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string RegimeFiscal
    {
        get => _regimeFiscal;
        set => SetField(ref _regimeFiscal, value);
    }

    public string ContaContabil
    {
        get => _contaContabil;
        set
        {
            if (SetField(ref _contaContabil, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string CentroCusto
    {
        get => _centroCusto;
        set => SetField(ref _centroCusto, value);
    }

    public string Departamento
    {
        get => _departamento;
        set => SetField(ref _departamento, value);
    }

    public string CargoFuncao
    {
        get => _cargoFuncao;
        set => SetField(ref _cargoFuncao, value);
    }

    public string TipoContrato
    {
        get => _tipoContrato;
        set => SetField(ref _tipoContrato, value);
    }

    public string SalarioBase
    {
        get => _salarioBase;
        set => SetField(ref _salarioBase, value);
    }

    public DateTime? DataAdmissao
    {
        get => _dataAdmissao;
        set => SetField(ref _dataAdmissao, value);
    }

    public DateTime? DataDesligamento
    {
        get => _dataDesligamento;
        set => SetField(ref _dataDesligamento, value);
    }

    public string NivelAcesso
    {
        get => _nivelAcesso;
        set => SetField(ref _nivelAcesso, value);
    }

    public string RegrasFiscaisComerciais
    {
        get => _regrasFiscaisComerciais;
        set => SetField(ref _regrasFiscaisComerciais, value);
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

    public bool CadastroCompleto => !string.IsNullOrWhiteSpace(NomeRazaoSocial)
        && !string.IsNullOrWhiteSpace(DocumentoFiscal)
        && !string.IsNullOrWhiteSpace(EnderecoCompleto)
        && !string.IsNullOrWhiteSpace(ContaContabil);

    public string StatusCadastro => CadastroCompleto ? "Cadastro apto para uso operacional" : "Cadastro incompleto";
    public string ValidacaoDetalhe => CadastroCompleto
        ? "Registro pode alimentar fiscal, financeiro, comercial, estoque, RH ou acesso conforme o tipo."
        : "Preencha nome, documento, endereço e conta contábil para evitar travas fiscais/financeiras posteriores.";

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        var draft = new CorporateMasterDataDraft
        {
            Codigo = Codigo,
            TipoCadastro = TipoCadastro.Trim(),
            NomeRazaoSocial = NomeRazaoSocial.Trim(),
            DocumentoFiscal = OnlyDigits(DocumentoFiscal),
            InscricaoEstadualMunicipal = InscricaoEstadualMunicipal.Trim(),
            Email = Email.Trim(),
            Telefone = Telefone.Trim(),
            Cep = OnlyDigits(Cep),
            EnderecoCompleto = EnderecoCompleto.Trim(),
            RegimeFiscal = RegimeFiscal.Trim(),
            ContaContabil = ContaContabil.Trim(),
            CentroCusto = CentroCusto.Trim(),
            Departamento = Departamento.Trim(),
            CargoFuncao = CargoFuncao.Trim(),
            TipoContrato = TipoContrato.Trim(),
            SalarioBase = ParseDecimal(SalarioBase),
            DataAdmissao = DataAdmissao,
            DataDesligamento = DataDesligamento,
            NivelAcesso = NivelAcesso.Trim(),
            RegrasFiscaisComerciais = RegrasFiscaisComerciais.Trim(),
            CadastroCompleto = CadastroCompleto,
            Status = CadastroCompleto ? "Cadastro corporativo registrado" : "Cadastro registrado pendente de complemento",
            Observacoes = Observacoes.Trim()
        };

        var submit = await _api.SubmitOperationAsync(_terminal, "cadastros-corporativos", Codigo, draft);
        if (submit.Success)
        {
            Status = $"Cadastro gravado no servidor: {submit.ServerReference ?? submit.Detail}";
            return;
        }

        var path = await _outbox.SaveOperationAsync("cadastros-corporativos", Codigo, draft);
        Status = $"API indisponível. Cadastro salvo em contingência local: {path}. Motivo: {submit.Detail}";
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
        Codigo = $"CAD-{NormalizeCode(TipoCadastro)}-{DateTime.Now:yyyyMMdd-HHmmss}";
        if (clearFields)
        {
            NomeRazaoSocial = string.Empty;
            DocumentoFiscal = string.Empty;
            InscricaoEstadualMunicipal = string.Empty;
            Email = string.Empty;
            Telefone = string.Empty;
            Cep = string.Empty;
            EnderecoCompleto = string.Empty;
            ContaContabil = string.Empty;
            CentroCusto = string.Empty;
            Departamento = string.Empty;
            CargoFuncao = string.Empty;
            SalarioBase = "0,00";
            DataAdmissao = null;
            DataDesligamento = null;
            RegrasFiscaisComerciais = string.Empty;
            Observacoes = string.Empty;
        }

        Status = "Novo cadastro corporativo pronto para preenchimento.";
        AtualizarResumo();
    }

    private void AtualizarResumo()
    {
        OnPropertyChanged(nameof(CadastroCompleto));
        OnPropertyChanged(nameof(StatusCadastro));
        OnPropertyChanged(nameof(ValidacaoDetalhe));
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

    private static string OnlyDigits(string value)
    {
        return new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
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
