using POGOProtos.Networking.Responses;
using PokemonBag.Common;
using PokemonBag.Logic;
using PokemonGo.RocketAPI;

namespace PokemonBag.State
{
    public interface ISession
    {
        ISettings Settings { get; }
        Inventory Inventory { get; }
        Client Client { get; }
        GetPlayerResponse Profile { get; set; }
    }

    public class Session : ISession
    {
        public ISettings Settings { get; }

        public Inventory Inventory { get; private set; }

        public Client Client { get; private set; }

        public GetPlayerResponse Profile { get; set; }

        public ApiFailureStrategy ApiFailureStrategy { get; set; }
    
        public Session(ISettings settings)
        {
            Settings = settings;
            ApiFailureStrategy = new ApiFailureStrategy(this);
            Reset(settings);
        }

        public void Reset(ISettings settings)
        {
            Client = new Client(Settings, ApiFailureStrategy);
            // ferox wants us to set this manually
            Inventory = new Inventory(Client);
        }
    }

    public class SessionManager
    {
        private static SessionManager instance;

        protected SessionManager()
        {
        }

        public static SessionManager Instance()
        {
            if (instance == null)
            {
                instance = new SessionManager();
            }

            return instance;
        }

        public Session Session { get; set; }
    }

}
