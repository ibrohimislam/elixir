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
    class Electron
    {
        public Int32 client_number;
        private TcpClient client;
        private NetworkStream stream;
        private Proton command_processor;

        public Electron(TcpClient _client, int _client_number)
        {
            client_number = _client_number;
            client = _client;
            stream = _client.GetStream();
            command_processor = new Proton(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (stream != null) stream.Dispose();
                if (command_processor != null) command_processor.Dispose();
            }
        }

        public static void doAsync(Task aTask) { }

        public async Task Serve()
        {
            //enter to an infinite cycle to be able to handle every change in stream
            while (true)
            {
                Byte[] bytes = new Byte[client.ReceiveBufferSize];

                await stream.ReadAsync(bytes, 0, bytes.Length);

                String data = Encoding.UTF8.GetString(bytes);

                Console.WriteLine(Process.GetCurrentProcess().Threads.Count);

                if (new Regex("^GET").IsMatch(data))
                {
                    Handshake(stream, data);
                }
                else
                {
                    if (bytes[0]==129)
                    {
                        if (bytes.Length > 0)
                        {
                            string clientInput = ParseInput(bytes);
                            Console.WriteLine("Client#" + client_number.ToString() + " request: search @" + clientInput);

                            command_processor.Do(clientInput);
                        }
                    }
                    else if (bytes[0] == 136)
                    {
                        // close action
                        this.Dispose();
                        Console.WriteLine("Client#" + client_number.ToString() + " disconnected");
                    }
                }
            }
        }

        public async Task Handshake(NetworkStream stream, String data)
        {
            Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                        + "Connection: Upgrade" + Environment.NewLine
                        + "Upgrade: websocket" + Environment.NewLine
                        + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                            SHA1.Create().ComputeHash(
                                Encoding.UTF8.GetBytes(
                                    new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                )
                            )
                        ) + Environment.NewLine
                        + Environment.NewLine);

            doAsync(stream.WriteAsync(response, 0, response.Length));
        }

        public string ParseInput(Byte[] bytes)
        {
            int len;
            int offset;

            if (bytes[1] < 254) // 7 bit
            {
                len = bytes[1] & 127;
                offset = 2;
            }
            else // 16 bit
            {
                len = bytes[2] << 8 + bytes[3];
                offset = 4;
            }

            Byte[] key = new Byte[4];

            for (int i = 0; i < 4; i++)
                key[i] = bytes[offset + i];

            Byte[] decoded = new Byte[len];

            int payload_data_offset = offset+4;
            for (int i = 0; i < len; i++)
                decoded[i] = (Byte)(bytes[payload_data_offset + i] ^ key[i % 4]);

            return Encoding.ASCII.GetString(decoded);
        }

        public async Task Emit(string message)
        {
            int offset;
            Byte[] response;

            if (message.Length < 126)
            {
                response = new Byte[message.Length + 2];
                response[1] = (Byte)message.Length;
                offset = 2;
            }
            else // (message.Length < 65536)
            {
                response = new Byte[message.Length + 4];
                response[1] = (Byte) 126;
                response[2] = (Byte) (message.Length >> 8);
                response[3] = (Byte) (message.Length & 255);
                offset = 4;
            }

            response[0] = 129;
            
            Byte[] byte_string = Encoding.UTF8.GetBytes(message);
            byte_string.CopyTo(response, offset);

            doAsync(stream.WriteAsync(response, 0, response.Length));
        }
    }
}
