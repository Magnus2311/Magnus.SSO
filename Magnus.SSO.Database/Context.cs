using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Magnus.SSO.Database.Models;

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

            InitIndexes();
        }

        public void InitIndexes()
        {
            var indexModel = new CreateIndexModel<Callback>(
            keys: Builders<Callback>.IndexKeys.Ascending(nameof(Callback.ExpireAt)),
            options: new CreateIndexOptions
            {
                ExpireAfter = TimeSpan.FromDays(1),
                Name = "ExpireAtIndex"
            });

            _db.GetCollection<Callback>(typeof(Callback).Name).Indexes.CreateOne(indexModel);
        }
    }
}