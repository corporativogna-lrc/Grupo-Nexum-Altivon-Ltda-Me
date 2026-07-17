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
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class AccessManagementWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _apiClient = new();
    private readonly TerminalProfile _terminal;
    private bool _isAuthenticated;
    private string _loginEmail = string.Empty;
    private string _mfaCode = string.Empty;
    private string _sessionLabel = "Sessão administrativa ausente";
    private string _endpointLabel = "API oficial não autenticada";
    private string _status = "Autentique-se com perfil Admin ou SuperAdmin para carregar dados reais.";
    private DesktopUsuarioAcesso? _selectedUser;
    private string _userName = string.Empty;
    private string _userEmail = string.Empty;
    private string _userPhone = string.Empty;
    private string _userRole = "Vendedor";
    private bool _userActive = true;
    private DesktopPerfilAcesso? _selectedProfile;
    private string _profileName = string.Empty;
    private string _profileDescription = string.Empty;
    private string _profileApprovalLimit = "0,00";
    private string _profileHierarchyLevel = "1";
    private bool _profileActive = true;
    private DesktopPermissaoAcesso? _selectedPermission;
    private string _permissionModule = string.Empty;
    private string _permissionFunctionality = string.Empty;
    private string _permissionKey = string.Empty;
    private string _permissionDescription = string.Empty;
    private bool _permissionActive = true;
    private DesktopPerfilAcesso? _matrixProfile;
    private DesktopPermissaoAcesso? _matrixPermission;
    private DesktopPerfilPermissao? _selectedProfilePermission;
    private bool _matrixRead = true;
    private bool _matrixWrite;
    private bool _matrixDelete;
    private bool _matrixPrint;

    public ObservableCollection<DesktopUsuarioAcesso> Users { get; } = new();
    public ObservableCollection<string> AdministrativeRoles { get; } = new();
    public ObservableCollection<DesktopPerfilAcesso> Profiles { get; } = new();
    public ObservableCollection<DesktopPermissaoAcesso> Permissions { get; } = new();
    public ObservableCollection<DesktopPermissaoAcesso> ActivePermissions { get; } = new();
    public ObservableCollection<DesktopPerfilPermissao> ProfilePermissions { get; } = new();

    public bool IsAuthenticated { get => _isAuthenticated; private set => SetField(ref _isAuthenticated, value); }
    public string LoginEmail { get => _loginEmail; set => SetField(ref _loginEmail, value); }
    public string MfaCode { get => _mfaCode; set => SetField(ref _mfaCode, value); }
    public string SessionLabel { get => _sessionLabel; private set => SetField(ref _sessionLabel, value); }
    public string EndpointLabel { get => _endpointLabel; private set => SetField(ref _endpointLabel, value); }
    public string Status { get => _status; private set => SetField(ref _status, value); }

    public DesktopUsuarioAcesso? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (!SetField(ref _selectedUser, value) || value is null)
            {
                return;
            }

            UserName = value.Nome;
            UserEmail = value.Email;
            UserPhone = value.Telefone ?? string.Empty;
            UserRole = value.Perfil;
            UserActive = value.Ativo;
            UserPassword.Clear();
        }
    }

    public string UserName { get => _userName; set => SetField(ref _userName, value); }
    public string UserEmail { get => _userEmail; set => SetField(ref _userEmail, value); }
    public string UserPhone { get => _userPhone; set => SetField(ref _userPhone, value); }
    public string UserRole { get => _userRole; set => SetField(ref _userRole, value); }
    public bool UserActive { get => _userActive; set => SetField(ref _userActive, value); }

    public DesktopPerfilAcesso? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (!SetField(ref _selectedProfile, value) || value is null)
            {
                return;
            }

            ProfileName = value.Nome;
            ProfileDescription = value.Descricao ?? string.Empty;
            ProfileApprovalLimit = value.AlcadaMaxima.ToString("N2", CultureInfo.CurrentCulture);
            ProfileHierarchyLevel = value.NivelHierarquico.ToString(CultureInfo.InvariantCulture);
            ProfileActive = value.Ativo;
        }
    }

    public string ProfileName { get => _profileName; set => SetField(ref _profileName, value); }
    public string ProfileDescription { get => _profileDescription; set => SetField(ref _profileDescription, value); }
    public string ProfileApprovalLimit { get => _profileApprovalLimit; set => SetField(ref _profileApprovalLimit, value); }
    public string ProfileHierarchyLevel { get => _profileHierarchyLevel; set => SetField(ref _profileHierarchyLevel, value); }
    public bool ProfileActive { get => _profileActive; set => SetField(ref _profileActive, value); }

    public DesktopPermissaoAcesso? SelectedPermission
    {
        get => _selectedPermission;
        set
        {
            if (!SetField(ref _selectedPermission, value) || value is null)
            {
                return;
            }

            PermissionModule = value.Modulo;
            PermissionFunctionality = value.Funcionalidade;
            PermissionKey = value.Chave;
            PermissionDescription = value.Descricao ?? string.Empty;
            PermissionActive = value.Ativo;
        }
    }

    public string PermissionModule { get => _permissionModule; set => SetField(ref _permissionModule, value); }
    public string PermissionFunctionality { get => _permissionFunctionality; set => SetField(ref _permissionFunctionality, value); }
    public string PermissionKey { get => _permissionKey; set => SetField(ref _permissionKey, value); }
    public string PermissionDescription { get => _permissionDescription; set => SetField(ref _permissionDescription, value); }
    public bool PermissionActive { get => _permissionActive; set => SetField(ref _permissionActive, value); }

    public DesktopPerfilAcesso? MatrixProfile { get => _matrixProfile; set => SetField(ref _matrixProfile, value); }
    public DesktopPermissaoAcesso? MatrixPermission { get => _matrixPermission; set => SetField(ref _matrixPermission, value); }
    public bool MatrixRead { get => _matrixRead; set => SetField(ref _matrixRead, value); }
    public bool MatrixWrite { get => _matrixWrite; set => SetField(ref _matrixWrite, value); }
    public bool MatrixDelete { get => _matrixDelete; set => SetField(ref _matrixDelete, value); }
    public bool MatrixPrint { get => _matrixPrint; set => SetField(ref _matrixPrint, value); }

    public DesktopPerfilPermissao? SelectedProfilePermission
    {
        get => _selectedProfilePermission;
        set
        {
            if (!SetField(ref _selectedProfilePermission, value) || value is null)
            {
                return;
            }

            MatrixPermission = ActivePermissions.FirstOrDefault(item => item.Id == value.PermissaoId);
            MatrixRead = value.Leitura;
            MatrixWrite = value.Escrita;
            MatrixDelete = value.Exclusao;
            MatrixPrint = value.Impressao;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public AccessManagementWindow(TerminalProfile terminal)
    {
        _terminal = terminal;
        InitializeComponent();
        DataContext = this;
    }

    private async void Authenticate_Click(object sender, RoutedEventArgs e)
    {
        Status = "Autenticando na API oficial...";
        var result = await _apiClient.AuthenticateAsync(_terminal, LoginEmail, LoginPassword.Password, MfaCode);
        if (!result.Success || result.User is null || string.IsNullOrWhiteSpace(result.Token))
        {
            ClearSession();
            Status = $"Autenticação recusada: {result.Detail}";
            return;
        }

        if (result.User.Perfil is not ("Admin" or "SuperAdmin"))
        {
            ClearSession();
            Status = $"O perfil {result.User.Perfil} não possui autorização para gerir acessos.";
            return;
        }

        _terminal.DesktopAccessToken = result.Token;
        IsAuthenticated = true;
        SessionLabel = $"{result.User.Nome} / {result.User.Perfil}";
        EndpointLabel = result.Detail;
        LoginPassword.Clear();
        MfaCode = string.Empty;
        await LoadAllAsync();
    }

    private async Task LoadAllAsync()
    {
        Status = "Carregando usuários, perfis e permissões da API oficial...";
        var usersTask = _apiClient.GetAccessUsersAsync(_terminal, _terminal.DesktopAccessToken);
        var rolesTask = _apiClient.GetAdministrativeRolesAsync(_terminal, _terminal.DesktopAccessToken);
        var profilesTask = _apiClient.GetAccessProfilesAsync(_terminal, _terminal.DesktopAccessToken);
        var permissionsTask = _apiClient.GetPermissionsAsync(_terminal, _terminal.DesktopAccessToken);
        await Task.WhenAll(usersTask, rolesTask, profilesTask, permissionsTask);

        var users = await usersTask;
        var roles = await rolesTask;
        var profiles = await profilesTask;
        var permissions = await permissionsTask;
        var failures = new[] { users.Detail, roles.Detail, profiles.Detail, permissions.Detail }
            .Where((_, index) => index switch
            {
                0 => !users.Success,
                1 => !roles.Success,
                2 => !profiles.Success,
                _ => !permissions.Success
            })
            .ToArray();
        if (failures.Length > 0 || users.Data is null || roles.Data is null || profiles.Data is null || permissions.Data is null)
        {
            Status = $"Carga IAM incompleta: {string.Join(" | ", failures)}";
            return;
        }

        Replace(Users, users.Data);
        Replace(AdministrativeRoles, roles.Data);
        Replace(Profiles, profiles.Data);
        Replace(Permissions, permissions.Data);
        Replace(ActivePermissions, permissions.Data.Where(item => item.Ativo));
        if (string.IsNullOrWhiteSpace(UserRole) || !AdministrativeRoles.Contains(UserRole))
        {
            UserRole = AdministrativeRoles.FirstOrDefault(item => item == "Vendedor") ?? AdministrativeRoles.FirstOrDefault() ?? string.Empty;
        }

        if (MatrixProfile is not null)
        {
            MatrixProfile = Profiles.FirstOrDefault(item => item.Id == MatrixProfile.Id);
            await LoadProfilePermissionsAsync();
        }

        Status = $"Carga confirmada: {Users.Count} usuários, {Profiles.Count} perfis e {Permissions.Count} permissões.";
    }

    private async void Reload_Click(object sender, RoutedEventArgs e) => await LoadAllAsync();

    private void NewUser_Click(object sender, RoutedEventArgs e)
    {
        SelectedUser = null;
        UserName = string.Empty;
        UserEmail = string.Empty;
        UserPhone = string.Empty;
        UserRole = AdministrativeRoles.FirstOrDefault(item => item == "Vendedor") ?? AdministrativeRoles.FirstOrDefault() ?? string.Empty;
        UserActive = true;
        UserPassword.Clear();
        Status = "Novo usuário: informe os dados e uma senha inicial com no mínimo oito caracteres.";
    }

    private async void SaveUser_Click(object sender, RoutedEventArgs e)
    {
        var password = UserPassword.Password;
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserEmail) || string.IsNullOrWhiteSpace(UserRole))
        {
            Status = "Nome, e-mail e perfil são obrigatórios.";
            return;
        }

        if (SelectedUser is null && password.Length < 8 || password.Length is > 0 and < 8)
        {
            Status = "A senha inicial ou nova senha deve ter pelo menos oito caracteres.";
            return;
        }

        Status = "Persistindo usuário e auditoria...";
        var result = await _apiClient.SaveAccessUserAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            new DesktopUsuarioAcessoRequest(
                UserName.Trim(),
                UserEmail.Trim(),
                UserRole,
                UserActive,
                NullIfWhiteSpace(UserPhone),
                string.IsNullOrWhiteSpace(password) ? null : password),
            SelectedUser?.Id);
        UserPassword.Clear();
        if (!result.Success || result.Data is null)
        {
            Status = $"Usuário não foi salvo: {result.Detail}";
            return;
        }

        var id = result.Data.Id;
        await LoadAllAsync();
        SelectedUser = Users.FirstOrDefault(item => item.Id == id);
        Status = $"Usuário {result.Data.Email} confirmado pela API e relido da base.";
    }

    private void NewProfile_Click(object sender, RoutedEventArgs e)
    {
        SelectedProfile = null;
        ProfileName = string.Empty;
        ProfileDescription = string.Empty;
        ProfileApprovalLimit = "0,00";
        ProfileHierarchyLevel = "1";
        ProfileActive = true;
        Status = "Novo perfil corporativo preparado para cadastro.";
    }

    private async void SaveProfile_Click(object sender, RoutedEventArgs e)
    {
        if (!TryReadProfileValues(out var approvalLimit, out var hierarchyLevel))
        {
            return;
        }

        var result = await _apiClient.SaveAccessProfileAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            new DesktopPerfilAcessoRequest(ProfileName.Trim(), NullIfWhiteSpace(ProfileDescription), approvalLimit, hierarchyLevel, ProfileActive),
            SelectedProfile?.Id);
        if (!result.Success || result.Data is null)
        {
            Status = $"Perfil não foi salvo: {result.Detail}";
            return;
        }

        var id = result.Data.Id;
        await LoadAllAsync();
        SelectedProfile = Profiles.FirstOrDefault(item => item.Id == id);
        Status = $"Perfil {result.Data.Nome} confirmado pela API e relido da base.";
    }

    private async void DeactivateProfile_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedProfile is null || MessageBox.Show(
                $"Desativar o perfil {SelectedProfile.Nome}?",
                "Confirmar desativação",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        var result = await _apiClient.DeactivateAccessProfileAsync(_terminal, _terminal.DesktopAccessToken, SelectedProfile.Id);
        if (!result.Success || result.Data is null)
        {
            Status = $"Perfil não foi desativado: {result.Detail}";
            return;
        }

        await LoadAllAsync();
        SelectedProfile = Profiles.FirstOrDefault(item => item.Id == result.Data.Id);
        Status = $"Perfil {result.Data.Nome} desativado e auditado.";
    }

    private void NewPermission_Click(object sender, RoutedEventArgs e)
    {
        SelectedPermission = null;
        PermissionModule = string.Empty;
        PermissionFunctionality = string.Empty;
        PermissionKey = string.Empty;
        PermissionDescription = string.Empty;
        PermissionActive = true;
        Status = "Nova permissão preparada para cadastro.";
    }

    private async void SavePermission_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PermissionModule) || string.IsNullOrWhiteSpace(PermissionFunctionality) || string.IsNullOrWhiteSpace(PermissionKey))
        {
            Status = "Módulo, funcionalidade e chave são obrigatórios.";
            return;
        }

        var result = await _apiClient.SavePermissionAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            new DesktopPermissaoAcessoRequest(
                PermissionModule.Trim(),
                PermissionFunctionality.Trim(),
                PermissionKey.Trim(),
                NullIfWhiteSpace(PermissionDescription),
                PermissionActive),
            SelectedPermission?.Id);
        if (!result.Success || result.Data is null)
        {
            Status = $"Permissão não foi salva: {result.Detail}";
            return;
        }

        var id = result.Data.Id;
        await LoadAllAsync();
        SelectedPermission = Permissions.FirstOrDefault(item => item.Id == id);
        Status = $"Permissão {result.Data.Chave} confirmada pela API e relida da base.";
    }

    private async void DeactivatePermission_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPermission is null || MessageBox.Show(
                $"Desativar a permissão {SelectedPermission.Chave}?",
                "Confirmar desativação",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        var result = await _apiClient.DeactivatePermissionAsync(_terminal, _terminal.DesktopAccessToken, SelectedPermission.Id);
        if (!result.Success || result.Data is null)
        {
            Status = $"Permissão não foi desativada: {result.Detail}";
            return;
        }

        await LoadAllAsync();
        SelectedPermission = Permissions.FirstOrDefault(item => item.Id == result.Data.Id);
        Status = $"Permissão {result.Data.Chave} desativada e auditada.";
    }

    private async void MatrixProfileSelectionChanged(object sender, SelectionChangedEventArgs e) => await LoadProfilePermissionsAsync();

    private async Task LoadProfilePermissionsAsync()
    {
        ProfilePermissions.Clear();
        SelectedProfilePermission = null;
        if (MatrixProfile is null || !IsAuthenticated)
        {
            return;
        }

        var result = await _apiClient.GetProfilePermissionsAsync(_terminal, _terminal.DesktopAccessToken, MatrixProfile.Id);
        if (!result.Success || result.Data is null)
        {
            Status = $"Matriz do perfil não foi carregada: {result.Detail}";
            return;
        }

        Replace(ProfilePermissions, result.Data);
        Status = $"Matriz de {MatrixProfile.Nome} carregada com {ProfilePermissions.Count} autorizações.";
    }

    private async void SaveProfilePermission_Click(object sender, RoutedEventArgs e)
    {
        if (MatrixProfile is null || MatrixPermission is null)
        {
            Status = "Selecione um perfil e uma permissão ativa.";
            return;
        }

        var result = await _apiClient.SaveProfilePermissionAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            MatrixProfile.Id,
            new DesktopPerfilPermissaoRequest(MatrixPermission.Id, MatrixRead, MatrixWrite, MatrixDelete, MatrixPrint));
        if (!result.Success || result.Data is null)
        {
            Status = $"Autorização não foi aplicada: {result.Detail}";
            return;
        }

        await LoadProfilePermissionsAsync();
        SelectedProfilePermission = ProfilePermissions.FirstOrDefault(item => item.PermissaoId == result.Data.PermissaoId);
        Status = $"Autorização {result.Data.Chave} aplicada ao perfil {MatrixProfile.Nome} e auditada.";
    }

    private async void RemoveProfilePermission_Click(object sender, RoutedEventArgs e)
    {
        if (MatrixProfile is null || SelectedProfilePermission is null || MessageBox.Show(
                $"Remover {SelectedProfilePermission.Chave} do perfil {MatrixProfile.Nome}?",
                "Confirmar remoção",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        var result = await _apiClient.RemoveProfilePermissionAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            MatrixProfile.Id,
            SelectedProfilePermission.PermissaoId);
        if (!result.Success)
        {
            Status = $"Vínculo não foi removido: {result.Detail}";
            return;
        }

        await LoadProfilePermissionsAsync();
        Status = "Vínculo removido da matriz e auditoria confirmada pela API.";
    }

    private bool TryReadProfileValues(out decimal approvalLimit, out int hierarchyLevel)
    {
        approvalLimit = 0;
        hierarchyLevel = 0;
        if (string.IsNullOrWhiteSpace(ProfileName))
        {
            Status = "O nome do perfil é obrigatório.";
            return false;
        }

        var parsedApproval = decimal.TryParse(ProfileApprovalLimit, NumberStyles.Number, CultureInfo.CurrentCulture, out approvalLimit)
            || decimal.TryParse(ProfileApprovalLimit, NumberStyles.Number, CultureInfo.InvariantCulture, out approvalLimit);
        if (!parsedApproval || approvalLimit < 0)
        {
            Status = "A alçada máxima deve ser um valor monetário válido e não negativo.";
            return false;
        }

        if (!int.TryParse(ProfileHierarchyLevel, NumberStyles.Integer, CultureInfo.InvariantCulture, out hierarchyLevel) || hierarchyLevel < 0)
        {
            Status = "O nível hierárquico deve ser um número inteiro não negativo.";
            return false;
        }

        return true;
    }

    private void ClearSession()
    {
        _terminal.DesktopAccessToken = string.Empty;
        IsAuthenticated = false;
        SessionLabel = "Sessão administrativa ausente";
        EndpointLabel = "API oficial não autenticada";
        Users.Clear();
        AdministrativeRoles.Clear();
        Profiles.Clear();
        Permissions.Clear();
        ActivePermissions.Clear();
        ProfilePermissions.Clear();
    }

    private static string? NullIfWhiteSpace(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> values)
    {
        target.Clear();
        foreach (var value in values)
        {
            target.Add(value);
        }
    }

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
