using Magnus.SSO.Database.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Magnus.SSO.Database.Repositories
{
    public class UsersRepository : Context
    {
        protected readonly IMongoCollection<User> _collection;

        public UsersRepository(IConfiguration configuration) : base(configuration)
        {
            _collection = _db.GetCollection<User>(typeof(User).Name);
        }

        public async Task Add(User entity)
            => await _collection.InsertOneAsync(entity);

        public async Task Delete(ObjectId id)
        {
            var entity = await (await _collection.FindAsync(e => e.Id == id)).FirstOrDefaultAsync();
            entity.Version = ++entity.Version;
            entity.IsDeleted = true;
            await _collection.ReplaceOneAsync(Builders<User>.Filter.Eq(e => e.Id, entity.Id), entity);
        }

        public async Task<User> Get(ObjectId id)
            => await (await _collection.FindAsync(e => e.Id == id)).FirstOrDefaultAsync();

        public async Task<User> GetByUsername(string username)
            => await (await _collection.FindAsync(e => e.Username.ToUpperInvariant() == username.ToUpperInvariant())).FirstOrDefaultAsync();

        public async Task<IEnumerable<User>> GetAll()
            => await (await _collection.FindAsync(Builders<User>.Filter.Empty)).ToListAsync();

        public async Task Update(User entity)
        {
            var oldEntity = await (await _collection.FindAsync(e => e.Id == entity.Id)).FirstOrDefaultAsync();
            entity.Version = ++oldEntity.Version;

            await _collection.ReplaceOneAsync(Builders<User>.Filter.Eq(e => e.Id, entity.Id), entity);
        }

        public async Task<User> GetByEmail(string email)
            => await (await _collection.FindAsync(e => e.Email.ToUpperInvariant() == email.ToUpperInvariant())).FirstOrDefaultAsync();
    }
}
