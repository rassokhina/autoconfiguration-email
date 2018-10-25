using AutoconfigurationEmail.Core;
using System;

namespace AutoconfigurationEmail.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Enter email: ");
                string email = Console.ReadLine();
                Console.WriteLine(Environment.NewLine);


                // try to get autoconfiguration for email
                var serverOptions = IspdbClient.FindServerOptionsAsync(email).Result;
                PrintAutoconfiguration(serverOptions);


                // read username and password for test connection
                Console.WriteLine("To try to make a test connection we need username and password");
                Console.WriteLine("Enter username: ");
                string username = Console.ReadLine();
                Console.WriteLine("Enter password: ");
                string password = Console.ReadLine();
                Console.WriteLine(Environment.NewLine);


                // test connection 
                var errors = IspdbClient.TestServerConnections(new TestConnectionSettings
                {
                    Email = email,
                    ImapServer = serverOptions.ImapServer,
                    ImapPort = serverOptions.ImapPort,
                    SmtpServer = serverOptions.SmtpServer,
                    SmtpPort = serverOptions.SmtpPort,
                    Username = username,
                    Password = password
                })
                .Result;

                if (errors.Count > 0)
                {
                    Console.WriteLine($"Connections failed: { string.Join("\n", errors) }");
                }
                else
                {
                    Console.WriteLine($"Connections successed");
                }
                Console.ReadKey();
            }
            catch (Exception ex)
            {

                Console.WriteLine($"An exception occured: {ex.ToString()}");
            }

        }

        public static void PrintAutoconfiguration(ServerOptions serverOptions)
        {
            Console.WriteLine($"Smpt Server: {serverOptions.SmtpServer}");
            Console.WriteLine($"Smpt Port: {serverOptions.SmtpPort}");
            Console.WriteLine($"Imap Server: {serverOptions.ImapServer}");
            Console.WriteLine($"Imap Port: {serverOptions.ImapPort}");
            Console.WriteLine(Environment.NewLine);
        }

    }
}
