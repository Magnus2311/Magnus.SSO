using Magnus.SSO.Database.Models;
using Magnus.SSO.Database.Repositories;

namespace Magnus.SSO.Services
{
    public class UrlsService
    {
        private readonly UrlsRepository _urlsRepository;

        public UrlsService(UrlsRepository urlsRepository)
        {
            _urlsRepository = urlsRepository;
        }

        public async Task Add(Callback callback)
            => await _urlsRepository.Add(callback);

        public async Task<string?> GetCallbackByToken(string token)
            => await _urlsRepository.FindCallbackByToken(token);
    }
}