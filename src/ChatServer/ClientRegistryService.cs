using StackExchange.Redis;
using System.Threading.Tasks;

namespace ChatServer
{
    public interface IClientRegistryService
    {
        void FireRegister(string serverIp, string userName);

        Task<string> Get(string userName);
    }

    public class ClientRegistryService : IClientRegistryService
    {
        private readonly IDatabase _db;

        public ClientRegistryService(IDatabase db) => _db = db;

        public void FireRegister(string serverIp, string userName) => _db.StringSet(userName, serverIp, flags: CommandFlags.FireAndForget);

        public async Task<string> Get(string userName) => await _db.StringGetAsync(userName);
    }
}