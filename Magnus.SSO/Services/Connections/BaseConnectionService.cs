namespace Magnus.SSO.Services.Connections
{
    public abstract class BaseConnectionService
    {
        protected readonly HttpClient _httpClient;

        public BaseConnectionService()
        {
            _httpClient = new HttpClient();
        }
    }
}