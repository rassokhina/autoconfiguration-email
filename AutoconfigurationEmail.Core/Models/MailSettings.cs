using AutoconfigurationEmail.Core.Properties;

namespace AutoconfigurationEmail.Core
{
    public sealed class MailSettings 
    {
        public MailSettings()
        {
            Server = Resource.Server;
            Port = int.Parse(Resource.Port ?? "25");
            Username = Resource.Username;
            Password = Resource.Password;
            OverSsl = bool.Parse(Resource.OverSsl ?? "false");
            FromEmail = Resource.FromEmail;
        }

        public string Server { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool OverSsl { get; set; }

        public string FromEmail { get; set; }
    }
}
