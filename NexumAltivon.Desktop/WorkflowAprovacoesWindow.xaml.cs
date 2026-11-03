/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7221 Data: 16/08/2026
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class WorkflowAprovacoesWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopWorkflowDefinition? _selectedDefinition;
    private DesktopWorkflowInstance? _selectedInstance;
    private string _status = "Aguardando carregamento da API oficial.";
    private string _definitionDetails = "Nenhuma definição selecionada.";
    private string _instanceDetails = "Nenhuma instância selecionada.";
    private bool _updatingForm;
    private bool _isBusy;

    public WorkflowAprovacoesWindow(TerminalProfile terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        InitializeComponent();
        _terminal = terminal;
        DataContext = this;
        Loaded += async (_, _) =>
        {
            await CarregarDefinicoesAsync();
            await CarregarInstanciasAsync();
        };
        Closed += (_, _) =>
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        };
    }

    public ObservableCollection<DesktopWorkflowDefinition> Definicoes { get; } = [];

    public ObservableCollection<DesktopWorkflowInstance> Instancias { get; } = [];

    public ObservableCollection<DesktopWorkflowTransition> Historico { get; } = [];

    public DesktopWorkflowDefinition? SelectedDefinition
    {
        get => _selectedDefinition;
        private set
        {
            if (!SetField(ref _selectedDefinition, value))
            {
                return;
            }

            _updatingForm = true;
            if (value is null)
            {
                EntidadeBox.Text = string.Empty;
                CodigoBox.Text = string.Empty;
                NomeBox.Text = string.Empty;
                EstadosBox.Text = string.Empty;
                TransicoesBox.Text = string.Empty;
                AtivoBox.IsChecked = true;
                DefinicaoDetails = "Nova definição. Nenhum dado foi gravado.";
            }
            else
            {
                EntidadeBox.Text = value.Entidade;
                CodigoBox.Text = value.Codigo;
                NomeBox.Text = value.Nome;
                EstadosBox.Text = FormatStates(value.EstadosJson);
                TransicoesBox.Text = FormatTransitions(value.TransicoesJson);
                AtivoBox.IsChecked = value.Ativo;
                DefinicaoDetails = $"Id: {value.Id}\nCriada: {value.CriadoEm:dd/MM/yyyy HH:mm:ss} UTC\nAtualizada: {value.AtualizadoEm:dd/MM/yyyy HH:mm:ss} UTC";
                DefinicaoInstanciaBox.SelectedItem = value;
            }

            _updatingForm = false;
        }
    }

    public DesktopWorkflowInstance? SelectedInstance
    {
        get => _selectedInstance;
        private set
        {
            if (!SetField(ref _selectedInstance, value))
            {
                return;
            }

            _updatingForm = true;
            if (value is null)
            {
                InstanciaEntidadeBox.Text = string.Empty;
                InstanciaRegistroBox.Text = string.Empty;
                EstadoInicialBox.Text = string.Empty;
                InstanciaObservacaoBox.Text = string.Empty;
                DestinoBox.Text = string.Empty;
                AcaoBox.Text = string.Empty;
                TransicaoObservacaoBox.Text = string.Empty;
                Historico.Clear();
                InstanciaDetails = "Nova instância. Nenhum dado foi gravado.";
            }
            else
            {
                InstanciaEntidadeBox.Text = value.Entidade;
                InstanciaRegistroBox.Text = value.RegistroChave;
                EstadoInicialBox.Text = value.EstadoAtual;
                InstanciaObservacaoBox.Text = string.Empty;
                InstanciaDetails = $"Id: {value.Id}\nDefinição: {value.DefinicaoId}\nEstado atual: {value.EstadoAtual}\nCriada: {value.CriadoEm:dd/MM/yyyy HH:mm:ss} UTC\nAtualizada: {value.AtualizadoEm:dd/MM/yyyy HH:mm:ss} UTC";
                var definition = Definicoes.FirstOrDefault(item => string.Equals(item.Id, value.DefinicaoId, StringComparison.OrdinalIgnoreCase));
                if (definition is not null)
                {
                    DefinicaoInstanciaBox.SelectedItem = definition;
                }
            }

            _updatingForm = false;
        }
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string DefinicaoDetails
    {
        get => _definitionDetails;
        private set => SetField(ref _definitionDetails, value);
    }

    public string InstanciaDetails
    {
        get => _instanceDetails;
        private set => SetField(ref _instanceDetails, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task<bool> CarregarDefinicoesAsync()
    {
        if (!EnsureSession())
        {
            Definicoes.Clear();
            return false;
        }

        var selectedId = _selectedDefinition?.Id;
        Status = "Consultando definições de workflow na API oficial...";
        var result = await _api.GetWorkflowDefinitionsAsync(_terminal, _terminal.DesktopAccessToken, cancellationToken: _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Definições não carregadas: {result.Detail}";
            return false;
        }

        Definicoes.Clear();
        foreach (var definition in result.Data)
        {
            Definicoes.Add(definition);
        }

        var selected = selectedId is null
            ? null
            : Definicoes.FirstOrDefault(item => string.Equals(item.Id, selectedId, StringComparison.OrdinalIgnoreCase));
        DefinicoesGrid.SelectedItem = selected;
        SelectedDefinition = selected;
        DefinicaoInstanciaBox.ItemsSource = Definicoes;
        Status = $"{Definicoes.Count} definição(ões) carregada(s) do tenant oficial.";
        return true;
    }

    private async Task<bool> CarregarInstanciasAsync()
    {
        if (!EnsureSession())
        {
            Instancias.Clear();
            return false;
        }

        var selectedId = _selectedInstance?.Id;
        Status = "Consultando instâncias de workflow na API oficial...";
        var result = await _api.GetWorkflowInstancesAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            NullIfWhiteSpace(FiltroEntidadeBox.Text),
            NullIfWhiteSpace(FiltroEstadoBox.Text),
            NullIfWhiteSpace(FiltroRegistroBox.Text),
            _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Instâncias não carregadas: {result.Detail}";
            return false;
        }

        Instancias.Clear();
        foreach (var instance in result.Data)
        {
            Instancias.Add(instance);
        }

        var selected = selectedId is null
            ? null
            : Instancias.FirstOrDefault(item => string.Equals(item.Id, selectedId, StringComparison.OrdinalIgnoreCase));
        InstanciasGrid.SelectedItem = selected;
        SelectedInstance = selected;
        Status = $"{Instancias.Count} instância(s) carregada(s) do tenant oficial.";
        return true;
    }

    private async void Recarregar_Click(object sender, RoutedEventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        await CarregarDefinicoesAsync();
        await CarregarInstanciasAsync();
    }

    private async void CarregarInstancias_Click(object sender, RoutedEventArgs e)
    {
        if (!_isBusy)
        {
            await CarregarInstanciasAsync();
        }
    }

    private void DefinicoesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_updatingForm)
        {
            SelectedDefinition = DefinicoesGrid.SelectedItem as DesktopWorkflowDefinition;
        }
    }

    private void InstanciasGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_updatingForm)
        {
            SelectedInstance = InstanciasGrid.SelectedItem as DesktopWorkflowInstance;
        }
    }

    private void NovaDefinicao_Click(object sender, RoutedEventArgs e)
    {
        DefinicoesGrid.SelectedItem = null;
        SelectedDefinition = null;
        Status = "Nova definição pronta para preenchimento. Nenhum dado foi gravado.";
    }

    private async void SalvarDefinicao_Click(object sender, RoutedEventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        var validDefinition = TryBuildDefinitionRequest(out var request, out var validationError);
        if (!validDefinition)
        {
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                Status = validationError;
                MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return;
        }

        Guid? id = null;
        if (SelectedDefinition is not null)
        {
            if (!Guid.TryParse(SelectedDefinition.Id, out var selectedId))
            {
                Status = "A definição selecionada retornou um GUID inválido; nenhuma alteração foi enviada.";
                MessageBox.Show(Status, "Falha de integridade", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            id = selectedId;
        }

        if (!EnsureSession())
        {
            return;
        }

        try
        {
            _isBusy = true;
            Status = id.HasValue ? "Atualizando definição na API oficial..." : "Criando definição na API oficial...";
            var result = await _api.SaveWorkflowDefinitionAsync(_terminal, _terminal.DesktopAccessToken, id, request, _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                Status = $"Operação não confirmada: {result.Detail}";
                MessageBox.Show(Status, "Falha ao salvar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var persistedId = result.Data.Id;
            if (!await CarregarDefinicoesAsync())
            {
                Status = "A API respondeu, mas a releitura oficial falhou. O resultado não foi apresentado como concluído.";
                return;
            }

            var persisted = Definicoes.FirstOrDefault(item => string.Equals(item.Id, persistedId, StringComparison.OrdinalIgnoreCase));
            if (persisted is null)
            {
                Status = "A API respondeu, mas a definição não apareceu na releitura oficial.";
                MessageBox.Show(Status, "Persistência não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DefinicoesGrid.SelectedItem = persisted;
            SelectedDefinition = persisted;
            Status = $"Definição {persisted.Codigo} persistida e relida no banco oficial.";
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void DesativarDefinicao_Click(object sender, RoutedEventArgs e)
    {
        if (_isBusy || SelectedDefinition is null || !Guid.TryParse(SelectedDefinition.Id, out var id))
        {
            Status = "Selecione uma definição válida antes de desativar.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirmation = MessageBox.Show(
            $"Desativar a definição '{SelectedDefinition.Nome}'? Definições com instâncias ativas serão recusadas pela API.",
            "Confirmar desativação",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes || !EnsureSession())
        {
            return;
        }

        try
        {
            _isBusy = true;
            Status = "Desativando definição na API oficial...";
            var result = await _api.DeleteWorkflowDefinitionAsync(_terminal, _terminal.DesktopAccessToken, id, _lifetime.Token);
            if (!result.Success)
            {
                Status = $"Desativação não confirmada: {result.Detail}";
                MessageBox.Show(Status, "Falha ao desativar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!await CarregarDefinicoesAsync())
            {
                Status = "A API respondeu, mas a releitura oficial falhou. A desativação não foi apresentada como concluída.";
                return;
            }

            if (Definicoes.Any(item => string.Equals(item.Id, id.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                Status = "A API respondeu, mas a definição continua na releitura oficial.";
                MessageBox.Show(Status, "Desativação não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NovaDefinicao_Click(sender, e);
            Status = "Definição desativada logicamente e removida da releitura oficial.";
        }
        finally
        {
            _isBusy = false;
        }
    }

    private void NovaInstancia_Click(object sender, RoutedEventArgs e)
    {
        InstanciasGrid.SelectedItem = null;
        SelectedInstance = null;
        Historico.Clear();
        Status = "Nova instância pronta para preenchimento. Nenhum dado foi gravado.";
    }

    private async void AbrirInstancia_Click(object sender, RoutedEventArgs e)
    {
        if (_isBusy || DefinicaoInstanciaBox.SelectedItem is not DesktopWorkflowDefinition definition)
        {
            Status = "Selecione uma definição ativa antes de abrir a instância.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Guid.TryParse(definition.Id, out var definitionId))
        {
            Status = "A definição selecionada retornou um GUID inválido.";
            MessageBox.Show(Status, "Falha de integridade", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var entidade = NullIfWhiteSpace(InstanciaEntidadeBox.Text);
        var registroChave = NullIfWhiteSpace(InstanciaRegistroBox.Text);
        if (entidade is null || registroChave is null)
        {
            Status = "Entidade e chave do registro são obrigatórias.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!EnsureSession())
        {
            return;
        }

        try
        {
            _isBusy = true;
            var request = new DesktopWorkflowInstanceRequest(
                definitionId,
                entidade,
                registroChave,
                NullIfWhiteSpace(EstadoInicialBox.Text),
                NullIfWhiteSpace(InstanciaObservacaoBox.Text));
            Status = "Abrindo instância na API oficial...";
            var result = await _api.CreateWorkflowInstanceAsync(_terminal, _terminal.DesktopAccessToken, request, _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                Status = $"Instância não aberta: {result.Detail}";
                MessageBox.Show(Status, "Falha ao abrir", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var persistedId = result.Data.Id;
            if (!await CarregarInstanciasAsync())
            {
                Status = "A API respondeu, mas a releitura das instâncias falhou.";
                return;
            }

            var persisted = Instancias.FirstOrDefault(item => string.Equals(item.Id, persistedId, StringComparison.OrdinalIgnoreCase));
            if (persisted is null)
            {
                Status = "A API respondeu, mas a instância não apareceu na releitura oficial.";
                MessageBox.Show(Status, "Persistência não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            InstanciasGrid.SelectedItem = persisted;
            SelectedInstance = persisted;
            Status = $"Instância {persisted.RegistroChave} aberta, persistida e relida no banco oficial.";
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void EncerrarInstancia_Click(object sender, RoutedEventArgs e)
    {
        if (_isBusy || SelectedInstance is null || !Guid.TryParse(SelectedInstance.Id, out var id))
        {
            Status = "Selecione uma instância válida antes de encerrar.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show("Encerrar logicamente a instância selecionada?", "Confirmar encerramento", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes || !EnsureSession())
        {
            return;
        }

        try
        {
            _isBusy = true;
            Status = "Encerrando instância na API oficial...";
            var result = await _api.DeleteWorkflowInstanceAsync(_terminal, _terminal.DesktopAccessToken, id, _lifetime.Token);
            if (!result.Success)
            {
                Status = $"Encerramento não confirmado: {result.Detail}";
                MessageBox.Show(Status, "Falha ao encerrar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!await CarregarInstanciasAsync())
            {
                Status = "A API respondeu, mas a releitura das instâncias falhou.";
                return;
            }

            if (Instancias.Any(item => string.Equals(item.Id, id.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                Status = "A API respondeu, mas a instância continua na releitura oficial.";
                MessageBox.Show(Status, "Encerramento não confirmado", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NovaInstancia_Click(sender, e);
            Status = "Instância encerrada logicamente e removida da releitura oficial.";
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void AplicarTransicao_Click(object sender, RoutedEventArgs e)
    {
        if (_isBusy || SelectedInstance is null || !Guid.TryParse(SelectedInstance.Id, out var id))
        {
            Status = "Selecione uma instância válida antes de aplicar uma transição.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var destino = NullIfWhiteSpace(DestinoBox.Text);
        if (destino is null)
        {
            Status = "O estado de destino é obrigatório.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!EnsureSession())
        {
            return;
        }

        try
        {
            _isBusy = true;
            Status = "Aplicando transição autorizada na API oficial...";
            var request = new DesktopWorkflowTransitionRequest(destino, NullIfWhiteSpace(AcaoBox.Text), NullIfWhiteSpace(TransicaoObservacaoBox.Text));
            var result = await _api.TransitionWorkflowInstanceAsync(_terminal, _terminal.DesktopAccessToken, id, request, _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                Status = $"Transição não aplicada: {result.Detail}";
                MessageBox.Show(Status, "Falha na transição", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var instanceId = SelectedInstance.Id;
            if (!await CarregarInstanciasAsync())
            {
                Status = "A API respondeu, mas a releitura da instância falhou.";
                return;
            }

            var persisted = Instancias.FirstOrDefault(item => string.Equals(item.Id, instanceId, StringComparison.OrdinalIgnoreCase));
            if (persisted is null)
            {
                Status = "A transição foi respondida, mas a instância não apareceu na releitura oficial.";
                MessageBox.Show(Status, "Persistência não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            InstanciasGrid.SelectedItem = persisted;
            SelectedInstance = persisted;
            await CarregarHistoricoAsync();
            Status = $"Transição {result.Data.EstadoOrigem} -> {result.Data.EstadoDestino} persistida e relida no banco oficial.";
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void CarregarHistorico_Click(object sender, RoutedEventArgs e)
    {
        if (!_isBusy)
        {
            await CarregarHistoricoAsync();
        }
    }

    private async Task<bool> CarregarHistoricoAsync()
    {
        if (SelectedInstance is null || !Guid.TryParse(SelectedInstance.Id, out var id) || !EnsureSession())
        {
            Historico.Clear();
            return false;
        }

        var result = await _api.GetWorkflowTransitionsAsync(_terminal, _terminal.DesktopAccessToken, id, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Histórico não carregado: {result.Detail}";
            return false;
        }

        Historico.Clear();
        foreach (var transition in result.Data)
        {
            Historico.Add(transition);
        }

        return true;
    }

    private bool TryBuildDefinitionRequest(out DesktopWorkflowDefinitionRequest request, out string validationError)
    {
        request = null!;
        validationError = string.Empty;
        var entidade = NullIfWhiteSpace(EntidadeBox.Text);
        var codigo = NullIfWhiteSpace(CodigoBox.Text);
        var nome = NullIfWhiteSpace(NomeBox.Text);
        if (entidade is null || codigo is null || nome is null)
        {
            validationError = "Entidade, código e nome são obrigatórios.";
            return false;
        }

        var estados = EstadosBox.Text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => item.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (estados.Count < 2)
        {
            validationError = "Informe pelo menos dois estados, um por linha.";
            return false;
        }

        var transicoes = new List<DesktopWorkflowTransitionRule>();
        var transitionLines = TransicoesBox.Text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in transitionLines)
        {
            var parts = line.Split('|', 4, StringSplitOptions.TrimEntries);
            if (parts.Length < 3 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]) || string.IsNullOrWhiteSpace(parts[2]))
            {
                validationError = $"Transição inválida: {line}. Use ORIGEM|DESTINO|AÇÃO|PERFIS.";
                return false;
            }

            if (!estados.Contains(parts[0], StringComparer.OrdinalIgnoreCase) || !estados.Contains(parts[1], StringComparer.OrdinalIgnoreCase))
            {
                validationError = $"A transição {parts[0]} -> {parts[1]} referencia estado não informado.";
                return false;
            }

            var perfis = parts.Length == 4
                ? parts[3].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                : [];
            transicoes.Add(new DesktopWorkflowTransitionRule(parts[0], parts[1], parts[2], perfis.Count == 0 ? null : perfis));
        }

        if (transicoes.Count == 0)
        {
            validationError = "Informe pelo menos uma transição.";
            return false;
        }

        request = new DesktopWorkflowDefinitionRequest(entidade, codigo, nome, estados, transicoes, AtivoBox.IsChecked == true);
        return true;
    }

    private bool EnsureSession()
    {
        if (!string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            return true;
        }

        Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de operar workflows.";
        return false;
    }

    private static string FormatStates(string json)
    {
        try
        {
            return string.Join(Environment.NewLine, JsonSerializer.Deserialize<List<string>>(json) ?? []);
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static string FormatTransitions(string json)
    {
        try
        {
            var transitions = JsonSerializer.Deserialize<List<DesktopWorkflowTransitionRule>>(json) ?? [];
            return string.Join(Environment.NewLine, transitions.Select(item => $"{item.Origem}|{item.Destino}|{item.Acao}|{string.Join(',', item.PerfisAutorizados ?? [])}"));
        }
        catch (JsonException)
        {
            return string.Empty;
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
