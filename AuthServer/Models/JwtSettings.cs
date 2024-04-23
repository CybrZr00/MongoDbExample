namespace AuthServer.Models
{
    public class JwtSettings
    {
        public string EncrytionKey { get; set; }
        public string SigningKey { get; set; }
        public TimeSpan TokenLifetime { get; set; }
    }
}
