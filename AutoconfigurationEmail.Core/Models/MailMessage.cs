using System.Net.Mail;

namespace AutoconfigurationEmail.Core
{
    public class MailMessage
    {
        public MailMessage()
        {
        }
        public MailAddress ToAddress { get; set; }
        public MailAddress FromAddress { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
