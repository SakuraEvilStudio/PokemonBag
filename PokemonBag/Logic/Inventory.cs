using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using PokemonBag.State;
using PokemonGo.RocketAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonBag.Logic
{
    public class Inventory
    {
        private readonly Client _client;
        private GetInventoryResponse _cachedInventory;
        private DateTime _lastRefresh;

        public Inventory(Client client)
        {
            _client = client;
        }

        private async Task<GetInventoryResponse> GetCachedInventory()
        {
            var now = DateTime.UtcNow;

            if (_lastRefresh.AddSeconds(30).Ticks > now.Ticks)
            {
                return _cachedInventory;
            }
            return await RefreshCachedInventory();
        }

        public async Task<GetInventoryResponse> RefreshCachedInventory()
        {
            var now = DateTime.UtcNow;
            var ss = new SemaphoreSlim(10);

            await ss.WaitAsync();
            try
            {
                _lastRefresh = now;
                _cachedInventory = await _client.Inventory.GetInventory();
                return _cachedInventory;
            }
            finally
            {
                ss.Release();
            }
        }

        public async Task<IEnumerable<PlayerStats>> GetPlayerStats()
        {
            var inventory = await GetCachedInventory();
            return inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.PlayerStats)
                .Where(p => p != null);
        }

        public async Task<IEnumerable<PokemonData>> GetEggs()
        {
            var inventory = await GetCachedInventory();
            return
                inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData)
                    .Where(p => p != null && p.IsEgg);
        }

        public async Task<IEnumerable<PokemonData>> GetPokemons()
        {
            var inventory = await GetCachedInventory();
            return
                inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData)
                    .Where(p => p != null && p.PokemonId > 0);
        }


        //public int GetStarDust()
        //{
        //    if (_player == null) GetPlayerData();
        //    return _player.PlayerData.Currencies[1].Amount;
        //}

        //public async void GetPlayerData()
        //{
        //    _player = await _client.Player.GetPlayer();
        //}

        //public string GetPlayerName()
        //{
        //    if (_player == null) GetPlayerData();
        //    return _player.PlayerData.Username;
        //}

        //public string GetTeam()
        //{
        //    if (_player == null) GetPlayerData();
        //    return _player.PlayerData.Team.ToString();
        //}

        //public int GetPlayerLV()
        //{
        //    var stats = GetPlayerStats().Result.FirstOrDefault();
        //    return stats.Level;
        //}

        //public async Task<IEnumerable<PokemonData>> GetPokemons()
        //{
        //    var inventory = await GetCachedInventory();
        //    return
        //        inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData)
        //            .Where(p => p != null && p.PokemonId > 0);
        //}

        //private async Task<GetInventoryResponse> GetCachedInventory()
        //{
        //    if (_player == null) GetPlayerData();
        //    var now = DateTime.UtcNow;

        //    if (_cachedInventory != null && _lastRefresh.AddSeconds(30).Ticks > now.Ticks)
        //        return _cachedInventory;

        //    return await RefreshCachedInventory();
        //}

        //public async Task<GetInventoryResponse> RefreshCachedInventory()
        //{
        //    var now = DateTime.UtcNow;
        //    var ss = new SemaphoreSlim(10);

        //    await ss.WaitAsync();
        //    try
        //    {
        //        _lastRefresh = now;
        //        _cachedInventory = await _client.Inventory.GetInventory();
        //        return _cachedInventory;
        //    }
        //    finally
        //    {
        //        ss.Release();
        //    }
        //}

        //public async Task<IEnumerable<PlayerStats>> GetPlayerStats()
        //{
        //    var inventory = await GetCachedInventory();
        //    return inventory.InventoryDelta.InventoryItems
        //        .Select(i => i.InventoryItemData?.PlayerStats)
        //        .Where(p => p != null);
        //}
    }
}
