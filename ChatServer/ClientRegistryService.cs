using StackExchange.Redis;
using System.Threading.Tasks;

namespace ChatServer
{
    public interface IClientRegistryService
    {
        void Register(string serverIp, string clientIp);

        Task<string> Get(string clientIp);
    }

    public class ClientRegistryService : IClientRegistryService
    {
        private readonly IDatabase _db;

        public ClientRegistryService(IDatabase db) => _db = db;

        public void Register(string serverIp, string clientIp) => _db.StringSet(clientIp, serverIp, flags: CommandFlags.FireAndForget);

        public async Task<string> Get(string clientIp) => await _db.StringGetAsync(clientIp);
    }
}