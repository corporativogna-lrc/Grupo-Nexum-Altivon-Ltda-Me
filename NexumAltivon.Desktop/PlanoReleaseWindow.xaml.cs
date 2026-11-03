using System.Windows;
using System.Windows.Controls;
namespace NexumAltivon.Desktop
{
    public partial class PlanoReleaseWindow : Window
    {
        public PlanoReleaseWindow()
        {
            InitializeComponent();
            _ = CarregarAsync();
        }
        private async System.Threading.Tasks.Task CarregarAsync()
        {
            await System.Threading.Tasks.Task.CompletedTask;
        }
        private void BuscaChanged(object sender, TextChangedEventArgs e) { }
        private void Resultados_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void Salvar_Click(object sender, RoutedEventArgs e) { }
        private void Excluir_Click(object sender, RoutedEventArgs e) { }
    }
}
