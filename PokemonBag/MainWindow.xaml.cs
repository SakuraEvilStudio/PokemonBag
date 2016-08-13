using POGOProtos.Data.Player;
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
using System.Linq;
using System.Windows.Controls;

namespace PokemonBag
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Inventory Inventory;
        private GetPlayerResponse Profile;
        private Session Session;
        private RadioButton AuthTypeRadioButton;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationSettings settings = ApplicationSettings.Load();
            SessionManager.Instance().Session = new Session(new ClientSettings(settings));
            LoginForm.IsOpen = true;
        }

        private void LoginWindow_ClosingFinished(object sender, RoutedEventArgs e)
        {
            Inventory = SessionManager.Instance().Session.Inventory;
            Profile = SessionManager.Instance().Session.Profile;

            var PlayerInfo = String.Format("Player: {0} - Team {1} (StarDust {2})",
                new object[3] { Profile.PlayerData.Username, Profile.PlayerData.Team, Profile.PlayerData.Currencies[1].Amount });
            PlayerName.Text = PlayerInfo;
        }

        private void LoginForm_Loaded(object sender, RoutedEventArgs e)
        {
            Session = SessionManager.Instance().Session;
            if (Session.Settings.AuthType == AuthType.Google)
            {
                GoogleRadio.IsChecked = true;
                UserName.Text = Session.Settings.GoogleUsername;
                Password.Password = Session.Settings.GooglePassword;
            }
            else if (Session.Settings.AuthType == AuthType.Ptc)
            {
                PtcRadio.IsChecked = true;
                UserName.Text = Session.Settings.PtcUsername;
                Password.Password = Session.Settings.PtcPassword;
            }
        }

        private void AuthType_Checked(object sender, RoutedEventArgs e)
        {
            AuthTypeRadioButton = sender as RadioButton;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (AuthTypeRadioButton.Content.ToString() == AuthType.Google.ToString())
            {
                Session.Settings.AuthType = AuthType.Google;
                Session.Settings.GoogleUsername = UserName.Text;
                Session.Settings.GooglePassword = Password.Password;

            }
            else if (AuthTypeRadioButton.Content.ToString() == AuthType.Ptc.ToString())
            {
                Session.Settings.AuthType = AuthType.Ptc;
                Session.Settings.PtcUsername = UserName.Text;
                Session.Settings.PtcPassword = Password.Password;
            }
           ((ClientSettings)SessionManager.Instance().Session.Settings).SaveSetting();
            SessionManager.Instance().Session.Reset(SessionManager.Instance().Session.Settings);

            PokemonAPILogic pokemonAPILogic = new PokemonAPILogic();
            IResult loginResult = await pokemonAPILogic.Login(SessionManager.Instance().Session);
            if (loginResult.IsSuccess == false)
            {
                LoginErr.Text = loginResult.Message;
            }
            else
            {
                LoginForm.Close();
            }

        }

    }
}
