namespace ChatServer
{
    public interface IClientRegistryService
    {
        void Register(string serverIp, string clientIp);
    }

    public class ClientRegistryService : IClientRegistryService
    {
        public void Register(string serverIp, string clientIp)
        { }
    }
}