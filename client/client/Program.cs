using encryption;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace client
{

    class Program
    {
        public static Encryption enc = new Encryption();
        public static string key = "jegharmangekeys";
        public static string privatekey = "24";
        public static string gkey = "128";
        public static string abgkey;
        public static byte[] pgkey;
        public static string nkey = "983090";
        static void Main(string[] args)
        {

            pgkey = enc.createpublicKey(privatekey, gkey, nkey);

            TcpClient client = new TcpClient();
            int port = 13356;

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(ip, port);

            client.Connect(endPoint);

            NetworkStream stream = client.GetStream();
            Console.WriteLine("skriv besked til server?\n\"c\" to cancel");



            stream.Write(pgkey, 0, pgkey.Length);
            byte[] buffer = new byte[256];
            int read = stream.Read(buffer, 0, buffer.Length);

            byte[] keyfromclient = new byte[read];
            Array.Copy(buffer, 0, keyfromclient, 0, read);
   
            abgkey = enc.genabgkey(privatekey, keyfromclient, nkey);

            Console.WriteLine(abgkey);

            RecieveMessage(stream);

            while (true)
            {
                string userInput = Console.ReadLine();
                if (userInput.ToLower() == "c")
                {
                    break;
                }
                buffer = Encoding.UTF8.GetBytes(userInput);

                stream.Write(enc.EncryptByte(buffer, abgkey), 0, buffer.Length);

            }
            client.Close();
        }

        public static async void RecieveMessage(NetworkStream stream)
        {
            while (true)
            {
                byte[] buffer = new byte[256];
                int nBytesRead = await stream.ReadAsync(buffer, 0, 256);

                string message = Encoding.UTF8.GetString(enc.DecryptByte(buffer, abgkey), 0, nBytesRead);
                if (message != string.Empty)
                {
                    Console.WriteLine(message);
                }
            }

        }
    }
}