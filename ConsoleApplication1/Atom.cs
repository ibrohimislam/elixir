using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Elixir
{
    class Atom
    {
        public void Run()
        {
            try
            {
                AsyncRun().Wait();
            }
            catch(System.AggregateException E)
            {
                Console.WriteLine(E.ToString());
                Console.ReadKey();
            }
        }

        public async Task AsyncRun()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);

            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:80.{0}Waiting for a connection...", Environment.NewLine);

            int number_clients = 0;

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                number_clients++;

                Console.WriteLine("Client#" + number_clients.ToString() + " connected.");

                var worker = new Electron(client, number_clients);
                worker.Serve();
            }
        }
    }
}
