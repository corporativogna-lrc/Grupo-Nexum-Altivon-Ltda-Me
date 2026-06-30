/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NexumAltivon.Desktop.Models;

public sealed class PdvCartItem : INotifyPropertyChanged
{
    private string _codigo = string.Empty;
    private string _descricao = string.Empty;
    private string _empresaDestino = "NEXUM";
    private string _origemAquisicao = "ECOM";
    private decimal _quantidade = 1m;
    private decimal _valorUnitario;
    private decimal _desconto;
    private decimal _custoEstimado;

    public string Codigo
    {
        get => _codigo;
        set => SetField(ref _codigo, value);
    }

    public string Descricao
    {
        get => _descricao;
        set => SetField(ref _descricao, value);
    }

    public string EmpresaDestino
    {
        get => _empresaDestino;
        set => SetField(ref _empresaDestino, value);
    }

    public string OrigemAquisicao
    {
        get => _origemAquisicao;
        set => SetField(ref _origemAquisicao, value);
    }

    public decimal Quantidade
    {
        get => _quantidade;
        set
        {
            SetField(ref _quantidade, value);
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(MargemEstimada));
        }
    }

    public decimal ValorUnitario
    {
        get => _valorUnitario;
        set
        {
            SetField(ref _valorUnitario, value);
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(MargemEstimada));
        }
    }

    public decimal Desconto
    {
        get => _desconto;
        set
        {
            SetField(ref _desconto, value);
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(MargemEstimada));
        }
    }

    public decimal CustoEstimado
    {
        get => _custoEstimado;
        set
        {
            SetField(ref _custoEstimado, value);
            OnPropertyChanged(nameof(MargemEstimada));
        }
    }

    public decimal Total => Math.Max(0m, Quantidade * ValorUnitario - Desconto);

    public decimal MargemEstimada => Total - Quantidade * CustoEstimado;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
