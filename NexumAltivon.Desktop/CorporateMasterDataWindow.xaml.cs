/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7214
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
    private readonly TerminalProfile _profile;
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

    public CorporateMasterDataWindow(string cadastroType, TerminalProfile profile)
    {
        _profile = profile;
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

    public bool CadastroCompleto => TipoCadastro switch
    {
        "Categorias e subcategorias" => !string.IsNullOrWhiteSpace(NomeRazaoSocial),
        "Centros de custo" => !string.IsNullOrWhiteSpace(NomeRazaoSocial),
        "Empresas do grupo" => !string.IsNullOrWhiteSpace(NomeRazaoSocial)
            && OnlyDigits(DocumentoFiscal).Length == 14,
        _ => false
    };

    public string StatusCadastro => CadastroCompleto ? "Cadastro apto para uso operacional" : "Cadastro incompleto";
    public string ValidacaoDetalhe => TipoCadastro switch
    {
        "Categorias e subcategorias" => CadastroCompleto
            ? "A categoria será persistida em /api/categorias e relida após a resposta."
            : "Informe o nome da categoria.",
        "Centros de custo" => CadastroCompleto
            ? "O centro será persistido em /api/centros-custo e relido após a resposta."
            : "Informe o nome do centro de custo.",
        "Empresas do grupo" => CadastroCompleto
            ? "A empresa será persistida em /api/erp/empresas e relida após a resposta."
            : "Informe nome e CNPJ com 14 dígitos.",
        _ => "Este tipo não possui contrato de gravação nesta tela; nenhum dado será enviado."
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_profile.DesktopAccessToken))
        {
            Status = "Gravação bloqueada: o terminal não possui token autenticado.";
            return;
        }

        try
        {
            switch (TipoCadastro)
            {
                case "Categorias e subcategorias":
                    await SalvarCategoriaAsync();
                    break;
                case "Centros de custo":
                    await SalvarCentroCustoAsync();
                    break;
                case "Empresas do grupo":
                    await SalvarEmpresaGrupoAsync();
                    break;
                default:
                    Status = "Nenhum dado foi gravado: este tipo de cadastro ainda não possui endpoint de domínio oficial nesta tela.";
                    break;
            }
        }
        catch (Exception ex)
        {
            Status = $"Gravação não confirmada e nenhum dado local foi criado: {ex.Message}";
        }
    }

    private async Task SalvarCategoriaAsync()
    {
        if (string.IsNullOrWhiteSpace(NomeRazaoSocial))
        {
            Status = "Gravação bloqueada: informe o nome da categoria.";
            return;
        }

        var request = new DesktopCategoriaRequest(
            Id: NormalizeCode(Codigo),
            Nome: NomeRazaoSocial.Trim(),
            Descricao: string.IsNullOrWhiteSpace(RegrasFiscaisComerciais) ? Observacoes.Trim() : RegrasFiscaisComerciais.Trim(),
            Ativa: true);
        var created = await _api.CreateCategoriaAsync(_profile, _profile.DesktopAccessToken, request);
        if (!created.Success || created.Data is null)
        {
            Status = $"Categoria não persistida: {created.Detail}";
            return;
        }

        var reread = await _api.GetCategoriasAsync(_profile, _profile.DesktopAccessToken);
        var confirmed = reread.Success && reread.Data?.Any(item =>
            string.Equals(item.Id, created.Data.Id, StringComparison.OrdinalIgnoreCase)) == true;
        Status = confirmed
            ? $"Categoria persistida e relida no servidor: {created.Data.Id}."
            : $"A API aceitou a categoria, mas a releitura não confirmou o registro: {created.Data.Id}.";
    }

    private async Task SalvarCentroCustoAsync()
    {
        if (string.IsNullOrWhiteSpace(NomeRazaoSocial))
        {
            Status = "Gravação bloqueada: informe o nome do centro de custo.";
            return;
        }

        var request = new DesktopCentroCustoRequest(
            NormalizeCode(Codigo),
            NomeRazaoSocial.Trim(),
            string.IsNullOrWhiteSpace(RegrasFiscaisComerciais) ? null : RegrasFiscaisComerciais.Trim(),
            string.IsNullOrWhiteSpace(Observacoes) ? null : Observacoes.Trim(),
            "Operacional",
            null,
            null,
            "A");
        var created = await _api.CreateCentroCustoAsync(_profile, _profile.DesktopAccessToken, request);
        if (!created.Success || created.Data is null)
        {
            Status = $"Centro de custo não persistido: {created.Detail}";
            return;
        }

        var reread = await _api.GetCentrosCustoAsync(_profile, _profile.DesktopAccessToken);
        var confirmed = reread.Success && reread.Data?.Any(item => item.Id == created.Data.Id) == true;
        Status = confirmed
            ? $"Centro de custo persistido e relido no servidor: {created.Data.Codigo}."
            : $"A API aceitou o centro, mas a releitura não confirmou o registro: {created.Data.Id}.";
    }

    private async Task SalvarEmpresaGrupoAsync()
    {
        var cnpj = OnlyDigits(DocumentoFiscal);
        if (string.IsNullOrWhiteSpace(NomeRazaoSocial) || cnpj.Length != 14)
        {
            Status = "Gravação bloqueada: informe nome e CNPJ com 14 dígitos.";
            return;
        }

        var request = new DesktopEmpresaGrupoRequest(
            TipoCadastro: "EmpresaGrupo",
            RazaoSocial: NomeRazaoSocial.Trim(),
            NomeFantasia: NomeRazaoSocial.Trim(),
            Cnpj: cnpj,
            InscricaoEstadual: string.IsNullOrWhiteSpace(InscricaoEstadualMunicipal) ? null : InscricaoEstadualMunicipal.Trim(),
            InscricaoMunicipal: null,
            MatrizFilial: "Filial",
            CodigoEmpresa: NormalizeCode(Codigo),
            RegimeTributario: RegimeFiscal == "Não aplicável" ? null : RegimeFiscal.Trim(),
            Crt: null,
            CnaePrincipal: null,
            CnaesSecundarios: null,
            CategoriaFiscal: null,
            SubcategoriaFiscal: null,
            NcmPadrao: null,
            NaturezaOperacaoPadrao: string.IsNullOrWhiteSpace(RegrasFiscaisComerciais) ? null : RegrasFiscaisComerciais.Trim(),
            ResponsavelLegal: null,
            ResponsavelFiscal: null,
            EmailFiscal: string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            EmailComercial: string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            Telefone: string.IsNullOrWhiteSpace(Telefone) ? null : Telefone.Trim(),
            Whatsapp: string.IsNullOrWhiteSpace(Telefone) ? null : Telefone.Trim(),
            Cep: string.IsNullOrWhiteSpace(Cep) ? null : OnlyDigits(Cep),
            Logradouro: string.IsNullOrWhiteSpace(EnderecoCompleto) ? null : EnderecoCompleto.Trim(),
            Numero: null,
            Complemento: null,
            Bairro: null,
            Cidade: null,
            Estado: null,
            Pais: "Brasil",
            AmbienteNfe: null,
            SerieNfe: null,
            SerieNfce: null,
            ModeloDocumentoPdv: null,
            AmbienteNfce: null,
            ProximaNfceNumero: null,
            NfceCsc: null,
            NfceCscIdToken: null,
            PdvSerieSat: null,
            PdvImpressoraFiscal: null,
            PdvNomeCaixaPadrao: null,
            PdvContingenciaOffline: null,
            ProximaNfeNumero: null,
            CfopPadraoInterno: null,
            CfopPadraoInterestadual: null,
            AliquotaIcmsInterna: null,
            AliquotaIcmsInterestadual: null,
            AliquotaPis: null,
            AliquotaCofins: null,
            AliquotaIss: null,
            AliquotaIpi: null,
            CargaTributariaPercentual: null,
            PerfilTributacao: null,
            UsaStLegado: null,
            DestacaIcmsStSeparado: null,
            CustoOperacionalPercentual: null,
            MargemMinimaPercentual: null,
            PrioridadeFiscal: null,
            PermiteNfeEntrada: null,
            PermiteNfeSaida: null,
            PermiteDropshipping: null,
            PermiteMarketplace: null,
            EmitentePreferencial: null,
            Ativa: true,
            BeneficiosEstrategicos: null,
            ContratoResumo: null,
            Observacoes: string.IsNullOrWhiteSpace(Observacoes) ? null : Observacoes.Trim());
        var created = await _api.CreateEmpresaGrupoAsync(_profile, _profile.DesktopAccessToken, request);
        if (!created.Success || created.Data is null)
        {
            Status = $"Empresa não persistida: {created.Detail}";
            return;
        }

        var reread = await _api.GetEmpresasGrupoAsync(_profile, _profile.DesktopAccessToken);
        var confirmed = reread.Success && reread.Data?.Any(item => item.Id == created.Data.Id) == true;
        Status = confirmed
            ? $"Empresa persistida e relida no servidor: {created.Data.RazaoSocial}."
            : $"A API aceitou a empresa, mas a releitura não confirmou o registro: {created.Data.Id}.";
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
