namespace IdentityServer.Models
{
    public class ProviderConfig
    {
        public string DisplayName { get; set; }
        public string Authority { get; set; }
        public string MetadataAddress { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Prompt { get; set; }
        public string Enabled { get; set; }
    }
}
