using MahApps.Metro.SimpleChildWindow;
using PokemonBag.Common;
using PokemonBag.Logic;
using PokemonBag.State;
using PokemonGo.RocketAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PokemonBag.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : ChildWindow
    {
        private RadioButton AuthTypeRadioButton;
        private bool isAutoLogin;
        private string userName;
        private string password;
        private Session session;

        public LoginWindow()
        {
            this.InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
           isAutoLogin = IsAutoLogin.IsChecked.Value;
           userName = UserName.Text;
           password = Password.Password;
           session =  SessionManager.Instance().Session;
           if (AuthTypeRadioButton.Content.ToString() == AuthType.Google.ToString())
           {
                session.Settings.AuthType = AuthType.Google;
                session.Settings.GoogleUsername = UserName.Text;
                session.Settings.GooglePassword = Password.Password;

           }
           else if (AuthTypeRadioButton.Content.ToString() == AuthType.Ptc.ToString())
           {
                session.Settings.AuthType = AuthType.Ptc;
                session.Settings.PtcUsername = UserName.Text;
                session.Settings.PtcPassword = Password.Password;
           }
           ((ClientSettings)SessionManager.Instance().Session.Settings).SaveSetting();
           SessionManager.Instance().Session.Reset(SessionManager.Instance().Session.Settings);
          // SessionManager.Instance().Session.Client.ApiFailure = new ApiFailureStrategy(SessionManager.Instance().Session);

           PokemonAPILogic pokemonAPILogic = new PokemonAPILogic();
           IResult loginResult = await pokemonAPILogic.Login(SessionManager.Instance().Session);
           if (loginResult.IsSuccess == false)
           {
                LoginErr.Text = loginResult.Message;
            }
            else
            {
                this.Close();
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AuthType_Checked(object sender, RoutedEventArgs e)
        {
            AuthTypeRadioButton = sender as RadioButton;
        }
    }
}
