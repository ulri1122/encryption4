using encryption;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace server
{
    public class Program
    {
        public static Encryption enc = new Encryption();
        public static List<TcpClient> clients = new List<TcpClient>();
        public static string key = "jegharmangekeys";
        public static string privatekey = "32";
        public static string gkey = "129";
        public static string abgkey;
        public static byte[] pgkey;
        public static string nkey = "983090";
        static void Main(string[] args)
        {

            pgkey = enc.createpublicKey(privatekey, gkey, nkey);
            


            int port = 13356;
            IPAddress ip = IPAddress.Any;
            IPEndPoint localEndpoint = new IPEndPoint(ip, port);

            TcpListener listener = new TcpListener(localEndpoint);
            listener.Start();
            
            Console.WriteLine("venter på klient\'");
            AcceptClients(listener);
            bool isRunning = true;

            while (isRunning)
            {
                Console.WriteLine("skiv en besked til klient");
                string message = Console.ReadLine();
                byte[] buffer = Encoding.UTF8.GetBytes(message);

                foreach (TcpClient client in clients)
                {
                    client.GetStream().Write(enc.EncryptByte(buffer, abgkey), 0, buffer.Length);
                }

                if (message.ToLower() == "c")
                {
                    Console.WriteLine("Conn closed");
                    break;
                }
            }
        }

        public static async void AcceptClients(TcpListener listener)
        {
         
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                clients.Add(client);
                NetworkStream stream = client.GetStream();

                stream.Write(pgkey, 0, pgkey.Length);

                byte[] buffer = new byte[256];
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);

                byte[] keyfromclient = new byte[read];
                Array.Copy(buffer, 0, keyfromclient, 0, read);
                abgkey = enc.genabgkey(privatekey, keyfromclient, nkey);
                Console.WriteLine(abgkey);
                RecieveMessages(stream);
            }
        }

        public static async void RecieveMessages(NetworkStream stream)
        {
            byte[] buffer = new byte[256];

            while (true)
            {
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);



                string text = Encoding.UTF8.GetString(enc.DecryptByte(buffer, abgkey), 0, read);
                Console.WriteLine($"Client writes: {text}");
                if (text.ToLower() == "c")
                {
                    break;
                }
            }
        }
    }
}