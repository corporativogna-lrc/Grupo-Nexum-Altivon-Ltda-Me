/* Gestão de empresas do grupo vinculada à API e ao banco operacional. */
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class EmpresasGrupoWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopEmpresaLojaVinculo? _selected;
    private string _status = "Aguardando sessão autenticada.";
    private string _buscaEmpresa = string.Empty;

    public EmpresasGrupoWindow(TerminalProfile terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        InitializeComponent();
        _terminal = terminal;
        DataContext = this;
        LojasView = CollectionViewSource.GetDefaultView(Lojas);
        LojasView.Filter = FiltrarLoja;
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => { _lifetime.Cancel(); _lifetime.Dispose(); };
    }

    public ObservableCollection<DesktopEmpresaLojaVinculo> Lojas { get; } = [];
    public ICollectionView LojasView { get; }
    public string BuscaEmpresa
    {
        get => _buscaEmpresa;
        set { _buscaEmpresa = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuscaEmpresa))); }
    }
    public string Status
    {
        get => _status;
        private set { _status = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task<bool> LoadAsync()
    {
        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            Lojas.Clear();
            Status = "Sessão ausente. Entre no Desktop para acessar as empresas.";
            return false;
        }

        Status = "Consultando lojas e vínculos empresariais…";
        var result = await _api.GetEmpresasGrupoLojasAsync(_terminal, token, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Lojas.Clear();
            Status = $"Falha ao consultar empresas: {result.Detail}";
            return false;
        }

        Lojas.Clear();
        foreach (var loja in result.Data) Lojas.Add(loja);
        Status = $"{Lojas.Count} loja(s) carregada(s) da API.";
        if (_selected is not null)
            LojasGrid.SelectedItem = Lojas.FirstOrDefault(item => item.LojaId == _selected.LojaId);
        return true;
    }

    private async void Recarregar_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private void Pesquisar_Click(object sender, RoutedEventArgs e)
    {
        LojasView.Refresh();
        Status = $"{LojasView.Cast<object>().Count()} loja(s) correspondente(s) na API.";
    }

    private void EditarSelecionada_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
        {
            Status = "Selecione uma loja antes de editar os dados empresariais.";
            return;
        }

        RazaoSocialBox.Text = _selected.RazaoSocial ?? string.Empty;
        NomeFantasiaBox.Text = _selected.NomeFantasia ?? _selected.NomeLoja;
        CnpjBox.Text = _selected.Cnpj ?? string.Empty;
        AtivaCheck.IsChecked = _selected.EmpresaAtiva ?? false;
        RazaoSocialBox.Focus();
        Status = $"Dados empresariais de {_selected.NomeLoja} carregados para edição.";
    }

    private bool FiltrarLoja(object item)
    {
        if (item is not DesktopEmpresaLojaVinculo loja) return false;
        var termo = BuscaEmpresa.Trim();
        if (termo.Length == 0) return true;
        return loja.NomeLoja.Contains(termo, StringComparison.CurrentCultureIgnoreCase)
            || (loja.Segmento?.Contains(termo, StringComparison.CurrentCultureIgnoreCase) ?? false)
            || (loja.RazaoSocial?.Contains(termo, StringComparison.CurrentCultureIgnoreCase) ?? false)
            || (loja.Cnpj?.Contains(termo, StringComparison.CurrentCultureIgnoreCase) ?? false);
    }

    private async void CriarRascunhos_Click(object sender, RoutedEventArgs e)
    {
        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            Status = "Sessão administrativa ausente.";
            return;
        }

        var pendentes = Lojas.Where(item => item.EmpresaId is null).ToList();
        if (pendentes.Count == 0)
        {
            Status = "Todas as lojas já possuem empresa vinculada.";
            return;
        }

        CriarRascunhosButton.IsEnabled = false;
        var criadas = 0;
        try
        {
            foreach (var loja in pendentes)
            {
                var result = await _api.CreateEmpresaGrupoDraftAsync(_terminal, token, loja.LojaId, _lifetime.Token);
                if (!result.Success)
                {
                    Status = $"Falha ao criar rascunho para {loja.NomeLoja}: {result.Detail}";
                    return;
                }
                criadas++;
            }

            await LoadAsync();
            Status = $"{criadas} rascunho(s) empresarial(is) persistido(s). Complete CNPJ e dados legais antes de ativar.";
        }
        catch (OperationCanceledException)
        {
            Status = "Operação cancelada ao fechar a janela.";
        }
        finally
        {
            CriarRascunhosButton.IsEnabled = true;
        }
    }

    private void LojasGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = LojasGrid.SelectedItem as DesktopEmpresaLojaVinculo;
        var hasCompany = _selected?.EmpresaId is not null;
        LojaInfoText.Text = _selected is null
            ? "Selecione uma loja."
            : $"{_selected.NomeLoja} · {_selected.Segmento ?? "Segmento não informado"}\n" +
              (hasCompany ? $"Empresa ID {_selected.EmpresaId}; CNPJ {_selected.Cnpj ?? "pendente"}." : "Sem empresa vinculada; crie um rascunho para iniciar.");
        RazaoSocialBox.Text = _selected?.RazaoSocial ?? string.Empty;
        NomeFantasiaBox.Text = _selected?.NomeFantasia ?? _selected?.NomeLoja ?? string.Empty;
        CnpjBox.Text = _selected?.Cnpj ?? string.Empty;
        AtivaCheck.IsChecked = _selected?.EmpresaAtiva ?? false;
        SalvarButton.IsEnabled = hasCompany;
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (_selected?.EmpresaId is not int empresaId || string.IsNullOrWhiteSpace(_selected.RowVersion))
        {
            Status = "Crie ou recarregue o rascunho da empresa antes de salvar.";
            return;
        }

        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            Status = "Sessão administrativa ausente.";
            return;
        }

        SalvarButton.IsEnabled = false;
        try
        {
            var payload = new DesktopEmpresaDadosLegaisRequest(
                RazaoSocialBox.Text, NomeFantasiaBox.Text, CnpjBox.Text,
                ResponsavelBox.Text, null, EmailFiscalBox.Text, null, TelefoneBox.Text, null,
                null, null, null, null, null, null, null, RegimeBox.Text,
                AtivaCheck.IsChecked == true, _selected.RowVersion);
            var result = await _api.UpdateEmpresaGrupoLegalDataAsync(
                _terminal, token, empresaId, payload, _lifetime.Token);
            Status = result.Success ? result.Detail : $"Falha ao salvar dados legais: {result.Detail}";
            if (result.Success) await LoadAsync();
        }
        catch (OperationCanceledException)
        {
            Status = "Operação cancelada ao fechar a janela.";
        }
        finally
        {
            SalvarButton.IsEnabled = _selected?.EmpresaId is not null;
        }
    }
}
