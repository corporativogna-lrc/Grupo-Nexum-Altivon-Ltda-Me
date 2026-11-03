/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7206
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class SiteMidiasWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private string _status = "Aguardando sessão administrativa.";
    private string _selectedFilePath = "Nenhum arquivo selecionado.";
    private DesktopSiteMidia? _selectedMidia;

    public SiteMidiasWindow(TerminalProfile terminal)
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

    public ObservableCollection<DesktopSiteMidia> Midias { get; } = [];

    public DesktopSiteMidia? SelectedMidia
    {
        get => _selectedMidia;
        private set
        {
            if (!SetField(ref _selectedMidia, value))
            {
                return;
            }

            if (value is null)
            {
                NomeBox.Text = string.Empty;
                TextoAlternativoBox.Text = string.Empty;
                TipoBox.SelectedIndex = -1;
                SelectedDetails = "Nenhum registro selecionado.";
                return;
            }

            NomeBox.Text = value.Nome;
            TextoAlternativoBox.Text = value.TextoAlternativo;
            TipoBox.SelectedIndex = value.Tipo switch
            {
                "Logo" => 0,
                "Banner" => 1,
                "Loja" => 2,
                "Institucional" => 3,
                _ => -1
            };
            SelectedDetails = $"{value.Tipo} | {value.Largura} x {value.Altura} | {value.TamanhoBytes:N0} bytes | RowVersion carregada.\n{value.Url}";
        }
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string SelectedFilePath
    {
        get => _selectedFilePath;
        private set => SetField(ref _selectedFilePath, value);
    }

    private string _selectedDetails = "Nenhum registro selecionado.";

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
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de acessar a biblioteca.";
            Midias.Clear();
            return false;
        }

        var tipo = TipoFiltroBox.SelectedItem is ComboBoxItem item
            && !string.Equals(item.Content?.ToString(), "Todos", StringComparison.OrdinalIgnoreCase)
            ? item.Content?.ToString()
            : null;

        Status = "Consultando /api/site/midias na API oficial...";
        var result = await _api.GetSiteMidiasAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            tipo,
            _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Biblioteca não carregada: {result.Detail}";
            Midias.Clear();
            return false;
        }

        Midias.Clear();
        foreach (var media in result.Data)
        {
            Midias.Add(media);
        }

        if (_selectedMidia is not null)
        {
            SelectedMidia = Midias.FirstOrDefault(item => item.Id == _selectedMidia.Id);
        }

        Status = $"{Midias.Count} mídia(s) carregada(s) da API e do banco oficial.";
        return true;
    }

    private async void Carregar_Click(object sender, RoutedEventArgs e)
    {
        await CarregarAsync();
    }

    private async void TipoFiltro_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
        {
            await CarregarAsync();
        }
    }

    private void MidiasGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedMidia = MidiasGrid.SelectedItem as DesktopSiteMidia;
    }

    private void SelecionarArquivo_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Selecionar mídia oficial",
            Filter = "Imagens validadas (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var contentType = GetContentType(dialog.FileName);
        var fileInfo = new FileInfo(dialog.FileName);
        if (string.IsNullOrWhiteSpace(contentType))
        {
            Status = "Arquivo recusado: apenas PNG, JPEG e WebP são aceitos pela API.";
            return;
        }

        if (fileInfo.Length is <= 0 or > 8_388_608)
        {
            Status = "Arquivo recusado: o tamanho deve estar entre 1 byte e 8 MB.";
            return;
        }

        SelectedFilePath = dialog.FileName;
        Status = $"Arquivo selecionado: {fileInfo.Length:N0} bytes. A API fará a validação estrutural da imagem.";
    }

    private async void Enviar_Click(object sender, RoutedEventArgs e)
    {
        if (!TryReadForm(out var nome, out var tipo, out var textoAlternativo, out var contentType, out var error))
        {
            Status = error;
            return;
        }

        if (!File.Exists(SelectedFilePath))
        {
            Status = "Selecione um arquivo de imagem antes de enviar.";
            return;
        }

        try
        {
            Status = "Lendo a imagem e enviando para a API oficial...";
            var bytes = await File.ReadAllBytesAsync(SelectedFilePath, _lifetime.Token);
            var dataUrl = $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";
            var payload = new DesktopSiteMidiaUploadRequest(
                nome,
                tipo,
                textoAlternativo,
                Path.GetFileName(SelectedFilePath),
                contentType,
                dataUrl);
            var result = await _api.UploadSiteMidiaAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                payload,
                _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                Status = $"Mídia não gravada: {result.Detail}";
                return;
            }

            var createdId = result.Data.Id;
            SelectedFilePath = "Nenhum arquivo selecionado.";
            if (!await CarregarAsync())
            {
                Status = "A API confirmou a gravação, mas a releitura da biblioteca falhou. O resultado não será tratado como concluído.";
                return;
            }

            SelectedMidia = Midias.FirstOrDefault(item => item.Id == createdId);
            Status = "Mídia criada, persistida e relida do banco oficial.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OperationCanceledException)
        {
            Status = $"Mídia não gravada: {ex.Message}";
        }
    }

    private async void Atualizar_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedMidia is null)
        {
            Status = "Selecione uma mídia para atualizar.";
            return;
        }

        if (!TryReadForm(out var nome, out var tipo, out var textoAlternativo, out _, out var error))
        {
            Status = error;
            return;
        }

        Status = "Atualizando metadados com controle de concorrência...";
        var result = await _api.UpdateSiteMidiaAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            SelectedMidia.Id,
            new DesktopSiteMidiaUpdateRequest(nome, tipo, textoAlternativo, SelectedMidia.RowVersion),
            _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Metadados não atualizados: {result.Detail}";
            return;
        }

        var updatedId = result.Data.Id;
        if (!await CarregarAsync())
        {
            Status = "A API confirmou a atualização, mas a releitura falhou. O resultado não será tratado como concluído.";
            return;
        }

        SelectedMidia = Midias.FirstOrDefault(item => item.Id == updatedId);
        Status = "Metadados atualizados, persistidos e relidos do banco oficial.";
    }

    private async void Excluir_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedMidia is null)
        {
            Status = "Selecione uma mídia para excluir.";
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            "A API excluirá o arquivo físico e o registro. Mídias vinculadas a configurações ou perfis serão recusadas. Confirma?",
            "Excluir mídia oficial",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        Status = "Solicitando exclusão protegida à API oficial...";
        var deletedId = SelectedMidia.Id;
        var result = await _api.DeleteSiteMidiaAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            deletedId,
            SelectedMidia.RowVersion,
            _lifetime.Token);
        if (!result.Success)
        {
            Status = $"Mídia não excluída: {result.Detail}";
            return;
        }

        if (!await CarregarAsync())
        {
            Status = "A API confirmou a exclusão, mas a releitura falhou. O resultado não será tratado como concluído.";
            return;
        }

        SelectedMidia = null;
        Status = $"Mídia {deletedId} excluída e remoção relida do banco oficial.";
    }

    private void AbrirUrl_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedMidia is null || string.IsNullOrWhiteSpace(SelectedMidia.Url))
        {
            Status = "Selecione uma mídia com URL pública persistida.";
            return;
        }

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = SelectedMidia.Url,
                UseShellExecute = true
            });
        }
        catch (InvalidOperationException ex)
        {
            Status = $"URL pública não foi aberta: {ex.Message}";
        }
    }

    private bool TryReadForm(
        out string nome,
        out string tipo,
        out string textoAlternativo,
        out string contentType,
        out string error)
    {
        nome = NomeBox.Text.Trim();
        tipo = (TipoBox.SelectedItem as ComboBoxItem)?.Content?.ToString()?.Trim() ?? string.Empty;
        textoAlternativo = TextoAlternativoBox.Text.Trim();
        contentType = string.IsNullOrWhiteSpace(SelectedFilePath) || SelectedFilePath.StartsWith("Nenhum", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : GetContentType(SelectedFilePath);
        error = string.Empty;

        if (nome.Length is < 3 or > 150)
        {
            error = "O nome da mídia deve possuir entre 3 e 150 caracteres.";
            return false;
        }

        if (tipo is not ("Logo" or "Banner" or "Loja" or "Institucional"))
        {
            error = "Selecione um tipo oficial de mídia.";
            return false;
        }

        if (textoAlternativo.Length is < 3 or > 240)
        {
            error = "O texto alternativo deve possuir entre 3 e 240 caracteres.";
            return false;
        }

        return true;
    }

    private static string GetContentType(string path)
        => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => string.Empty
        };

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
