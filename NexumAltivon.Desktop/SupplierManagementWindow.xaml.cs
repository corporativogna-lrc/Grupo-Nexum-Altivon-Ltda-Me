/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7186
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class SupplierManagementWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly HashSet<int> _contactAuditIds = [];
    private DesktopFornecedor? _selectedFornecedor;
    private DesktopFornecedorContato? _selectedContato;
    private bool _isBusy;
    private int _selectedTabIndex;
    private string _status = "Aguardando leitura dos fornecedores.";
    private string _razaoSocial = string.Empty;
    private string _nomeFantasia = string.Empty;
    private string _documento = string.Empty;
    private string _inscricaoEstadual = string.Empty;
    private string _email = string.Empty;
    private string _telefone = string.Empty;
    private string _whatsapp = string.Empty;
    private string _segmento = string.Empty;
    private string _endereco = string.Empty;
    private string _cidade = string.Empty;
    private string _estado = string.Empty;
    private string _cep = string.Empty;
    private string _lojaVinculadaIdText = string.Empty;
    private string _comissaoPercentualText = "0,00";
    private string _prazoEntregaDiasText = "7";
    private string _fornecedorStatus = "Ativo";
    private string _observacoes = string.Empty;
    private string _contatoNome = string.Empty;
    private string _contatoCargo = string.Empty;
    private string _contatoEmail = string.Empty;
    private string _contatoTelefone = string.Empty;
    private string _contatoCelular = string.Empty;
    private bool _contatoPrincipal;
    private bool _contatoAtivo = true;

    public SupplierManagementWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        StatusOptions = new ObservableCollection<string> { "Ativo", "Pendente", "Bloqueado", "Inativo" };
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) =>
        {
            FitToWorkArea();
            await RefreshFornecedoresAsync();
        };
    }

    public ObservableCollection<DesktopFornecedor> Fornecedores { get; } = [];
    public ObservableCollection<DesktopFornecedorContato> Contatos { get; } = [];
    public ObservableCollection<DesktopAuditoriaOperacional> Auditoria { get; } = [];
    public ObservableCollection<string> StatusOptions { get; }

    public DesktopFornecedor? SelectedFornecedor
    {
        get => _selectedFornecedor;
        set
        {
            if (!SetField(ref _selectedFornecedor, value))
            {
                return;
            }

            if (value is not null)
            {
                LoadFornecedorForm(value);
                _ = LoadFornecedorDependenciasAsync(value.Id);
            }

            NotifyFornecedorState();
        }
    }

    public DesktopFornecedorContato? SelectedContato
    {
        get => _selectedContato;
        set
        {
            if (!SetField(ref _selectedContato, value))
            {
                return;
            }

            if (value is not null)
            {
                LoadContatoForm(value);
            }

            OnPropertyChanged(nameof(ContactVersionLabel));
        }
    }

    public int SelectedTabIndex { get => _selectedTabIndex; set => SetField(ref _selectedTabIndex, value); }
    public string Status { get => _status; private set => SetField(ref _status, value); }
    public string RazaoSocial { get => _razaoSocial; set { if (SetField(ref _razaoSocial, value)) OnPropertyChanged(nameof(CadastroValidation)); } }
    public string NomeFantasia { get => _nomeFantasia; set => SetField(ref _nomeFantasia, value); }
    public string Documento { get => _documento; set => SetField(ref _documento, value); }
    public string InscricaoEstadual { get => _inscricaoEstadual; set => SetField(ref _inscricaoEstadual, value); }
    public string Email { get => _email; set => SetField(ref _email, value); }
    public string Telefone { get => _telefone; set => SetField(ref _telefone, value); }
    public string Whatsapp { get => _whatsapp; set => SetField(ref _whatsapp, value); }
    public string Segmento { get => _segmento; set => SetField(ref _segmento, value); }
    public string Endereco { get => _endereco; set => SetField(ref _endereco, value); }
    public string Cidade { get => _cidade; set => SetField(ref _cidade, value); }
    public string Estado { get => _estado; set => SetField(ref _estado, value); }
    public string Cep { get => _cep; set => SetField(ref _cep, value); }
    public string LojaVinculadaIdText { get => _lojaVinculadaIdText; set => SetField(ref _lojaVinculadaIdText, value); }
    public string ComissaoPercentualText { get => _comissaoPercentualText; set => SetField(ref _comissaoPercentualText, value); }
    public string PrazoEntregaDiasText { get => _prazoEntregaDiasText; set => SetField(ref _prazoEntregaDiasText, value); }
    public string FornecedorStatus { get => _fornecedorStatus; set => SetField(ref _fornecedorStatus, value); }
    public string Observacoes { get => _observacoes; set => SetField(ref _observacoes, value); }
    public string ContatoNome { get => _contatoNome; set => SetField(ref _contatoNome, value); }
    public string ContatoCargo { get => _contatoCargo; set => SetField(ref _contatoCargo, value); }
    public string ContatoEmail { get => _contatoEmail; set => SetField(ref _contatoEmail, value); }
    public string ContatoTelefone { get => _contatoTelefone; set => SetField(ref _contatoTelefone, value); }
    public string ContatoCelular { get => _contatoCelular; set => SetField(ref _contatoCelular, value); }
    public bool ContatoPrincipal { get => _contatoPrincipal; set => SetField(ref _contatoPrincipal, value); }
    public bool ContatoAtivo { get => _contatoAtivo; set => SetField(ref _contatoAtivo, value); }

    public bool CanOperate => !_isBusy && !string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken);
    public bool HasPersistedFornecedor => SelectedFornecedor is { Id: > 0 };
    public string FornecedorCountLabel => $"{Fornecedores.Count} registro(s)";
    public string CadastroReference => SelectedFornecedor is null ? "Novo fornecedor sem identificador persistido" : $"Fornecedor #{SelectedFornecedor.Id}";
    public string RowVersionLabel => SelectedFornecedor is null ? "Nova versão será gerada ao salvar" : $"Versão {ShortVersion(SelectedFornecedor.RowVersion)}";
    public string CadastroValidation => string.IsNullOrWhiteSpace(RazaoSocial)
        ? "Razão social obrigatória. Documento, e-mail, UF, CEP, comissão, prazo e loja serão validados pela API."
        : "Cadastro pronto para validação definitiva pela API oficial.";
    public string ContactHeader => SelectedFornecedor is null ? "Selecione um fornecedor persistido" : $"Contatos de {SelectedFornecedor.Nome}";
    public string ContactVersionLabel => SelectedContato is null
        ? "Novo contato: a versão será gerada pelo banco."
        : $"Contato #{SelectedContato.Id} / versão {ShortVersion(SelectedContato.RowVersion)}";
    public string TenantLabel => $"Loja {(_terminal.StoreCode ?? string.Empty).Trim()} / terminal {(_terminal.TerminalCode ?? string.Empty).Trim()}";

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Atualizar_Click(object sender, RoutedEventArgs e) => await RefreshFornecedoresAsync();

    private void NovoFornecedor_Click(object sender, RoutedEventArgs e)
    {
        SelectedFornecedor = null;
        ClearFornecedorForm();
        Contatos.Clear();
        Auditoria.Clear();
        _contactAuditIds.Clear();
        SelectedTabIndex = 0;
        Status = "Novo fornecedor pronto para preenchimento. Nenhuma gravação foi realizada.";
    }

    private async void SalvarFornecedor_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureSession() || _isBusy)
        {
            return;
        }

        if (!TryBuildFornecedorRequest(out var request, out var validationError))
        {
            Status = validationError;
            return;
        }

        SetBusy(true);
        try
        {
            var previousVersion = SelectedFornecedor?.RowVersion;
            var result = SelectedFornecedor is null
                ? await _api.CreateFornecedorAsync(_terminal, _terminal.DesktopAccessToken, request!)
                : await _api.UpdateFornecedorAsync(_terminal, _terminal.DesktopAccessToken, SelectedFornecedor.Id, request!);
            if (!result.Success || result.Data is null)
            {
                Status = $"Fornecedor não gravado: {result.Detail}";
                return;
            }

            var persisted = await _api.GetFornecedorAsync(_terminal, _terminal.DesktopAccessToken, result.Data.Id);
            if (!persisted.Success || persisted.Data is null)
            {
                Status = $"A API informou gravação do fornecedor #{result.Data.Id}, mas a releitura falhou: {persisted.Detail}. Consulte antes de repetir.";
                return;
            }

            if (!string.Equals(result.Data.RowVersion, persisted.Data.RowVersion, StringComparison.Ordinal)
                || (!string.IsNullOrWhiteSpace(previousVersion)
                    && string.Equals(previousVersion, persisted.Data.RowVersion, StringComparison.Ordinal)))
            {
                Status = "A releitura não confirmou uma nova versão do fornecedor. A operação não será apresentada como concluída.";
                return;
            }

            var persistedId = persisted.Data.Id;
            await RefreshFornecedoresAsync(persistedId, allowBusy: true);
            Status = $"Fornecedor #{persistedId} gravado e relido no banco oficial.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void NovoContato_Click(object sender, RoutedEventArgs e)
    {
        SelectedContato = null;
        ClearContatoForm();
        Status = "Novo contato pronto para preenchimento. Nenhuma gravação foi realizada.";
    }

    private async void SalvarContato_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureSession() || _isBusy || SelectedFornecedor is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(ContatoNome))
        {
            Status = "Nome do contato é obrigatório.";
            return;
        }

        var request = new DesktopFornecedorContatoRequest(
            ContatoNome.Trim(),
            NullIfWhiteSpace(ContatoCargo),
            NullIfWhiteSpace(ContatoEmail),
            NullIfWhiteSpace(ContatoTelefone),
            NullIfWhiteSpace(ContatoCelular),
            ContatoPrincipal,
            ContatoAtivo,
            SelectedContato?.RowVersion);

        SetBusy(true);
        try
        {
            var previousVersion = SelectedContato?.RowVersion;
            var result = SelectedContato is null
                ? await _api.CreateFornecedorContatoAsync(_terminal, _terminal.DesktopAccessToken, SelectedFornecedor.Id, request)
                : await _api.UpdateFornecedorContatoAsync(_terminal, _terminal.DesktopAccessToken, SelectedFornecedor.Id, SelectedContato.Id, request);
            if (!result.Success || result.Data is null)
            {
                Status = $"Contato não gravado: {result.Detail}";
                return;
            }

            var contacts = await _api.GetFornecedorContatosAsync(_terminal, _terminal.DesktopAccessToken, SelectedFornecedor.Id);
            var persisted = contacts.Data?.FirstOrDefault(item => item.Id == result.Data.Id);
            if (!contacts.Success || persisted is null
                || !string.Equals(persisted.RowVersion, result.Data.RowVersion, StringComparison.Ordinal)
                || (!string.IsNullOrWhiteSpace(previousVersion)
                    && string.Equals(previousVersion, persisted.RowVersion, StringComparison.Ordinal)))
            {
                Status = $"A API informou gravação do contato #{result.Data.Id}, mas a releitura não confirmou a nova versão. Consulte antes de repetir.";
                return;
            }

            _contactAuditIds.Add(persisted.Id);
            ReplaceCollection(Contatos, contacts.Data!);
            SelectedContato = Contatos.First(item => item.Id == persisted.Id);
            await LoadAuditoriaAsync(SelectedFornecedor.Id);
            Status = $"Contato #{persisted.Id} gravado e relido no banco oficial.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void DesativarContato_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureSession() || _isBusy || SelectedFornecedor is null || SelectedContato is null)
        {
            Status = "Selecione um contato persistido para desativar.";
            return;
        }

        var confirmation = MessageBox.Show(
            $"Desativar o contato '{SelectedContato.Nome}' do fornecedor '{SelectedFornecedor.Nome}'?",
            "Desativar contato",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            Status = "Desativação cancelada. Nenhum dado foi alterado.";
            return;
        }

        var contactId = SelectedContato.Id;
        var supplierId = SelectedFornecedor.Id;
        SetBusy(true);
        try
        {
            var result = await _api.DeactivateFornecedorContatoAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                supplierId,
                contactId,
                SelectedContato.RowVersion);
            if (!result.Success)
            {
                Status = $"Contato não desativado: {result.Detail}";
                return;
            }

            var contacts = await _api.GetFornecedorContatosAsync(_terminal, _terminal.DesktopAccessToken, supplierId);
            if (!contacts.Success || contacts.Data is null || contacts.Data.Any(item => item.Id == contactId))
            {
                Status = $"A API informou desativação do contato #{contactId}, mas a releitura não confirmou o soft-delete. Consulte antes de repetir.";
                return;
            }

            _contactAuditIds.Add(contactId);
            ReplaceCollection(Contatos, contacts.Data);
            SelectedContato = null;
            ClearContatoForm();
            await LoadAuditoriaAsync(supplierId);
            Status = $"Contato #{contactId} desativado e ausência confirmada na releitura oficial.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();

    private async Task RefreshFornecedoresAsync(int? selectId = null, bool allowBusy = false)
    {
        if (!EnsureSession() || (_isBusy && !allowBusy))
        {
            return;
        }

        var currentId = selectId ?? SelectedFornecedor?.Id;
        SetBusy(true);
        try
        {
            var result = await _api.GetFornecedoresAsync(_terminal, _terminal.DesktopAccessToken);
            if (!result.Success || result.Data is null)
            {
                Status = $"Falha ao carregar fornecedores: {result.Detail}";
                return;
            }

            ReplaceCollection(Fornecedores, result.Data);
            OnPropertyChanged(nameof(FornecedorCountLabel));
            SelectedFornecedor = currentId.HasValue
                ? Fornecedores.FirstOrDefault(item => item.Id == currentId.Value)
                : Fornecedores.FirstOrDefault();
            if (SelectedFornecedor is null)
            {
                ClearFornecedorForm();
                Contatos.Clear();
                Auditoria.Clear();
            }

            Status = $"{Fornecedores.Count} fornecedor(es) relido(s) da API oficial.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task LoadFornecedorDependenciasAsync(int fornecedorId)
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            return;
        }

        var contacts = await _api.GetFornecedorContatosAsync(_terminal, _terminal.DesktopAccessToken, fornecedorId);
        if (SelectedFornecedor?.Id != fornecedorId)
        {
            return;
        }

        if (!contacts.Success || contacts.Data is null)
        {
            Status = $"Fornecedor carregado, mas os contatos falharam: {contacts.Detail}";
            return;
        }

        ReplaceCollection(Contatos, contacts.Data);
        foreach (var contact in contacts.Data)
        {
            _contactAuditIds.Add(contact.Id);
        }

        SelectedContato = Contatos.FirstOrDefault();
        if (SelectedContato is null)
        {
            ClearContatoForm();
        }

        await LoadAuditoriaAsync(fornecedorId);
    }

    private async Task LoadAuditoriaAsync(int fornecedorId)
    {
        var supplierAudit = await _api.GetAuditoriaAsync(_terminal, _terminal.DesktopAccessToken, "fornecedores");
        var contactAudit = await _api.GetAuditoriaAsync(_terminal, _terminal.DesktopAccessToken, "md_fornecedor_contatos");
        if (SelectedFornecedor?.Id != fornecedorId)
        {
            return;
        }

        var entries = new List<DesktopAuditoriaOperacional>();
        if (supplierAudit.Success && supplierAudit.Data is not null)
        {
            entries.AddRange(supplierAudit.Data.Where(item => item.RegistroId == fornecedorId));
        }

        if (contactAudit.Success && contactAudit.Data is not null)
        {
            entries.AddRange(contactAudit.Data.Where(item => _contactAuditIds.Contains(item.RegistroId)));
        }

        ReplaceCollection(Auditoria, entries.OrderByDescending(item => item.CreatedAt));
    }

    private bool TryBuildFornecedorRequest(out DesktopFornecedorRequest? request, out string validationError)
    {
        request = null;
        if (string.IsNullOrWhiteSpace(RazaoSocial))
        {
            validationError = "Razão social do fornecedor é obrigatória.";
            return false;
        }

        if (!TryParseDecimal(ComissaoPercentualText, out var commission) || commission is < 0m or > 100m)
        {
            validationError = "Comissão deve ser um percentual entre 0 e 100.";
            return false;
        }

        if (!int.TryParse(PrazoEntregaDiasText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var deadline)
            || deadline is < 0 or > 3650)
        {
            validationError = "Prazo de entrega deve ser um número entre 0 e 3650 dias.";
            return false;
        }

        int? storeId = null;
        if (!string.IsNullOrWhiteSpace(LojaVinculadaIdText))
        {
            if (!int.TryParse(LojaVinculadaIdText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedStoreId)
                || parsedStoreId <= 0)
            {
                validationError = "ID da loja vinculada deve ser um número inteiro positivo.";
                return false;
            }

            storeId = parsedStoreId;
        }

        request = new DesktopFornecedorRequest(
            RazaoSocial.Trim(),
            NullIfWhiteSpace(Documento),
            NullIfWhiteSpace(Email),
            NullIfWhiteSpace(Telefone),
            NullIfWhiteSpace(Segmento),
            NullIfWhiteSpace(NomeFantasia),
            NullIfWhiteSpace(InscricaoEstadual),
            NullIfWhiteSpace(Whatsapp),
            NullIfWhiteSpace(Endereco),
            NullIfWhiteSpace(Cidade),
            NullIfWhiteSpace(Estado),
            NullIfWhiteSpace(Cep),
            storeId,
            commission,
            deadline,
            FornecedorStatus,
            NullIfWhiteSpace(Observacoes),
            SelectedFornecedor?.RowVersion);
        validationError = string.Empty;
        return true;
    }

    private void LoadFornecedorForm(DesktopFornecedor fornecedor)
    {
        RazaoSocial = fornecedor.RazaoSocial ?? fornecedor.Nome;
        NomeFantasia = fornecedor.NomeFantasia ?? fornecedor.Nome;
        Documento = fornecedor.Documento;
        InscricaoEstadual = fornecedor.Ie ?? string.Empty;
        Email = fornecedor.Email;
        Telefone = fornecedor.Telefone;
        Whatsapp = fornecedor.Whatsapp ?? string.Empty;
        Segmento = fornecedor.Categoria;
        Endereco = fornecedor.Endereco ?? string.Empty;
        Cidade = fornecedor.Cidade ?? string.Empty;
        Estado = fornecedor.Estado ?? string.Empty;
        Cep = fornecedor.Cep ?? string.Empty;
        LojaVinculadaIdText = fornecedor.LojaVinculadaId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        ComissaoPercentualText = fornecedor.ComissaoPercentual.ToString("0.00", CultureInfo.GetCultureInfo("pt-BR"));
        PrazoEntregaDiasText = fornecedor.PrazoEntregaDias.ToString(CultureInfo.InvariantCulture);
        FornecedorStatus = string.IsNullOrWhiteSpace(fornecedor.Status) ? "Ativo" : fornecedor.Status;
        Observacoes = fornecedor.Observacoes ?? string.Empty;
    }

    private void LoadContatoForm(DesktopFornecedorContato contato)
    {
        ContatoNome = contato.Nome;
        ContatoCargo = contato.Cargo ?? string.Empty;
        ContatoEmail = contato.Email ?? string.Empty;
        ContatoTelefone = contato.Telefone ?? string.Empty;
        ContatoCelular = contato.Celular ?? string.Empty;
        ContatoPrincipal = contato.Principal;
        ContatoAtivo = contato.Ativo;
    }

    private void ClearFornecedorForm()
    {
        RazaoSocial = string.Empty;
        NomeFantasia = string.Empty;
        Documento = string.Empty;
        InscricaoEstadual = string.Empty;
        Email = string.Empty;
        Telefone = string.Empty;
        Whatsapp = string.Empty;
        Segmento = string.Empty;
        Endereco = string.Empty;
        Cidade = string.Empty;
        Estado = string.Empty;
        Cep = string.Empty;
        LojaVinculadaIdText = string.Empty;
        ComissaoPercentualText = "0,00";
        PrazoEntregaDiasText = "7";
        FornecedorStatus = "Ativo";
        Observacoes = string.Empty;
        NotifyFornecedorState();
    }

    private void ClearContatoForm()
    {
        ContatoNome = string.Empty;
        ContatoCargo = string.Empty;
        ContatoEmail = string.Empty;
        ContatoTelefone = string.Empty;
        ContatoCelular = string.Empty;
        ContatoPrincipal = false;
        ContatoAtivo = true;
        OnPropertyChanged(nameof(ContactVersionLabel));
    }

    private bool EnsureSession()
    {
        if (!string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            return true;
        }

        Status = "Sessão administrativa ausente. Autentique no menu de configurações ou na gestão de acessos.";
        OnPropertyChanged(nameof(CanOperate));
        return false;
    }

    private void SetBusy(bool value)
    {
        _isBusy = value;
        OnPropertyChanged(nameof(CanOperate));
    }

    private void NotifyFornecedorState()
    {
        OnPropertyChanged(nameof(HasPersistedFornecedor));
        OnPropertyChanged(nameof(CadastroReference));
        OnPropertyChanged(nameof(RowVersionLabel));
        OnPropertyChanged(nameof(CadastroValidation));
        OnPropertyChanged(nameof(ContactHeader));
    }

    private void FitToWorkArea()
    {
        var workArea = SystemParameters.WorkArea;
        MaxWidth = workArea.Width;
        MaxHeight = workArea.Height;
        Width = Math.Min(1280, Math.Max(960, workArea.Width - 32));
        Height = Math.Min(760, Math.Max(640, workArea.Height - 32));
        MinWidth = Math.Min(1080, Width);
        MinHeight = Math.Min(680, Height);
    }

    private static bool TryParseDecimal(string value, out decimal parsed) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out parsed)
        || decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed);

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string ShortVersion(string? rowVersion) =>
        string.IsNullOrWhiteSpace(rowVersion)
            ? "ausente"
            : rowVersion.Length <= 10 ? rowVersion : rowVersion[..10];

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
