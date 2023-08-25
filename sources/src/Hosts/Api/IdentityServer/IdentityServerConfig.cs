namespace auth_service.IdentityServer
{
    public class IdentityServerConfig
    {
        /// <summary>
        /// Подтвержденные клиенты.
        /// </summary>
        public Dictionary<string, string> KnownClients { get; set; }
    }
}
