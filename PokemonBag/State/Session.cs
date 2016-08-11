using POGOProtos.Networking.Responses;
using PokemonBag.Common;
using PokemonGo.RocketAPI;

namespace PokemonBag.State
{
    public interface ISession
    {
        ISettings Settings { get; }
        //Inventory Inventory { get; }
        Client Client { get; }
        GetPlayerResponse Profile { get; set; }
    }

    public class Session : ISession
    {
        public ISettings Settings { get; }
        public Client Client { get; private set; }
        public GetPlayerResponse Profile { get; set; }

        public Session(ISettings settings)
        {
            Settings = settings;
            Reset(settings);
        }

        public void Reset(ISettings settings)
        {
            ApiFailureStrategy _apiStrategy = new ApiFailureStrategy(this);
            Client = new Client(Settings, _apiStrategy);
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
