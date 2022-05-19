using Magnus.SSO.Database.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Magnus.SSO.Database.Repositories
{
    public class UrlsRepository : Context
    {
        protected readonly IMongoCollection<Callback> _collection;

        public UrlsRepository(IConfiguration configuration) : base(configuration)
        {
            _collection = _db.GetCollection<Callback>(typeof(Callback).Name);
        }

        public async Task Add(Callback callback)
            => await _collection.InsertOneAsync(callback);

        public async Task<string?> FindCallbackByToken(string token)
            => (await _collection.FindAsync(c => c.Token == token))?.FirstOrDefault()?.CallbackUrl;
    }
}