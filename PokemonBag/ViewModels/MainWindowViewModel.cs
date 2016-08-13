using PropertyChanged;
using System.Windows.Input;
using System.Windows;
using System;
using PokemonBag.State;
using PokemonBag.Logic;
using System.Windows.Controls;
using System.Collections.Generic;
using POGOProtos.Data;

namespace PokemonBag.ViewModels
{
    [ImplementPropertyChanged]
    class MainWindowViewModel
    {
        public IEnumerable<PokemonData> Pokemons { get; set; }

        public Inventory Inventory { get; set; }

        public int TransitionerIndex { get; set; }
        public ICommand PokemonComand { get; }
        public ICommand SettingComand { get; }
        public ICommand RefreshCommand { get; }
    //    public IEnumerable<PokemonData> Pokemons { get; set; }

    public MainWindowViewModel()
        {
            Instance = this;
            PokemonComand = new ActionCommand(ShowPokemon);
            SettingComand = new ActionCommand(ShowSettings);
            RefreshCommand = new ActionCommand(Refresh);
        }

        private async void Refresh(object parameter)
        {
            Inventory = SessionManager.Instance().Session.Inventory;
            Pokemons = await Inventory.GetPokemons();
        }

        public void ShowPokemon()
        {
            if (TransitionerIndex != 0)
            {
                TransitionerIndex = 0;
                return;
            }
            TransitionerIndex = 0;
        }

        public void ShowSettings()
        {
            if (TransitionerIndex != 0)
            {
                TransitionerIndex = 0;
                return;
            }
            TransitionerIndex = 1;
        }

        public static MainWindowViewModel Instance { get; private set; }
    }

    public class BindingProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object),
                typeof(BindingProxy));
    }
}
