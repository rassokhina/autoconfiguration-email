using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoconfigurationEmail.Core.Properties;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using DnsClient;
using DnsClient.Protocol;

namespace AutoconfigurationEmail.Core
{
    public static class IspdbClient
    {
        private static readonly int[] smtpPorts = { 465, 587, 25 };
        private static readonly int[] imapPorts = { 993, 143 };

        public static async Task<ServerOptions> FindServerOptionsAsync(string email)
        {
            var result = new ServerOptions();
            using (var emailDomainResponse = await GetIspbdResponse(Resource.Ispdb_Url + GetDomain(email)))
            {
                if (emailDomainResponse.IsSuccessStatusCode)
                {
                    return new ServerOptions(emailDomainResponse.Content.ReadAsStreamAsync().Result);
                }
            }
          var mxRecord = await GetMxRecord(GetDomain(email));
            if (string.IsNullOrWhiteSpace(mxRecord?.DomainName.Value))
            {
                return result;
            }
            var mxDomain = ("" + mxRecord.DomainName.Value).TrimEnd('.');
            using (var mxDomainResponse = await GetIspbdResponse(Resource.Ispdb_Url + mxDomain))
            {
                if (mxDomainResponse.IsSuccessStatusCode)
                {
                    return new ServerOptions(mxDomainResponse.Content.ReadAsStreamAsync().Result);
                }
            }
            var mxExchange = ("" + mxRecord.Exchange.Value).TrimEnd('.');
            await TryDetermineSmtpOptions(mxExchange, result);
            await TryDetermineImapOptions(mxExchange, result);
            return result;
        }

        private static async Task TryDetermineSmtpOptions(string host, ServerOptions serverOptions)
        {
            using (var client = new SmtpClient())
            {
                client.Timeout = 15000;
                foreach (int port in smtpPorts)
                {

                    try
                    {
                        await client.ConnectAsync(host, port, SecureSocketOptions.Auto);
                        await client.DisconnectAsync(true);
                        serverOptions.SmtpServer = host;
                        serverOptions.SmtpPort = port;
                        return;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static async Task TryDetermineImapOptions(string host, ServerOptions serverOptions)
        {
            using (var client = new ImapClient())
            {
                client.Timeout = 15000;
                foreach (var port in imapPorts)
                {

                    try
                    {
                        await client.ConnectAsync(host, port, SecureSocketOptions.Auto);
                        await client.DisconnectAsync(true);
                        serverOptions.ImapServer = host;
                        serverOptions.ImapPort = port;
                        return;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public static async Task<List<string>> TestServerConnections(TestConnectionSettings emailSettings)
        {
            var smtpErrors = await TestSmtpConnection(emailSettings);
            var imapErrors = await TestImapConnection(emailSettings);
            if (smtpErrors.Count() > 0)
            {
                smtpErrors.Insert(0, Resource.Connection_SmtpFailed);
            }
            if (imapErrors.Count() > 0)
            {
                imapErrors.Insert(0, Resource.Connection_ImapFailed);
            }
            return smtpErrors.Concat(imapErrors).ToList();
        }

        private static async Task<List<string>> TestSmtpConnection(TestConnectionSettings emailSettings)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(emailSettings.Email));
            emailMessage.To.Add(new MailboxAddress(new MailSettings().FromEmail));
            emailMessage.Subject = "Test Email";
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = string.Empty
            };
            var errors = new List<string>();
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(emailSettings.SmtpServer, emailSettings.SmtpPort ?? 0);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(emailSettings.Username, emailSettings.Password);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);

                    return errors;
                }
                catch (AuthenticationException e)
                {
                    errors.Add(e.Message);
                    errors.Add(Resource.Connection_CheckUsernamePassword);
                    return errors;
                }
                catch (ServiceNotConnectedException e)
                {
                    errors.Add(e.Message);
                    errors.Add(Resource.Connection_ContactAdmin);
                    return errors;
                }
                catch (ServiceNotAuthenticatedException e)
                {
                    errors.Add(e.Message);
                    errors.Add(Resource.Connection_ContactAdmin);
                    return errors;
                }
                catch (Exception)
                {
                    errors.Add(Resource.Connection_ContactAdmin);
                    return errors;
                }
            }
        }

        private static async Task<List<string>> TestImapConnection(TestConnectionSettings emailSettings)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(emailSettings.Email));
            emailMessage.To.Add(new MailboxAddress(new MailSettings().FromEmail));
            emailMessage.Subject = "Test Email";
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = string.Empty
            };

            var errors = new List<string>();
            using (var client = new ImapClient())
            {
                try
                {
                    await client.ConnectAsync(emailSettings.ImapServer, emailSettings.ImapPort ?? 0);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    await client.AuthenticateAsync(emailSettings.Username, emailSettings.Password);
                    client.Inbox.Open(FolderAccess.ReadOnly);
                    return errors;
                }
                catch (AuthenticationException e)
                {
                    errors.Add(e.Message);
                    errors.Add(Resource.Connection_CheckUsernamePassword);
                    return errors;
                }
                catch (Exception)
                {
                    errors.Add(Resource.Connection_ContactAdmin);
                    return errors;
                }
            }
        }

        private static async Task<MxRecord> GetMxRecord(string domain)
        {
            var lookupClient = new LookupClient();
            var dnsQueryResponse = await lookupClient.QueryAsync(domain, QueryType.MX);
            var result = dnsQueryResponse.Answers.MxRecords()
                .OrderBy(x => x.Preference)
                .FirstOrDefault();
            return result;
        }

        private static string GetDomain(string email)
        {
            return email.Split('@').LastOrDefault();
        }

        private static async Task<HttpResponseMessage> GetIspbdResponse(string url)
        {
            using (var client = new HttpClient())
            {
                return await client.GetAsync(url);
            }
        }
    }
}