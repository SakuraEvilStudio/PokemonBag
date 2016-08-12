

using POGOProtos.Networking.Responses;
using PokemonBag.Common;
using PokemonBag.Logic;
using PokemonBag.State;
using PokemonBag.Views;
using PokemonGo.RocketAPI.Enums;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace PokemonBag
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private LoginWindow LoginWindow;
        private Inventory Inventory;

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
                LoginWindow = new LoginWindow() { AllowMove = false };
                await MahApps.Metro.SimpleChildWindow.ChildWindowManager.ShowChildWindowAsync(this, LoginWindow , RootGrid);
                LoginWindow.ClosingFinished += LoginWindow_ClosingFinished; ;
            }
            else {
                //SessionManager.Instance().Session.Client.ApiFailure = new ApiFailureStrategy(SessionManager.Instance().Session);
            }
        }

        private void LoginWindow_ClosingFinished(object sender, RoutedEventArgs e)
        {
            Inventory = new Inventory(SessionManager.Instance().Session);
            var PlayerInfo = String.Format("[Player: {0}] [LV {1}] [Team {2}]",
                new object[3] { Inventory.GetPlayerName(), Inventory.GetPlayerLV(), Inventory.GetTeam()});
            PlayerName.Text = PlayerInfo;
        }
    }
}
