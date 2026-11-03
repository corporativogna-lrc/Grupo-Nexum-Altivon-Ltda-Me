/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7207
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class SitePerfisPublicosWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopSitePublicProfile? _selectedProfile;
    private string _status = "Aguardando sessão administrativa.";
    private string _selectedDetails = "Nenhum perfil selecionado.";
    private string _produtosDisponiveisResumo = "Produtos divulgáveis ainda não foram carregados.";
    private bool _updatingForm;

    public SitePerfisPublicosWindow(TerminalProfile terminal)
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

    public ObservableCollection<DesktopSitePublicProfile> Perfis { get; } = [];
    public ObservableCollection<DesktopSiteProfileOrigin> Origens { get; } = [];
    public ObservableCollection<DesktopSiteProfileOrigin> OrigensDoTipoSelecionado { get; } = [];
    public ObservableCollection<DesktopSiteProfileProduct> ProdutosDisponiveis { get; } = [];
    public ObservableCollection<DesktopSiteMidia> MidiasDisponiveis { get; } = [];

    public DesktopSitePublicProfile? SelectedProfile
    {
        get => _selectedProfile;
        private set
        {
            if (!SetField(ref _selectedProfile, value))
            {
                return;
            }

            if (value is null)
            {
                LimparFormulario();
                return;
            }

            _updatingForm = true;
            SelectComboItem(TipoPerfilBox, value.TipoPerfil);
            SelectComboItem(OrigemTipoBox, value.OrigemTipo);
            AtualizarOrigens(value.OrigemTipo);
            OrigemBox.SelectedValue = value.OrigemId;
            NomeBox.Text = value.Nome;
            SlugBox.Text = value.Slug;
            SegmentoBox.Text = value.Segmento;
            AtividadeBox.Text = value.Atividade;
            DescricaoBox.Text = value.Descricao;
            LogoUrlBox.Text = value.LogoUrl ?? string.Empty;
            BannerUrlBox.Text = value.BannerUrl ?? string.Empty;
            LogoMediaBox.SelectedValue = value.LogoUrl;
            BannerMediaBox.SelectedValue = value.BannerUrl;
            ProdutoIdsBox.Text = string.Join(",", value.ProdutoIds);
            CoresBox.Text = string.Join(";", new[] { value.CorPrimaria, value.CorSecundaria, value.CorFundo, value.CorTexto }.Select(item => item ?? string.Empty));
            SiteUrlBox.Text = value.SiteUrl ?? string.Empty;
            OrdemBox.Text = value.OrdemExibicao.ToString();
            PublicadoBox.IsChecked = value.Publicado;
            CtaTextoBox.Text = value.CtaTexto ?? string.Empty;
            CtaUrlBox.Text = value.CtaUrl ?? string.Empty;
            SelectedDetails = $"{value.TipoPerfil} | origem {value.OrigemTipo} | RowVersion carregada | {value.ProdutoIds.Count} produto(s) autorizado(s).";
            _updatingForm = false;
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

    public string ProdutosDisponiveisResumo
    {
        get => _produtosDisponiveisResumo;
        private set => SetField(ref _produtosDisponiveisResumo, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task<bool> CarregarAsync()
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de gerir perfis públicos.";
            Perfis.Clear();
            return false;
        }

        Status = "Consultando perfis, origens, produtos e mídias da API oficial...";
        var profilesTask = _api.GetSitePublicProfilesAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
        var originsTask = _api.GetSiteProfileOriginsAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
        var productsTask = _api.GetSiteProfileProductsAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
        var mediaTask = _api.GetSiteMidiasAsync(_terminal, _terminal.DesktopAccessToken, null, _lifetime.Token);
        await Task.WhenAll(profilesTask, originsTask, productsTask, mediaTask);

        var profiles = await profilesTask;
        var origins = await originsTask;
        var products = await productsTask;
        var media = await mediaTask;
        var failures = new[]
        {
            profiles.Success && profiles.Data is not null ? string.Empty : profiles.Detail,
            origins.Success && origins.Data is not null ? string.Empty : origins.Detail,
            products.Success && products.Data is not null ? string.Empty : products.Detail,
            media.Success && media.Data is not null ? string.Empty : media.Detail
        }
            .Where(detail => !string.IsNullOrWhiteSpace(detail))
            .ToList();
        if (failures.Count > 0)
        {
            Status = $"Cadastro não carregado: {string.Join(" | ", failures)}";
            return false;
        }

        Perfis.Clear();
        foreach (var item in profiles.Data!)
        {
            Perfis.Add(item);
        }

        Origens.Clear();
        foreach (var item in origins.Data!)
        {
            Origens.Add(item);
        }

        ProdutosDisponiveis.Clear();
        foreach (var item in products.Data!)
        {
            ProdutosDisponiveis.Add(item);
        }

        MidiasDisponiveis.Clear();
        foreach (var item in media.Data!)
        {
            MidiasDisponiveis.Add(item);
        }

        ProdutosDisponiveisResumo = $"{ProdutosDisponiveis.Count} produto(s) elegível(is) para divulgação. Os IDs informados serão validados pela API conforme a origem.";
        if (_selectedProfile is not null)
        {
            SelectedProfile = Perfis.FirstOrDefault(item => item.Id == _selectedProfile.Id);
        }

        Status = $"{Perfis.Count} perfil(is) carregado(s) da API e do banco oficial.";
        return true;
    }

    private async void Carregar_Click(object sender, RoutedEventArgs e)
    {
        await CarregarAsync();
    }

    private void PerfisGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedProfile = PerfisGrid.SelectedItem as DesktopSitePublicProfile;
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        SelectedProfile = null;
        TipoPerfilBox.SelectedIndex = 0;
        OrigemTipoBox.SelectedIndex = 0;
        AtualizarOrigens("Loja");
        Status = "Novo perfil pronto para preenchimento. Nenhum dado foi gravado.";
    }

    private void TipoPerfil_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingForm || TipoPerfilBox.SelectedItem is not ComboBoxItem item)
        {
            return;
        }

        var profileType = item.Content?.ToString();
        if (profileType == "Loja")
        {
            SelectComboItem(OrigemTipoBox, "Loja");
        }
        else if (profileType == "Fornecedor")
        {
            SelectComboItem(OrigemTipoBox, "Fornecedor");
        }
        else if (profileType == "Marketplace")
        {
            SelectComboItem(OrigemTipoBox, "Marketplace");
        }
        else if (profileType is "ParceiroVenda" or "ParceiroCompra")
        {
            SelectComboItem(OrigemTipoBox, "Parceiro");
        }
    }

    private void OrigemTipo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (OrigemTipoBox.SelectedItem is ComboBoxItem item)
        {
            AtualizarOrigens(item.Content?.ToString());
        }
    }

    private void AtualizarOrigens(string? origemTipo)
    {
        OrigensDoTipoSelecionado.Clear();
        foreach (var origin in Origens.Where(item => string.Equals(item.Tipo, origemTipo, StringComparison.OrdinalIgnoreCase)))
        {
            OrigensDoTipoSelecionado.Add(origin);
        }

        OrigemBox.IsEnabled = !string.Equals(origemTipo, "Parceiro", StringComparison.OrdinalIgnoreCase);
        if (!OrigemBox.IsEnabled)
        {
            OrigemBox.SelectedIndex = -1;
        }
    }

    private void LogoMedia_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_updatingForm && LogoMediaBox.SelectedValue is string path && !string.IsNullOrWhiteSpace(path))
        {
            LogoUrlBox.Text = path;
        }
    }

    private void BannerMedia_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_updatingForm && BannerMediaBox.SelectedValue is string path && !string.IsNullOrWhiteSpace(path))
        {
            BannerUrlBox.Text = path;
        }
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (!TryReadForm(out var request, out var error))
        {
            Status = error;
            return;
        }

        Status = SelectedProfile is null
            ? "Criando perfil na API oficial..."
            : "Atualizando perfil com controle de concorrência...";
        var result = await _api.SaveSitePublicProfileAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            SelectedProfile?.Id,
            request,
            _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Perfil não persistido: {result.Detail}";
            return;
        }

        var savedId = result.Data.Id;
        if (!await CarregarAsync())
        {
            Status = "A API confirmou a persistência, mas a releitura falhou. O resultado não será tratado como concluído.";
            return;
        }

        SelectedProfile = Perfis.FirstOrDefault(item => item.Id == savedId);
        Status = "Perfil criado/atualizado e relido do banco oficial.";
    }

    private async void Excluir_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedProfile is null)
        {
            Status = "Selecione um perfil para excluir.";
            return;
        }

        if (MessageBox.Show(this, "Confirma a exclusão do perfil público?", "Excluir perfil", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        var deletedId = SelectedProfile.Id;
        Status = "Excluindo perfil pela API oficial...";
        var result = await _api.DeleteSitePublicProfileAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            deletedId,
            SelectedProfile.RowVersion,
            _lifetime.Token);
        if (!result.Success)
        {
            Status = $"Perfil não excluído: {result.Detail}";
            return;
        }

        if (!await CarregarAsync())
        {
            Status = "A API confirmou a exclusão, mas a releitura falhou. O resultado não será tratado como concluído.";
            return;
        }

        SelectedProfile = null;
        Status = $"Perfil {deletedId} excluído e remoção relida do banco oficial.";
    }

    private bool TryReadForm(out DesktopSitePublicProfileRequest request, out string error)
    {
        request = null!;
        error = string.Empty;
        var tipoPerfil = (TipoPerfilBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
        var origemTipo = (OrigemTipoBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
        var nome = NomeBox.Text.Trim();
        var slug = SlugBox.Text.Trim();
        var segmento = SegmentoBox.Text.Trim();
        var atividade = AtividadeBox.Text.Trim();
        var descricao = DescricaoBox.Text.Trim();
        var logo = NullIfWhiteSpace(LogoUrlBox.Text);
        var banner = NullIfWhiteSpace(BannerUrlBox.Text);
        var originId = OrigemBox.SelectedValue is int id ? (int?)id : null;
        var productIds = ParseProductIds(ProdutoIdsBox.Text, out error);
        if (!string.IsNullOrWhiteSpace(error)) return false;

        if (tipoPerfil is not ("Loja" or "Fornecedor" or "ParceiroVenda" or "ParceiroCompra" or "Marketplace"))
        {
            error = "Selecione um tipo de perfil válido.";
            return false;
        }

        if (origemTipo is not ("Loja" or "Fornecedor" or "Marketplace" or "Parceiro"))
        {
            error = "Selecione uma origem válida.";
            return false;
        }

        if (origemTipo == "Parceiro" && originId.HasValue)
        {
            error = "Parceiros independentes não devem usar origem de outra tabela.";
            return false;
        }

        if (origemTipo != "Parceiro" && !originId.HasValue)
        {
            error = "Selecione um cadastro real de origem.";
            return false;
        }

        if (nome.Length is < 2 or > 160 || slug.Length is < 3 or > 80 || segmento.Length is < 2 or > 120 || atividade.Length is < 5 or > 240 || descricao.Length is < 20 or > 2000)
        {
            error = "Nome, slug, segmento, atividade ou descrição estão fora dos limites oficiais.";
            return false;
        }

        if (logo is null && banner is null)
        {
            error = "Informe uma logomarca ou um banner persistido no storage oficial.";
            return false;
        }

        if (!TryReadColors(out var colors, out error)) return false;
        if (!int.TryParse(OrdemBox.Text.Trim(), out var order) || order is < 0 or > 9999)
        {
            error = "A ordem deve ser um número entre 0 e 9999.";
            return false;
        }

        var ctaText = NullIfWhiteSpace(CtaTextoBox.Text);
        var ctaUrl = NullIfWhiteSpace(CtaUrlBox.Text);
        if ((ctaText is null) != (ctaUrl is null))
        {
            error = "A chamada comercial deve possuir texto e URL juntos.";
            return false;
        }

        request = new DesktopSitePublicProfileRequest(
            tipoPerfil,
            origemTipo,
            originId,
            nome,
            slug,
            segmento,
            atividade,
            descricao,
            logo,
            banner,
            null,
            ctaText,
            ctaUrl,
            NullIfWhiteSpace(SiteUrlBox.Text),
            null,
            null,
            null,
            colors[0],
            colors[1],
            colors[2],
            colors[3],
            productIds,
            PublicadoBox.IsChecked == true,
            order,
            SelectedProfile?.RowVersion);
        return true;
    }

    private static List<int> ParseProductIds(string value, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(value)) return [];
        var ids = new List<int>();
        foreach (var part in value.Split([',', ';', ' ', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(part, out var id) || id <= 0)
            {
                error = $"ID de produto inválido: {part}.";
                return [];
            }
            if (!ids.Contains(id)) ids.Add(id);
        }
        return ids;
    }

    private bool TryReadColors(out string?[] colors, out string error)
    {
        colors = new string?[4];
        error = string.Empty;
        var parts = CoresBox.Text
            .Split(';', StringSplitOptions.None)
            .Select(NullIfWhiteSpace)
            .ToArray();
        for (var index = 0; index < Math.Min(parts.Length, colors.Length); index++) colors[index] = parts[index];
        if (parts.Length > 4)
        {
            error = "Informe no máximo quatro cores: primária;secundária;fundo;texto.";
            return false;
        }
        foreach (var color in colors.Where(item => item is not null))
        {
            if (color!.Length != 7 || color[0] != '#' || !color[1..].All(Uri.IsHexDigit))
            {
                error = "As cores devem usar o formato hexadecimal #RRGGBB.";
                return false;
            }
        }
        return true;
    }

    private void LimparFormulario()
    {
        _updatingForm = true;
        NomeBox.Text = string.Empty;
        SlugBox.Text = string.Empty;
        SegmentoBox.Text = string.Empty;
        AtividadeBox.Text = string.Empty;
        DescricaoBox.Text = string.Empty;
        LogoUrlBox.Text = string.Empty;
        BannerUrlBox.Text = string.Empty;
        LogoMediaBox.SelectedIndex = -1;
        BannerMediaBox.SelectedIndex = -1;
        ProdutoIdsBox.Text = string.Empty;
        CoresBox.Text = string.Empty;
        SiteUrlBox.Text = string.Empty;
        OrdemBox.Text = "0";
        PublicadoBox.IsChecked = false;
        CtaTextoBox.Text = string.Empty;
        CtaUrlBox.Text = string.Empty;
        SelectedDetails = "Novo perfil sem persistência.";
        _updatingForm = false;
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void SelectComboItem(ComboBox comboBox, string value)
    {
        foreach (var item in comboBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedItem = item;
                return;
            }
        }

        comboBox.SelectedIndex = -1;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
