using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace Magnus.SSO.Database
{
    public class Context : DbContext
    {
        protected readonly MongoClient _client;
        protected readonly IMongoDatabase _db;
        private readonly IConfiguration _configuration;

        public Context(IConfiguration configuration)
        {
            _configuration = configuration;

            _client = new MongoClient($"mongodb+srv://{_configuration["DB:SSO:Username"]}:{_configuration["DB:SSO:Password"]}{_configuration["DB:SSO:Cluster"]}.rdkdn.mongodb.net/{_configuration["DB:SSO:Collection"]}?retryWrites=true&w=majority");

            _db = _client.GetDatabase(_configuration["DB:SSO:Collection"]);
        }
    }
}