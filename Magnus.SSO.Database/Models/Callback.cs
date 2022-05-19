using MongoDB.Bson;

namespace Magnus.SSO.Database.Models
{
    public class Callback
    {
        public ObjectId Id { get; set; }
        public string? Token { get; set; }
        public string? CallbackUrl { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ExpireAt { get; set; } = DateTime.Now.AddDays(1);
    }
}