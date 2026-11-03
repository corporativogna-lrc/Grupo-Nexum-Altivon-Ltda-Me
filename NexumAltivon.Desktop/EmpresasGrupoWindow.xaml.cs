/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7219
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class EmpresasGrupoWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopEmpresaGrupo? _selectedEmpresa;
    private string _status = "Aguardando carregamento da API oficial.";
    private string _selectedDetails = "Nenhuma empresa selecionada.";
    private bool _updatingForm;

    public EmpresasGrupoWindow(TerminalProfile terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        InitializeComponent();
        _terminal = terminal;
        DataContext = this;
        Loaded += async (_, _) => await CarregarAsync();
        Closed += (_, _) =>
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        };
    }

    public ObservableCollection<DesktopEmpresaGrupo> Empresas { get; } = [];

    public DesktopEmpresaGrupo? SelectedEmpresa
    {
        get => _selectedEmpresa;
        private set
        {
            if (!SetField(ref _selectedEmpresa, value))
            {
                return;
            }

            if (value is null)
            {
                SelectedDetails = "Novo cadastro societário/fiscal. Nenhum dado foi gravado.";
                return;
            }

            SelectedDetails = $"ID: {value.Id}\nRazão social: {value.RazaoSocial}\nCNPJ: {value.Cnpj}\nCódigo: {value.CodigoEmpresa ?? "sem código"}\nEstado: {(value.Ativa ? "ativa" : "inativa")}";
            PreencherFormulario(value);
        }
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string SelectedDetails
    {
        get => _selectedDetails;
        private set => SetField(ref _selectedDetails, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task<bool> CarregarAsync()
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de gerir empresas.";
            Empresas.Clear();
            return false;
        }

        Status = "Consultando empresas do grupo na API oficial...";
        var result = await _api.GetEmpresasGrupoAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Empresas não carregadas: {result.Detail}";
            return false;
        }

        var selectedId = _selectedEmpresa?.Id;
        Empresas.Clear();
        foreach (var empresa in result.Data)
        {
            Empresas.Add(empresa);
        }

        SelectedEmpresa = selectedId.HasValue
            ? Empresas.FirstOrDefault(item => item.Id == selectedId.Value)
            : null;
        Status = $"{Empresas.Count} empresa(s) carregada(s) da API e do banco oficial.";
        return true;
    }

    private async void Carregar_Click(object sender, RoutedEventArgs e)
    {
        await CarregarAsync();
    }

    private void EmpresasGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_updatingForm)
        {
            SelectedEmpresa = EmpresasGrid.SelectedItem as DesktopEmpresaGrupo;
        }
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        EmpresasGrid.SelectedItem = null;
        SelectedEmpresa = null;
        LimparFormulario();
        Status = "Novo cadastro pronto para preenchimento. Nenhum dado foi gravado.";
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarFormulario(out var request))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Nenhum dado foi enviado.";
            MessageBox.Show(Status, "Sessão necessária", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selectedId = SelectedEmpresa?.Id;
        Status = selectedId.HasValue
            ? "Atualizando empresa societária/fiscal na API oficial..."
            : "Cadastrando empresa societária/fiscal na API oficial...";
        var result = selectedId.HasValue
            ? await _api.UpdateEmpresaGrupoAsync(_terminal, _terminal.DesktopAccessToken, selectedId.Value, request, _lifetime.Token)
            : await _api.CreateEmpresaGrupoAsync(_terminal, _terminal.DesktopAccessToken, request, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Empresa não persistida: {result.Detail}";
            MessageBox.Show(Status, "Falha ao cadastrar", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var persistedId = result.Data.Id;
        if (!await CarregarAsync())
        {
            Status = "A API respondeu, mas a releitura oficial falhou. A operação não foi apresentada como concluída.";
            return;
        }

        var persisted = Empresas.FirstOrDefault(item => item.Id == persistedId);
        if (persisted is null)
        {
            Status = "A API respondeu, mas a empresa não apareceu na releitura oficial. Persistência não confirmada.";
            MessageBox.Show(Status, "Persistência não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        EmpresasGrid.SelectedItem = persisted;
        SelectedEmpresa = persisted;
        Status = selectedId.HasValue
            ? $"Empresa {persisted.RazaoSocial} atualizada e relida no banco oficial."
            : $"Empresa {persisted.RazaoSocial} persistida e relida no banco oficial.";
    }

    private async void Excluir_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedEmpresa is null)
        {
            Status = "Selecione uma empresa válida antes de desativar.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirmation = MessageBox.Show(
            $"Desativar a empresa {SelectedEmpresa.RazaoSocial}? O registro será preservado no banco com Ativa = false.",
            "Confirmar desativação",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Nenhum dado foi enviado.";
            MessageBox.Show(Status, "Sessão necessária", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var id = SelectedEmpresa.Id;
        Status = "Desativando empresa na API oficial...";
        var result = await _api.DeleteEmpresaGrupoAsync(_terminal, _terminal.DesktopAccessToken, id, _lifetime.Token);
        if (!result.Success || result.Data is null || result.Data.Ativa)
        {
            Status = $"Desativação não confirmada: {result.Detail}";
            MessageBox.Show(Status, "Falha ao desativar", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!await CarregarAsync())
        {
            Status = "A API respondeu, mas a releitura oficial falhou. A desativação não foi apresentada como concluída.";
            return;
        }

        var persisted = Empresas.FirstOrDefault(item => item.Id == id);
        if (persisted is null || persisted.Ativa)
        {
            Status = "A API respondeu, mas Ativa=false não foi confirmado na releitura oficial.";
            MessageBox.Show(Status, "Desativação não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        EmpresasGrid.SelectedItem = persisted;
        SelectedEmpresa = persisted;
        Status = $"Empresa {persisted.RazaoSocial} desativada e relida no banco oficial.";
    }

    private bool ValidarFormulario(out DesktopEmpresaGrupoRequest request)
    {
        request = default!;
        var razaoSocial = RazaoSocialBox.Text.Trim();
        var cnpj = CnpjBox.Text.Trim();
        if (razaoSocial.Length == 0 || cnpj.Length == 0)
        {
            Status = "Razão social e CNPJ são obrigatórios.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!int.TryParse(ProximaNfceBox.Text.Trim(), out var proximaNfce) && !string.IsNullOrWhiteSpace(ProximaNfceBox.Text))
        {
            Status = "O próximo número NFC-e deve ser inteiro ou ficar vazio.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        request = new DesktopEmpresaGrupoRequest(
            TipoCadastro: NullIfWhiteSpace(TipoCadastroBox.Text) ?? "GrupoSocietario",
            RazaoSocial: razaoSocial,
            NomeFantasia: NullIfWhiteSpace(NomeFantasiaBox.Text),
            Cnpj: cnpj,
            InscricaoEstadual: NullIfWhiteSpace(InscricaoEstadualBox.Text),
            InscricaoMunicipal: NullIfWhiteSpace(InscricaoMunicipalBox.Text),
            MatrizFilial: NullIfWhiteSpace(MatrizFilialBox.Text),
            CodigoEmpresa: NullIfWhiteSpace(CodigoEmpresaBox.Text),
            RegimeTributario: NullIfWhiteSpace(RegimeTributarioBox.Text),
            CategoriaFiscal: NullIfWhiteSpace(CategoriaFiscalBox.Text),
            NaturezaOperacaoPadrao: NullIfWhiteSpace(NaturezaOperacaoBox.Text),
            EmailFiscal: NullIfWhiteSpace(EmailFiscalBox.Text),
            EmailComercial: NullIfWhiteSpace(EmailComercialBox.Text),
            Telefone: NullIfWhiteSpace(TelefoneBox.Text),
            Cep: NullIfWhiteSpace(CepBox.Text),
            Logradouro: NullIfWhiteSpace(LogradouroBox.Text),
            Numero: NullIfWhiteSpace(NumeroBox.Text),
            Bairro: NullIfWhiteSpace(BairroBox.Text),
            Cidade: NullIfWhiteSpace(CidadeBox.Text),
            Estado: NullIfWhiteSpace(EstadoBox.Text),
            Pais: "Brasil",
            AmbienteNfe: NullIfWhiteSpace(AmbienteNfeBox.Text),
            SerieNfe: NullIfWhiteSpace(SerieNfeBox.Text),
            SerieNfce: NullIfWhiteSpace(SerieNfceBox.Text),
            ModeloDocumentoPdv: NullIfWhiteSpace(ModeloPdvBox.Text) ?? "NFCe",
            AmbienteNfce: NullIfWhiteSpace(AmbienteNfeBox.Text),
            ProximaNfceNumero: string.IsNullOrWhiteSpace(ProximaNfceBox.Text) ? null : proximaNfce,
            PerfilTributacao: "TributacaoAtual",
            PermiteNfeEntrada: PermiteNfeEntradaBox.IsChecked == true,
            PermiteNfeSaida: PermiteNfeSaidaBox.IsChecked == true,
            PermiteDropshipping: false,
            PermiteMarketplace: PermiteMarketplaceBox.IsChecked == true,
            EmitentePreferencial: EmitentePreferencialBox.IsChecked == true,
            Ativa: AtivaBox.IsChecked == true,
            Observacoes: NullIfWhiteSpace(ObservacoesBox.Text));
            request = request with
            {
                Crt = SelectedEmpresa?.Crt,
                CnaePrincipal = SelectedEmpresa?.CnaePrincipal,
                CnaesSecundarios = SelectedEmpresa?.CnaesSecundarios,
                NcmPadrao = SelectedEmpresa?.NcmPadrao,
                ResponsavelLegal = SelectedEmpresa?.ResponsavelLegal,
                ResponsavelFiscal = SelectedEmpresa?.ResponsavelFiscal,
                NfceCsc = SelectedEmpresa?.NfceCsc,
                NfceCscIdToken = SelectedEmpresa?.NfceCscIdToken,
                PdvSerieSat = SelectedEmpresa?.PdvSerieSat,
                PdvImpressoraFiscal = SelectedEmpresa?.PdvImpressoraFiscal,
                PdvNomeCaixaPadrao = SelectedEmpresa?.PdvNomeCaixaPadrao,
                PdvContingenciaOffline = SelectedEmpresa?.PdvContingenciaOffline,
                ProximaNfeNumero = SelectedEmpresa?.ProximaNfeNumero,
                UsaStLegado = SelectedEmpresa?.UsaStLegado,
                DestacaIcmsStSeparado = SelectedEmpresa?.DestacaIcmsStSeparado
            };
        return true;
    }

    private void PreencherFormulario(DesktopEmpresaGrupo empresa)
    {
        _updatingForm = true;
        try
        {
            TipoCadastroBox.Text = empresa.TipoCadastro ?? "GrupoSocietario";
            CodigoEmpresaBox.Text = empresa.CodigoEmpresa ?? string.Empty;
            RazaoSocialBox.Text = empresa.RazaoSocial;
            NomeFantasiaBox.Text = empresa.NomeFantasia ?? string.Empty;
            CnpjBox.Text = empresa.Cnpj;
            MatrizFilialBox.Text = empresa.MatrizFilial ?? string.Empty;
            InscricaoEstadualBox.Text = empresa.InscricaoEstadual ?? string.Empty;
            InscricaoMunicipalBox.Text = empresa.InscricaoMunicipal ?? string.Empty;
            RegimeTributarioBox.Text = empresa.RegimeTributario ?? string.Empty;
            CategoriaFiscalBox.Text = empresa.CategoriaFiscal ?? string.Empty;
            NaturezaOperacaoBox.Text = empresa.NaturezaOperacaoPadrao ?? string.Empty;
            EmailFiscalBox.Text = empresa.EmailFiscal ?? string.Empty;
            EmailComercialBox.Text = empresa.EmailComercial ?? string.Empty;
            TelefoneBox.Text = empresa.Telefone ?? string.Empty;
            CepBox.Text = empresa.Cep ?? string.Empty;
            EstadoBox.Text = empresa.Estado ?? string.Empty;
            LogradouroBox.Text = empresa.Logradouro ?? string.Empty;
            NumeroBox.Text = empresa.Numero ?? string.Empty;
            BairroBox.Text = empresa.Bairro ?? string.Empty;
            CidadeBox.Text = empresa.Cidade ?? string.Empty;
            AmbienteNfeBox.Text = empresa.AmbienteNfe ?? "HOMOLOGACAO";
            SerieNfeBox.Text = empresa.SerieNfe ?? string.Empty;
            SerieNfceBox.Text = empresa.SerieNfce ?? string.Empty;
            ModeloPdvBox.Text = empresa.ModeloDocumentoPdv ?? "NFCe";
            ProximaNfceBox.Text = empresa.ProximaNfceNumero?.ToString() ?? string.Empty;
            PermiteNfeEntradaBox.IsChecked = empresa.PermiteNfeEntrada;
            PermiteNfeSaidaBox.IsChecked = empresa.PermiteNfeSaida;
            PermiteMarketplaceBox.IsChecked = empresa.PermiteMarketplace;
            EmitentePreferencialBox.IsChecked = empresa.EmitentePreferencial;
            AtivaBox.IsChecked = empresa.Ativa;
            ObservacoesBox.Text = empresa.Observacoes ?? string.Empty;
        }
        finally
        {
            _updatingForm = false;
        }
    }

    private void LimparFormulario()
    {
        _updatingForm = true;
        try
        {
            TipoCadastroBox.Text = "GrupoSocietario";
            CodigoEmpresaBox.Text = string.Empty;
            RazaoSocialBox.Text = string.Empty;
            NomeFantasiaBox.Text = string.Empty;
            CnpjBox.Text = string.Empty;
            MatrizFilialBox.Text = string.Empty;
            InscricaoEstadualBox.Text = string.Empty;
            InscricaoMunicipalBox.Text = string.Empty;
            RegimeTributarioBox.Text = string.Empty;
            CategoriaFiscalBox.Text = string.Empty;
            NaturezaOperacaoBox.Text = string.Empty;
            EmailFiscalBox.Text = string.Empty;
            EmailComercialBox.Text = string.Empty;
            TelefoneBox.Text = string.Empty;
            CepBox.Text = string.Empty;
            EstadoBox.Text = string.Empty;
            LogradouroBox.Text = string.Empty;
            NumeroBox.Text = string.Empty;
            BairroBox.Text = string.Empty;
            CidadeBox.Text = string.Empty;
            AmbienteNfeBox.Text = "HOMOLOGACAO";
            SerieNfeBox.Text = string.Empty;
            SerieNfceBox.Text = string.Empty;
            ModeloPdvBox.Text = "NFCe";
            ProximaNfceBox.Text = string.Empty;
            PermiteNfeEntradaBox.IsChecked = true;
            PermiteNfeSaidaBox.IsChecked = true;
            PermiteMarketplaceBox.IsChecked = false;
            EmitentePreferencialBox.IsChecked = false;
            AtivaBox.IsChecked = true;
            ObservacoesBox.Text = string.Empty;
        }
        finally
        {
            _updatingForm = false;
        }
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
