using System.Collections.ObjectModel;

namespace NexumAltivon.Desktop.Models;

public sealed class OrganizationNode
{
    public string Name { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string TaxSituation { get; init; } = string.Empty;
    public string EmailPattern { get; init; } = string.Empty;
    public ObservableCollection<OrganizationNode> Children { get; } = new();
}
