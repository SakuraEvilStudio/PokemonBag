using PokemonBag.ViewModels;
using System.Windows;

namespace PokemonBag
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void AppStartup(object sender, StartupEventArgs args)
        {
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            mainWindow.Show();
        }
    }
}
