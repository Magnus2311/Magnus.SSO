namespace Magnus.SSO.Database.Models
{
    public record Login
    {
        public string URL { get; init; } = string.Empty;
        public DateTime Date { get; init; } = DateTime.Now;
        public string IP { get; init; } = string.Empty;
    }
}
