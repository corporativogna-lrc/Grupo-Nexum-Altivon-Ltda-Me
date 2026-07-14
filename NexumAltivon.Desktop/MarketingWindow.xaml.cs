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
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class MarketingWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _profile;
    private string? _token;
    private bool _busy;

    public MarketingWindow(TerminalProfile profile)
    {
        _profile = profile;
        CampaignTypes = new ObservableCollection<string>(["Email", "Sms", "WhatsApp", "MidiaPaga", "Organica", "Evento", "Promocao"]);
        AllowedCampaignStatuses = new ObservableCollection<string>();
        ResetCampaign();
        ResetSegment();
        InitializeComponent();
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<DesktopCrmCampanha> Campaigns { get; } = [];
    public ObservableCollection<DesktopCrmSegmento> Segments { get; } = [];
    public ObservableCollection<string> CampaignTypes { get; }
    public ObservableCollection<string> AllowedCampaignStatuses { get; }

    public string LoginEmail { get; set; } = string.Empty;
    public string MfaCode { get; set; } = string.Empty;
    public bool IsAuthenticated { get; private set; }
    public string SessionLabel { get; private set; } = "Sessao administrativa nao autenticada";
    public string EndpointLabel { get; private set; } = "API oficial obrigatoria";
    public string Status { get; private set; } = "Autentique-se para consultar campanhas e segmentos.";

    public DesktopCrmCampanha? SelectedCampaign { get; set; }
    public DesktopCrmSegmento? SelectedSegment { get; set; }

    public Guid? CampaignId { get; set; }
    public string? CampaignRowVersion { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string CampaignDescription { get; set; } = string.Empty;
    public string CampaignType { get; set; } = "Email";
    public string CampaignStatus { get; set; } = "Rascunho";
    public Guid? CampaignSegmentId { get; set; }
    public DateTime? CampaignStart { get; set; }
    public DateTime? CampaignEnd { get; set; }
    public string CampaignBudget { get; set; } = "0,00";
    public string CampaignCost { get; set; } = "0,00";
    public string CampaignReach { get; set; } = "0";
    public string CampaignClicks { get; set; } = "0";
    public string CampaignLeads { get; set; } = "0";
    public string CampaignOpportunities { get; set; } = "0";
    public string CampaignSales { get; set; } = "0";
    public string CampaignRevenue { get; set; } = "0,00";
    public string CampaignAudience { get; set; } = string.Empty;
    public string CampaignContent { get; set; } = string.Empty;

    public Guid? SegmentId { get; set; }
    public string? SegmentRowVersion { get; set; }
    public string SegmentName { get; set; } = string.Empty;
    public string SegmentDescription { get; set; } = string.Empty;
    public string SegmentColor { get; set; } = "#C9A227";
    public string SegmentPriority { get; set; } = "0";
    public string SegmentTicketMin { get; set; } = string.Empty;
    public string SegmentTicketMax { get; set; } = string.Empty;
    public string SegmentFrequencyMin { get; set; } = string.Empty;
    public string SegmentFrequencyMax { get; set; } = string.Empty;
    public bool SegmentActive { get; set; } = true;

    private async void Authenticate_Click(object sender, RoutedEventArgs e)
    {
        if (_busy)
        {
            return;
        }

        SetBusy(true, "Autenticando na API oficial...");
        try
        {
            var result = await _api.AuthenticateAsync(_profile, LoginEmail, LoginPassword.Password, MfaCode);
            if (!result.Success || string.IsNullOrWhiteSpace(result.Token) || result.User is null)
            {
                IsAuthenticated = false;
                _token = null;
                Status = $"Autenticacao recusada: {result.Detail}";
                RefreshBindings();
                return;
            }

            _token = result.Token;
            LoginPassword.Clear();
            IsAuthenticated = true;
            SessionLabel = $"{result.User.Nome} / {result.User.Perfil}";
            EndpointLabel = _profile.ApiBaseUrl;
            await ReloadAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ReloadAsync()
    {
        if (string.IsNullOrWhiteSpace(_token))
        {
            return;
        }

        var segmentResult = await _api.GetMarketingSegmentsAsync(_profile, _token);
        if (!segmentResult.Success || segmentResult.Data is null)
        {
            Status = $"Falha ao carregar segmentos: {segmentResult.Detail}";
            RefreshBindings();
            return;
        }

        var campaignResult = await _api.GetMarketingCampaignsAsync(_profile, _token);
        if (!campaignResult.Success || campaignResult.Data is null)
        {
            Status = $"Falha ao carregar campanhas: {campaignResult.Detail}";
            RefreshBindings();
            return;
        }

        ReplaceCollection(Segments, segmentResult.Data);
        ReplaceCollection(Campaigns, campaignResult.Data);
        Status = $"API confirmou {Campaigns.Count} campanha(s) e {Segments.Count} segmento(s) no tenant atual.";
        RefreshBindings();
    }

    private async void SaveCampaign_Click(object sender, RoutedEventArgs e)
    {
        if (_busy || string.IsNullOrWhiteSpace(_token))
        {
            return;
        }

        if (!TryBuildCampaignRequest(out var request, out var validation))
        {
            Status = validation;
            RefreshBindings();
            return;
        }

        SetBusy(true, "Salvando campanha na API...");
        try
        {
            var result = await _api.SaveMarketingCampaignAsync(_profile, _token, request!, CampaignId);
            if (!result.Success || result.Data is null || result.Data.Id == Guid.Empty || string.IsNullOrWhiteSpace(result.Data.RowVersion))
            {
                Status = $"Campanha nao confirmada: {result.Detail}";
                RefreshBindings();
                return;
            }

            FillCampaign(result.Data);
            await ReloadAsync();
            Status = $"Campanha {result.Data.Nome} persistida com ID {result.Data.Id}.";
            RefreshBindings();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void SaveSegment_Click(object sender, RoutedEventArgs e)
    {
        if (_busy || string.IsNullOrWhiteSpace(_token))
        {
            return;
        }

        if (!TryBuildSegmentRequest(out var request, out var validation))
        {
            Status = validation;
            RefreshBindings();
            return;
        }

        SetBusy(true, "Salvando segmento na API...");
        try
        {
            var result = await _api.SaveMarketingSegmentAsync(_profile, _token, request!, SegmentId);
            if (!result.Success || result.Data is null || result.Data.Id == Guid.Empty || string.IsNullOrWhiteSpace(result.Data.RowVersion))
            {
                Status = $"Segmento nao confirmado: {result.Detail}";
                RefreshBindings();
                return;
            }

            FillSegment(result.Data);
            await ReloadAsync();
            Status = $"Segmento {result.Data.Nome} persistido com ID {result.Data.Id}.";
            RefreshBindings();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void DeleteSelected_Click(object sender, RoutedEventArgs e)
    {
        if (_busy || string.IsNullOrWhiteSpace(_token))
        {
            return;
        }

        if (MainTabs.SelectedIndex == 0 && SelectedCampaign is not null)
        {
            if (MessageBox.Show($"Excluir a campanha {SelectedCampaign.Nome}?", "Confirmar exclusao", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            SetBusy(true, "Excluindo campanha...");
            try
            {
                var result = await _api.DeleteMarketingCampaignAsync(_profile, _token, SelectedCampaign.Id);
                Status = result.Success ? "Exclusao da campanha confirmada pela API." : $"Exclusao recusada: {result.Detail}";
                if (result.Success)
                {
                    ResetCampaign();
                    await ReloadAsync();
                }
            }
            finally
            {
                SetBusy(false);
            }
            return;
        }

        if (MainTabs.SelectedIndex == 1 && SelectedSegment is not null)
        {
            if (MessageBox.Show($"Excluir o segmento {SelectedSegment.Nome}?", "Confirmar exclusao", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            SetBusy(true, "Excluindo segmento...");
            try
            {
                var result = await _api.DeleteMarketingSegmentAsync(_profile, _token, SelectedSegment.Id);
                Status = result.Success ? "Exclusao do segmento confirmada pela API." : $"Exclusao recusada: {result.Detail}";
                if (result.Success)
                {
                    ResetSegment();
                    await ReloadAsync();
                }
            }
            finally
            {
                SetBusy(false);
            }
        }
    }

    private void CampaignSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedCampaign is not null)
        {
            FillCampaign(SelectedCampaign);
        }
    }

    private void SegmentSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedSegment is not null)
        {
            FillSegment(SelectedSegment);
        }
    }

    private void NewCampaign_Click(object sender, RoutedEventArgs e) => ResetCampaign();
    private void NewSegment_Click(object sender, RoutedEventArgs e) => ResetSegment();

    private bool TryBuildCampaignRequest(out DesktopCrmCampanhaRequest? request, out string validation)
    {
        request = null;
        validation = string.Empty;
        if (string.IsNullOrWhiteSpace(CampaignName) || CampaignName.Trim().Length < 3)
        {
            validation = "Informe um nome de campanha com ao menos 3 caracteres.";
            return false;
        }

        if (!CampaignStart.HasValue)
        {
            validation = "Informe a data inicial da campanha.";
            return false;
        }

        if (!TryDecimal(CampaignBudget, out var budget) || !TryDecimal(CampaignCost, out var cost) || !TryDecimal(CampaignRevenue, out var revenue)
            || !TryInteger(CampaignReach, out var reach) || !TryInteger(CampaignClicks, out var clicks)
            || !TryInteger(CampaignLeads, out var leads) || !TryInteger(CampaignOpportunities, out var opportunities)
            || !TryInteger(CampaignSales, out var sales))
        {
            validation = "Valores financeiros e metricas devem ser numericos e nao negativos.";
            return false;
        }

        request = new DesktopCrmCampanhaRequest(
            CampaignName.Trim(), NullIfWhiteSpace(CampaignDescription), CampaignType, CampaignStatus,
            CampaignSegmentId, CampaignStart.Value.ToUniversalTime(), CampaignEnd?.ToUniversalTime(), budget, cost,
            reach, clicks, leads, opportunities, sales, revenue, NullIfWhiteSpace(CampaignAudience),
            NullIfWhiteSpace(CampaignContent), CampaignRowVersion);
        return true;
    }

    private bool TryBuildSegmentRequest(out DesktopCrmSegmentoRequest? request, out string validation)
    {
        request = null;
        validation = string.Empty;
        if (string.IsNullOrWhiteSpace(SegmentName) || SegmentName.Trim().Length < 3)
        {
            validation = "Informe um nome de segmento com ao menos 3 caracteres.";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(SegmentColor, "^#[0-9A-Fa-f]{6}$") || !TryInteger(SegmentPriority, out var priority))
        {
            validation = "Cor deve usar #RRGGBB e prioridade deve ser numerica nao negativa.";
            return false;
        }

        if (!TryOptionalDecimal(SegmentTicketMin, out var ticketMin) || !TryOptionalDecimal(SegmentTicketMax, out var ticketMax)
            || !TryOptionalInteger(SegmentFrequencyMin, out var frequencyMin) || !TryOptionalInteger(SegmentFrequencyMax, out var frequencyMax))
        {
            validation = "Faixas de ticket e frequencia devem conter numeros nao negativos.";
            return false;
        }

        request = new DesktopCrmSegmentoRequest(
            SegmentName.Trim(), NullIfWhiteSpace(SegmentDescription), SegmentColor.ToUpperInvariant(), priority,
            ticketMin, ticketMax, frequencyMin, frequencyMax, SegmentActive, SegmentRowVersion);
        return true;
    }

    private void FillCampaign(DesktopCrmCampanha item)
    {
        CampaignId = item.Id;
        CampaignRowVersion = item.RowVersion;
        CampaignName = item.Nome;
        CampaignDescription = item.Descricao ?? string.Empty;
        CampaignType = item.Tipo;
        SetAllowedStatuses(item.Status);
        CampaignStatus = item.Status;
        CampaignSegmentId = item.SegmentoId;
        CampaignStart = item.DataInicio.ToLocalTime();
        CampaignEnd = item.DataFim?.ToLocalTime();
        CampaignBudget = FormatDecimal(item.Orcamento);
        CampaignCost = FormatDecimal(item.CustoAtual);
        CampaignReach = item.Alcance.ToString(CultureInfo.CurrentCulture);
        CampaignClicks = item.Cliques.ToString(CultureInfo.CurrentCulture);
        CampaignLeads = item.LeadsGerados.ToString(CultureInfo.CurrentCulture);
        CampaignOpportunities = item.OportunidadesGeradas.ToString(CultureInfo.CurrentCulture);
        CampaignSales = item.VendasGeradas.ToString(CultureInfo.CurrentCulture);
        CampaignRevenue = FormatDecimal(item.ReceitaGerada);
        CampaignAudience = item.PublicoAlvo ?? string.Empty;
        CampaignContent = item.Conteudo ?? string.Empty;
        RefreshBindings();
    }

    private void FillSegment(DesktopCrmSegmento item)
    {
        SegmentId = item.Id;
        SegmentRowVersion = item.RowVersion;
        SegmentName = item.Nome;
        SegmentDescription = item.Descricao ?? string.Empty;
        SegmentColor = item.Cor;
        SegmentPriority = item.Prioridade.ToString(CultureInfo.CurrentCulture);
        SegmentTicketMin = item.TicketMedioMinimo.HasValue ? FormatDecimal(item.TicketMedioMinimo.Value) : string.Empty;
        SegmentTicketMax = item.TicketMedioMaximo.HasValue ? FormatDecimal(item.TicketMedioMaximo.Value) : string.Empty;
        SegmentFrequencyMin = item.FrequenciaMinimaDias?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        SegmentFrequencyMax = item.FrequenciaMaximaDias?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        SegmentActive = item.Ativo;
        RefreshBindings();
    }

    private void ResetCampaign()
    {
        CampaignId = null;
        CampaignRowVersion = null;
        CampaignName = string.Empty;
        CampaignDescription = string.Empty;
        CampaignType = "Email";
        SetAllowedStatuses("Rascunho", true);
        CampaignStatus = "Rascunho";
        CampaignSegmentId = null;
        CampaignStart = DateTime.Today;
        CampaignEnd = null;
        CampaignBudget = CampaignCost = CampaignRevenue = "0,00";
        CampaignReach = CampaignClicks = CampaignLeads = CampaignOpportunities = CampaignSales = "0";
        CampaignAudience = CampaignContent = string.Empty;
        SelectedCampaign = null;
        RefreshBindings();
    }

    private void ResetSegment()
    {
        SegmentId = null;
        SegmentRowVersion = null;
        SegmentName = SegmentDescription = string.Empty;
        SegmentColor = "#C9A227";
        SegmentPriority = "0";
        SegmentTicketMin = SegmentTicketMax = SegmentFrequencyMin = SegmentFrequencyMax = string.Empty;
        SegmentActive = true;
        SelectedSegment = null;
        RefreshBindings();
    }

    private void SetAllowedStatuses(string current, bool creating = false)
    {
        var values = creating
            ? new[] { "Rascunho", "Agendada" }
            : current switch
            {
                "Rascunho" => ["Rascunho", "Agendada", "EmAndamento", "Cancelada"],
                "Agendada" => ["Agendada", "EmAndamento", "Pausada", "Cancelada"],
                "EmAndamento" => ["EmAndamento", "Pausada", "Concluida", "Cancelada"],
                "Pausada" => ["Pausada", "EmAndamento", "Concluida", "Cancelada"],
                _ => new[] { current }
            };
        ReplaceCollection(AllowedCampaignStatuses, values);
    }

    private void SetBusy(bool value, string? message = null)
    {
        _busy = value;
        if (!string.IsNullOrWhiteSpace(message))
        {
            Status = message;
        }
        RefreshBindings();
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private static bool TryDecimal(string value, out decimal result)
        => (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result)
            || decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result)) && result >= 0;

    private static bool TryInteger(string value, out int result)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out result) && result >= 0;

    private static bool TryOptionalDecimal(string value, out decimal? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (!TryDecimal(value, out var parsed)) return false;
        result = parsed;
        return true;
    }

    private static bool TryOptionalInteger(string value, out int? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (!TryInteger(value, out var parsed)) return false;
        result = parsed;
        return true;
    }

    private static string FormatDecimal(decimal value) => value.ToString("N2", CultureInfo.CurrentCulture);
    private static string? NullIfWhiteSpace(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void RefreshBindings()
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
}
