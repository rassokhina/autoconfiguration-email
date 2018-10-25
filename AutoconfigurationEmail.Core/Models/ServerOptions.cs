using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AutoconfigurationEmail.Core
{
    public class ServerOptions
    {
        public ServerOptions()
        {
        }

        public ServerOptions(Stream stream)
        {
            var emailProvider = XDocument.Load(stream)
                .Descendants()
                .First(x => x.Name.LocalName == "emailProvider");

            var server = emailProvider.Elements().Where(e => (e.Attribute("type").Value == "smtp") 
                && (GetElementValue(e, "socketType") == "SSL")).FirstOrDefault();
            if(server != null)
            {
                var outgoingServer = ParseServerOptions(server);
                SmtpServer = outgoingServer.server;
                SmtpPort = outgoingServer.port;
                SmtpSocketType = SocketType.SSL;
                SmtpUserName = outgoingServer.userName;
                SmtpPassType = outgoingServer.passType;
            }

            server = emailProvider.Elements().Where(e => (e.Attribute("type").Value == "imap")
               && (GetElementValue(e, "socketType") == "SSL")).FirstOrDefault();
            if (server != null)
            {
                var incomingServer = ParseServerOptions(server);
                ImapServer = incomingServer.server;
                ImapPort = incomingServer.port;
                ImapSocketType = SocketType.SSL;
                ImapUserName = incomingServer.userName;
                ImapPassType = incomingServer.passType;
            }
        }

        public string ImapServer { get; set; }

        public int ImapPort { get; set; }

        public SocketType ImapSocketType { get; set; }

        public UsernameType ImapUserName { get; set; }

        public PasswordType ImapPassType { get; set; }

        public string SmtpServer { get; set; }

        public int SmtpPort { get; set; }

        public SocketType SmtpSocketType { get; set; }

        public UsernameType SmtpUserName { get; set; }

        public PasswordType SmtpPassType { get; set; }

        private static (string server, int port, UsernameType userName, PasswordType passType) ParseServerOptions(XElement server)
        {
                return
                       (GetElementValue(server, "hostname"),
                           int.Parse(GetElementValue(server, "port")),
                           GetElementValue(server, "username") == "%EMAILADDRESS%"
                               ? UsernameType.Address
                               : UsernameType.LocalPart,
                           GetElementValue(server, "authentication") == "password-cleartext"
                               ? PasswordType.ClearText
                               : (GetElementValue(server, "authentication") == "password-encrypted"
                                   ? PasswordType.CRAMMD5
                                   : PasswordType.Other)
                       );
        }

        private static string GetElementValue(XElement node, string name)
        {
            return node.Element(node.Name.Namespace + name)?.Value;
        }
    }
}