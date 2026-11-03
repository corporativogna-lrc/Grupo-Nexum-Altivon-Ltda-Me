/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Sistema de gestão: GenesisGest.Net (Grupo Nexum Altivon)
 * Módulo: Infraestrutura Base de ViewModels (WPF MVVM)
 * Versão: 1.1.5.7226 | Data: 16/08/2026
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NexumAltivon.Desktop.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}