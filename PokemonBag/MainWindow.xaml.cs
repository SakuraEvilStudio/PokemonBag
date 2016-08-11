

using PokemonBag.Common;
using PokemonBag.Logic;
using PokemonBag.State;
using PokemonBag.Views;
using PokemonGo.RocketAPI.Enums;
using System;
using System.IO;
using System.Windows;

namespace PokemonBag
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var profileConfigPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config");
            var configFile = Path.Combine(profileConfigPath, "config.json");

            ApplicationSettings settings = ApplicationSettings.Load();

            SessionManager.Instance().Session = new Session(new ClientSettings(settings));


            if (settings.IsAutoLogin == false || (settings.AuthType != AuthType.Google && settings.AuthType != AuthType.Ptc))
            {
                await MahApps.Metro.SimpleChildWindow.ChildWindowManager.ShowChildWindowAsync(this, new LoginWindow() { AllowMove = false }, RootGrid);
            }else {
                SessionManager.Instance().Session.Client.ApiFailure = new ApiFailureStrategy(SessionManager.Instance().Session);
            }
        }
    }
}
