namespace auth_service.IdentityServer
{
    public class IdentityServerConfig
    {
        /// <summary>
        /// Идентификатор клиента, который будет подключаться к серверу.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Секрет клиента, который будет подключаться к серверу.
        /// </summary>
        public string ClientSecret { get; set;}
    }
}
