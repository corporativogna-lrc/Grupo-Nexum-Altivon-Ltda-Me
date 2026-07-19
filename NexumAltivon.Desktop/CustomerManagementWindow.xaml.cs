/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7187
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class CustomerManagementWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly HashSet<int> _enderecoAuditIds = [];
    private DesktopCliente? _selectedCliente;
    private DesktopClienteEndereco? _selectedEndereco;
    private bool _isBusy;
    private int _selectedTabIndex;
    private string _status = "Aguardando leitura dos clientes.";
    private string _clienteNome = string.Empty;
    private string _clienteEmail = string.Empty;
    private string _clienteDocumento = string.Empty;
    private string _clienteTipo = "PF";
    private string _clienteStatus = "Pendente";
    private string _clienteRgIe = string.Empty;
    private DateTime? _clienteDataNascimento;
    private string _clienteTelefone = string.Empty;
    private string _clienteWhatsapp = string.Empty;
    private string _clienteAvatar = string.Empty;
    private string _clientePontosText = "0";
    private bool _clienteNewsletter = true;
    private bool _clienteVip;
    private string _enderecoApelido = "Principal";
    private string _enderecoTipo = "Entrega";
    private string _enderecoCep = string.Empty;
    private string _enderecoLogradouro = string.Empty;
    private string _enderecoNumero = string.Empty;
    private string _enderecoComplemento = string.Empty;
    private string _enderecoBairro = string.Empty;
    private string _enderecoCidade = string.Empty;
    private string _enderecoEstado = string.Empty;
    private string _enderecoPais = "Brasil";
    private bool _enderecoPadrao;

    public CustomerManagementWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        TipoClienteOptions = new ObservableCollection<string> { "PF", "PJ" };
        StatusClienteOptions = new ObservableCollection<string> { "Ativo", "Pendente", "Bloqueado", "Inativo" };
        TipoEnderecoOptions = new ObservableCollection<string> { "Entrega", "Cobranca", "Ambos" };
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) =>
        {
            FitToWorkArea();
            await RefreshClientesAsync();
        };
    }

    public ObservableCollection<DesktopCliente> Clientes { get; } = [];
    public ObservableCollection<DesktopClienteEndereco> Enderecos { get; } = [];
    public ObservableCollection<DesktopAuditoriaOperacional> Auditoria { get; } = [];
    public ObservableCollection<string> TipoClienteOptions { get; }
    public ObservableCollection<string> StatusClienteOptions { get; }
    public ObservableCollection<string> TipoEnderecoOptions { get; }

    public DesktopCliente? SelectedCliente
    {
        get => _selectedCliente;
        set
        {
            if (!SetField(ref _selectedCliente, value))
            {
                return;
            }

            _enderecoAuditIds.Clear();
            if (value is not null)
            {
                LoadClienteForm(value);
                _ = LoadClienteDependenciasAsync(value.Id);
            }

            NotifyClienteState();
        }
    }

    public DesktopClienteEndereco? SelectedEndereco
    {
        get => _selectedEndereco;
        set
        {
            if (!SetField(ref _selectedEndereco, value))
            {
                return;
            }

            if (value is not null)
            {
                LoadEnderecoForm(value);
            }

            OnPropertyChanged(nameof(EnderecoVersionLabel));
        }
    }

    public int SelectedTabIndex { get => _selectedTabIndex; set => SetField(ref _selectedTabIndex, value); }
    public string Status { get => _status; private set => SetField(ref _status, value); }
    public string ClienteNome { get => _clienteNome; set { if (SetField(ref _clienteNome, value)) OnPropertyChanged(nameof(CadastroValidation)); } }
    public string ClienteEmail { get => _clienteEmail; set { if (SetField(ref _clienteEmail, value)) OnPropertyChanged(nameof(CadastroValidation)); } }
    public string ClienteDocumento { get => _clienteDocumento; set { if (SetField(ref _clienteDocumento, value)) OnPropertyChanged(nameof(CadastroValidation)); } }
    public string ClienteTipo { get => _clienteTipo; set => SetField(ref _clienteTipo, value); }
    public string ClienteStatus { get => _clienteStatus; set => SetField(ref _clienteStatus, value); }
    public string ClienteRgIe { get => _clienteRgIe; set => SetField(ref _clienteRgIe, value); }
    public DateTime? ClienteDataNascimento { get => _clienteDataNascimento; set => SetField(ref _clienteDataNascimento, value); }
    public string ClienteTelefone { get => _clienteTelefone; set => SetField(ref _clienteTelefone, value); }
    public string ClienteWhatsapp { get => _clienteWhatsapp; set => SetField(ref _clienteWhatsapp, value); }
    public string ClienteAvatar { get => _clienteAvatar; set => SetField(ref _clienteAvatar, value); }
    public string ClientePontosText { get => _clientePontosText; set => SetField(ref _clientePontosText, value); }
    public bool ClienteNewsletter { get => _clienteNewsletter; set => SetField(ref _clienteNewsletter, value); }
    public bool ClienteVip { get => _clienteVip; set => SetField(ref _clienteVip, value); }
    public string EnderecoApelido { get => _enderecoApelido; set => SetField(ref _enderecoApelido, value); }
    public string EnderecoTipo { get => _enderecoTipo; set => SetField(ref _enderecoTipo, value); }
    public string EnderecoCep { get => _enderecoCep; set => SetField(ref _enderecoCep, value); }
    public string EnderecoLogradouro { get => _enderecoLogradouro; set => SetField(ref _enderecoLogradouro, value); }
    public string EnderecoNumero { get => _enderecoNumero; set => SetField(ref _enderecoNumero, value); }
    public string EnderecoComplemento { get => _enderecoComplemento; set => SetField(ref _enderecoComplemento, value); }
    public string EnderecoBairro { get => _enderecoBairro; set => SetField(ref _enderecoBairro, value); }
    public string EnderecoCidade { get => _enderecoCidade; set => SetField(ref _enderecoCidade, value); }
    public string EnderecoEstado { get => _enderecoEstado; set => SetField(ref _enderecoEstado, value); }
    public string EnderecoPais { get => _enderecoPais; set => SetField(ref _enderecoPais, value); }
    public bool EnderecoPadrao { get => _enderecoPadrao; set => SetField(ref _enderecoPadrao, value); }

    public bool CanOperate => !_isBusy && !string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken);
    public bool HasPersistedCliente => SelectedCliente is { Id: > 0 };
    public string ClienteCountLabel => $"{Clientes.Count} registro(s)";
    public string CadastroReference => SelectedCliente is null ? "Novo cliente sem identificador persistido" : $"Cliente #{SelectedCliente.Id}";
    public string RowVersionLabel => SelectedCliente is null ? "Nova versão será gerada ao salvar" : $"Versão {ShortVersion(SelectedCliente.RowVersion)}";
    public string CadastroValidation => GetCadastroValidation();
    public string EnderecoHeader => SelectedCliente is null ? "Selecione um cliente persistido" : $"Endereços de {SelectedCliente.Nome}";
    public string EnderecoVersionLabel => SelectedEndereco is null
        ? "Nova versão será gerada"
        : $"Endereço #{SelectedEndereco.Id} / {ShortVersion(SelectedEndereco.RowVersion)}";
    public string TenantLabel => $"Loja {(_terminal.StoreCode ?? string.Empty).Trim()} / terminal {(_terminal.TerminalCode ?? string.Empty).Trim()}";

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Atualizar_Click(object sender, RoutedEventArgs e) => await RefreshClientesAsync();

    private void NovoCliente_Click(object sender, RoutedEventArgs e)
    {
        SelectedCliente = null;
        ClearClienteForm();
        Enderecos.Clear();
        Auditoria.Clear();
        _enderecoAuditIds.Clear();
        SelectedEndereco = null;
        SelectedTabIndex = 0;
        Status = "Novo cliente pronto para preenchimento. Nenhuma gravação foi realizada.";
    }

    private async void SalvarCliente_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureSession() || _isBusy)
        {
            return;
        }

        if (!TryBuildClienteRequest(out var request, out var validationError))
        {
            Status = validationError;
            return;
        }

        SetBusy(true);
        try
        {
            var previousVersion = SelectedCliente?.RowVersion;
            var result = SelectedCliente is null
                ? await _api.CreateClienteAsync(_terminal, _terminal.DesktopAccessToken, request!)
                : await _api.UpdateClienteAsync(_terminal, _terminal.DesktopAccessToken, SelectedCliente.Id, request!);
            if (!result.Success || result.Data is null)
            {
                Status = $"Cliente não gravado: {result.Detail}";
                return;
            }

            var persisted = await _api.GetClienteAsync(_terminal, _terminal.DesktopAccessToken, result.Data.Id);
            if (!persisted.Success || persisted.Data is null)
            {
                Status = $"A API informou gravação do cliente #{result.Data.Id}, mas a releitura falhou: {persisted.Detail}. Consulte antes de repetir.";
                return;
            }

            if (string.IsNullOrWhiteSpace(persisted.Data.RowVersion)
                || !string.Equals(result.Data.RowVersion, persisted.Data.RowVersion, StringComparison.Ordinal)
                || (!string.IsNullOrWhiteSpace(previousVersion)
                    && string.Equals(previousVersion, persisted.Data.RowVersion, StringComparison.Ordinal)))
            {
                Status = "A releitura não confirmou uma nova versão do cliente. A operação não será apresentada como concluída.";
                return;
            }

            var persistedId = persisted.Data.Id;
            await RefreshClientesAsync(persistedId, allowBusy: true);
            Status = $"Cliente #{persistedId} gravado e relido no banco oficial.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void NovoEndereco_Click(object sender, RoutedEventArgs e)
    {
        SelectedEndereco = null;
        ClearEnderecoForm();
        Status = "Novo endereço pronto para preenchimento. Nenhuma gravação foi realizada.";
    }

    private async void SalvarEndereco_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureSession() || _isBusy || SelectedCliente is null)
        {
            return;
        }

        if (!TryBuildEnderecoRequest(out var request, out var validationError))
        {
            Status = validationError;
            return;
        }

        var clienteId = SelectedCliente.Id;
        SetBusy(true);
        try
        {
            var previousVersion = SelectedEndereco?.RowVersion;
            var result = SelectedEndereco is null
                ? await _api.CreateClienteEnderecoAsync(_terminal, _terminal.DesktopAccessToken, clienteId, request!)
                : await _api.UpdateClienteEnderecoAsync(_terminal, _terminal.DesktopAccessToken, clienteId, SelectedEndereco.Id, request!);
            if (!result.Success || result.Data is null)
            {
                Status = $"Endereço não gravado: {result.Detail}";
                return;
            }

            var addresses = await _api.GetClienteEnderecosAsync(_terminal, _terminal.DesktopAccessToken, clienteId);
            var persisted = addresses.Data?.FirstOrDefault(item => item.Id == result.Data.Id);
            if (!addresses.Success || persisted is null
                || string.IsNullOrWhiteSpace(persisted.RowVersion)
                || !string.Equals(persisted.RowVersion, result.Data.RowVersion, StringComparison.Ordinal)
                || (!string.IsNullOrWhiteSpace(previousVersion)
                    && string.Equals(previousVersion, persisted.RowVersion, StringComparison.Ordinal)))
            {
                Status = $"A API informou gravação do endereço #{result.Data.Id}, mas a releitura não confirmou a nova versão. Consulte antes de repetir.";
                return;
            }

            _enderecoAuditIds.Add(persisted.Id);
            ReplaceCollection(Enderecos, addresses.Data!);
            SelectedEndereco = Enderecos.First(item => item.Id == persisted.Id);
            await LoadAuditoriaAsync(clienteId);
            Status = $"Endereço #{persisted.Id} gravado e relido no banco oficial.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void ArquivarEndereco_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureSession() || _isBusy || SelectedCliente is null || SelectedEndereco is null)
        {
            Status = "Selecione um endereço persistido para arquivar.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedEndereco.RowVersion))
        {
            Status = "O endereço selecionado não possui versão válida. Atualize a lista antes de arquivar.";
            return;
        }

        var confirmation = MessageBox.Show(
            $"Arquivar o endereço '{SelectedEndereco.Apelido}' do cliente '{SelectedCliente.Nome}'?",
            "Arquivar endereço",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            Status = "Arquivamento cancelado. Nenhum dado foi alterado.";
            return;
        }

        var clienteId = SelectedCliente.Id;
        var enderecoId = SelectedEndereco.Id;
        var rowVersion = SelectedEndereco.RowVersion;
        SetBusy(true);
        try
        {
            var result = await _api.ArchiveClienteEnderecoAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                clienteId,
                enderecoId,
                rowVersion);
            if (!result.Success)
            {
                Status = $"Endereço não arquivado: {result.Detail}";
                return;
            }

            var addresses = await _api.GetClienteEnderecosAsync(_terminal, _terminal.DesktopAccessToken, clienteId);
            if (!addresses.Success || addresses.Data is null || addresses.Data.Any(item => item.Id == enderecoId))
            {
                Status = $"A API informou arquivamento do endereço #{enderecoId}, mas a releitura não confirmou o soft-delete. Consulte antes de repetir.";
                return;
            }

            _enderecoAuditIds.Add(enderecoId);
            ReplaceCollection(Enderecos, addresses.Data);
            SelectedEndereco = Enderecos.FirstOrDefault();
            if (SelectedEndereco is null)
            {
                ClearEnderecoForm();
            }

            await LoadAuditoriaAsync(clienteId);
            Status = $"Endereço #{enderecoId} arquivado e ausência confirmada na releitura oficial.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();

    private async Task RefreshClientesAsync(int? selectId = null, bool allowBusy = false)
    {
        if (!EnsureSession() || (_isBusy && !allowBusy))
        {
            return;
        }

        var currentId = selectId ?? SelectedCliente?.Id;
        SetBusy(true);
        try
        {
            var result = await _api.GetClientesAsync(_terminal, _terminal.DesktopAccessToken);
            if (!result.Success || result.Data is null)
            {
                Status = $"Falha ao carregar clientes: {result.Detail}";
                return;
            }

            ReplaceCollection(Clientes, result.Data);
            OnPropertyChanged(nameof(ClienteCountLabel));
            SelectedCliente = currentId.HasValue
                ? Clientes.FirstOrDefault(item => item.Id == currentId.Value)
                : Clientes.FirstOrDefault();
            if (SelectedCliente is null)
            {
                ClearClienteForm();
                Enderecos.Clear();
                Auditoria.Clear();
                SelectedEndereco = null;
            }

            Status = $"{Clientes.Count} cliente(s) relido(s) da API oficial.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task LoadClienteDependenciasAsync(int clienteId)
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            return;
        }

        var addresses = await _api.GetClienteEnderecosAsync(_terminal, _terminal.DesktopAccessToken, clienteId);
        if (SelectedCliente?.Id != clienteId)
        {
            return;
        }

        if (!addresses.Success || addresses.Data is null)
        {
            Status = $"Cliente carregado, mas os endereços falharam: {addresses.Detail}";
            return;
        }

        ReplaceCollection(Enderecos, addresses.Data);
        _enderecoAuditIds.Clear();
        foreach (var address in addresses.Data)
        {
            _enderecoAuditIds.Add(address.Id);
        }

        SelectedEndereco = Enderecos.FirstOrDefault();
        if (SelectedEndereco is null)
        {
            ClearEnderecoForm();
        }

        await LoadAuditoriaAsync(clienteId);
    }

    private async Task LoadAuditoriaAsync(int clienteId)
    {
        var customerAudit = await _api.GetAuditoriaAsync(_terminal, _terminal.DesktopAccessToken, "clientes");
        var addressAudit = await _api.GetAuditoriaAsync(_terminal, _terminal.DesktopAccessToken, "enderecos");
        if (SelectedCliente?.Id != clienteId)
        {
            return;
        }

        var entries = new List<DesktopAuditoriaOperacional>();
        if (customerAudit.Success && customerAudit.Data is not null)
        {
            entries.AddRange(customerAudit.Data.Where(item => item.RegistroId == clienteId));
        }

        if (addressAudit.Success && addressAudit.Data is not null)
        {
            entries.AddRange(addressAudit.Data.Where(item => _enderecoAuditIds.Contains(item.RegistroId)));
        }

        ReplaceCollection(Auditoria, entries.OrderByDescending(item => item.CreatedAt));
    }

    private bool TryBuildClienteRequest(out DesktopClienteRequest? request, out string validationError)
    {
        request = null;
        var name = ClienteNome.Trim();
        if (name.Length is < 3 or > 150)
        {
            validationError = "Nome ou razão social deve ter entre 3 e 150 caracteres.";
            return false;
        }

        var email = ClienteEmail.Trim();
        if (email.Length > 150 || !MailAddress.TryCreate(email, out _))
        {
            validationError = "Informe um e-mail válido para o cliente.";
            return false;
        }

        var document = DigitsOnly(ClienteDocumento);
        var expectedLength = ClienteTipo == "PJ" ? 14 : 11;
        if (document.Length != expectedLength)
        {
            validationError = ClienteTipo == "PJ" ? "Informe um CNPJ com 14 dígitos." : "Informe um CPF com 11 dígitos.";
            return false;
        }

        if (!int.TryParse(ClientePontosText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var points)
            || points is < 0 or > 100000000)
        {
            validationError = "Pontos de fidelidade devem ser um número entre 0 e 100.000.000.";
            return false;
        }

        if (SelectedCliente is not null && string.IsNullOrWhiteSpace(SelectedCliente.RowVersion))
        {
            validationError = "O cliente selecionado não possui versão válida. Atualize a lista antes de salvar.";
            return false;
        }

        request = new DesktopClienteRequest(
            name,
            email,
            document,
            NullIfWhiteSpace(ClienteTelefone),
            null,
            ClienteNewsletter,
            null,
            NullIfWhiteSpace(ClienteRgIe),
            ClienteDataNascimento?.Date,
            NullIfWhiteSpace(ClienteWhatsapp),
            NullIfWhiteSpace(ClienteAvatar),
            ClienteVip,
            points,
            ClienteStatus,
            ClienteTipo,
            SelectedCliente?.RowVersion);
        validationError = string.Empty;
        return true;
    }

    private bool TryBuildEnderecoRequest(out DesktopClienteEnderecoRequest? request, out string validationError)
    {
        request = null;
        var cep = DigitsOnly(EnderecoCep);
        if (cep.Length != 8)
        {
            validationError = "CEP deve conter 8 dígitos.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(EnderecoLogradouro)
            || string.IsNullOrWhiteSpace(EnderecoNumero)
            || string.IsNullOrWhiteSpace(EnderecoBairro)
            || string.IsNullOrWhiteSpace(EnderecoCidade))
        {
            validationError = "Logradouro, número, bairro e cidade são obrigatórios.";
            return false;
        }

        var state = EnderecoEstado.Trim().ToUpperInvariant();
        if (state.Length != 2 || !state.All(char.IsLetter))
        {
            validationError = "UF deve conter 2 letras.";
            return false;
        }

        if (SelectedEndereco is not null && string.IsNullOrWhiteSpace(SelectedEndereco.RowVersion))
        {
            validationError = "O endereço selecionado não possui versão válida. Atualize a lista antes de salvar.";
            return false;
        }

        request = new DesktopClienteEnderecoRequest(
            NullIfWhiteSpace(EnderecoApelido) ?? "Principal",
            EnderecoTipo,
            cep,
            EnderecoLogradouro.Trim(),
            EnderecoNumero.Trim(),
            NullIfWhiteSpace(EnderecoComplemento),
            EnderecoBairro.Trim(),
            EnderecoCidade.Trim(),
            state,
            NullIfWhiteSpace(EnderecoPais) ?? "Brasil",
            EnderecoPadrao,
            SelectedEndereco?.RowVersion);
        validationError = string.Empty;
        return true;
    }

    private void LoadClienteForm(DesktopCliente cliente)
    {
        ClienteNome = cliente.Nome;
        ClienteEmail = cliente.Email;
        ClienteDocumento = cliente.Cpf ?? string.Empty;
        ClienteTipo = string.IsNullOrWhiteSpace(cliente.Tipo) ? "PF" : cliente.Tipo;
        ClienteStatus = string.IsNullOrWhiteSpace(cliente.Status) ? "Pendente" : cliente.Status;
        ClienteRgIe = cliente.RgIe ?? string.Empty;
        ClienteDataNascimento = cliente.DataNascimento;
        ClienteTelefone = cliente.Telefone ?? string.Empty;
        ClienteWhatsapp = cliente.Whatsapp ?? string.Empty;
        ClienteAvatar = cliente.Avatar ?? string.Empty;
        ClientePontosText = cliente.PontosFidelidade.ToString(CultureInfo.InvariantCulture);
        ClienteNewsletter = cliente.Newsletter;
        ClienteVip = cliente.Vip;
    }

    private void LoadEnderecoForm(DesktopClienteEndereco endereco)
    {
        EnderecoApelido = endereco.Apelido;
        EnderecoTipo = endereco.Tipo;
        EnderecoCep = endereco.Cep;
        EnderecoLogradouro = endereco.Logradouro;
        EnderecoNumero = endereco.Numero;
        EnderecoComplemento = endereco.Complemento ?? string.Empty;
        EnderecoBairro = endereco.Bairro ?? string.Empty;
        EnderecoCidade = endereco.Cidade ?? string.Empty;
        EnderecoEstado = endereco.Estado ?? string.Empty;
        EnderecoPais = string.IsNullOrWhiteSpace(endereco.Pais) ? "Brasil" : endereco.Pais;
        EnderecoPadrao = endereco.Padrao;
    }

    private void ClearClienteForm()
    {
        ClienteNome = string.Empty;
        ClienteEmail = string.Empty;
        ClienteDocumento = string.Empty;
        ClienteTipo = "PF";
        ClienteStatus = "Pendente";
        ClienteRgIe = string.Empty;
        ClienteDataNascimento = null;
        ClienteTelefone = string.Empty;
        ClienteWhatsapp = string.Empty;
        ClienteAvatar = string.Empty;
        ClientePontosText = "0";
        ClienteNewsletter = true;
        ClienteVip = false;
        NotifyClienteState();
    }

    private void ClearEnderecoForm()
    {
        EnderecoApelido = "Principal";
        EnderecoTipo = "Entrega";
        EnderecoCep = string.Empty;
        EnderecoLogradouro = string.Empty;
        EnderecoNumero = string.Empty;
        EnderecoComplemento = string.Empty;
        EnderecoBairro = string.Empty;
        EnderecoCidade = string.Empty;
        EnderecoEstado = string.Empty;
        EnderecoPais = "Brasil";
        EnderecoPadrao = Enderecos.Count == 0;
        OnPropertyChanged(nameof(EnderecoVersionLabel));
    }

    private string GetCadastroValidation()
    {
        if (ClienteNome.Trim().Length < 3)
        {
            return "Nome ou razão social é obrigatório.";
        }

        if (!MailAddress.TryCreate(ClienteEmail.Trim(), out _))
        {
            return "E-mail válido é obrigatório.";
        }

        var expectedLength = ClienteTipo == "PJ" ? 14 : 11;
        return DigitsOnly(ClienteDocumento).Length == expectedLength
            ? "Cadastro pronto para validação definitiva pela API oficial."
            : ClienteTipo == "PJ" ? "CNPJ válido é obrigatório." : "CPF válido é obrigatório.";
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

    private void NotifyClienteState()
    {
        OnPropertyChanged(nameof(HasPersistedCliente));
        OnPropertyChanged(nameof(CadastroReference));
        OnPropertyChanged(nameof(RowVersionLabel));
        OnPropertyChanged(nameof(CadastroValidation));
        OnPropertyChanged(nameof(EnderecoHeader));
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

    private static string DigitsOnly(string? value) => new((value ?? string.Empty).Where(char.IsDigit).ToArray());

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
