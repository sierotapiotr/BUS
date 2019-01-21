using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Voter
{
    /// <summary>
    /// Klasa reprezentująca klientów TCP - Election authority oraz Proxy
    /// </summary>
    class Client
    {
        /// <summary>
        /// Enkoder do przekształcania wiadomości ASCII - UTF-8
        /// </summary>
        private ASCIIEncoding encoder;
        /// <summary>
        /// Zasadniczy klient TCP
        /// </summary>
        private TcpClient tcpClient;
        /// <summary>
        /// Wątek, w którym uruchomiony jest klient Votera
        /// </summary>
        private Thread tcpClientThread;
        /// <summary>
        /// Strumień sieciowy do przesyłania wiadomości
        /// </summary>
        private NetworkStream stream;
        /// <summary>
        /// Logi - służą wyświetlaniu wiadomości
        /// </summary>
        private Logs logs;
        /// <summary>
        /// Parser do parsowania wiadomości od Election Authority oraz Proxy
        /// </summary>
        private Parser parser;
        /// <summary>
        /// nazwa klienta TCP
        /// </summary>
        private string tcpClientName;
        /// <summary>
        /// Flaga określająca, czy nawiązano połączenie z klientem TCP.
        /// </summary>
        private bool connected;

        public bool Connected
        {
            get { return connected; }
        }


        /// <summary>
        /// Konstruktor klasy klient
        /// </summary>
        /// <param name="name">nazwa votera</param>
        /// <param name="logs">instancja logów</param>
        /// <param name="voter">instancja votera</param>
        public Client(string name, Logs logs, Voter voter)
        {
            this.encoder = new ASCIIEncoding();
            this.logs = logs;
            this.tcpClientName = name;
            this.parser = new Parser(this.logs, voter);
            this.connected = false;
        }

        /// <summary>
        /// Metoda służąca połączeniu z klientem TCP
        /// </summary>
        /// <param name="ip">IP klienta</param>
        /// <param name="port">Port, na którym nasłuchuje klient</param>
        /// <param name="target">Nazwa klienta</param>
        /// <returns></returns>
        public bool Connect(string ip, string port, string target)
        {
            tcpClient = new TcpClient();
            IPAddress ipAddress;
            if (ip.Contains(Constants.LOCALHOST))
            {
                ipAddress = IPAddress.Loopback;
            }
            else
            {
                ipAddress = IPAddress.Parse(ip);
            }

            try
            {
                int dstPort = Convert.ToInt32(port);
                tcpClient.Connect(new IPEndPoint(ipAddress, dstPort));
            }
            catch
            {
            }

            if (tcpClient.Connected)
            {
                stream = tcpClient.GetStream();
                tcpClientThread = new Thread(new ThreadStart(DisplayMessageReceived));
                tcpClientThread.Start();
                SendName();
                connected = true;
                logs.AddLog(Constants.CONNECTION_PASS + target, Logs.LogType.Info);
                return true;
            }
            else
            {
                tcpClient = null;
                logs.AddLog(Constants.CONNECTION_FAILED + target, Logs.LogType.Error);
                return false;
            }

        }


        /// <summary>
        /// Metoda służąca wyświetlaniu otrzymanych wiadomości
        /// </summary>
        private void DisplayMessageReceived()
        {
            byte[] message = new byte[4096];
            int bytesRead;

            while (stream.CanRead)
            {
                bytesRead = 0;
                try
                {
                    bytesRead = stream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }
                string msg = encoder.GetString(message, 0, bytesRead);
                logs.AddLog(msg, Logs.LogType.Message);
                this.parser.ParseMessage(msg);
            }
            if (tcpClient != null)
            {
                Disconnect(true);
            }
        }

        /// <summary>
        /// Metoda służąca rozłączeniu z klientem TCP.
        /// </summary>
        /// <param name="error">Flaga sygnalizująca wystąpienie błędu</param>
        public void Disconnect(bool error = false)
        {
            if (tcpClient != null)
            {
                try
                {
                    tcpClient.GetStream().Close();
                    tcpClient.Close();
                    tcpClient = null;
                }
                catch
                {
                    Console.WriteLine(Constants.CONNECTION_DISCONNECTED);
                }

                if (!error)
                {
                    logs.AddLog(Constants.CONNECTION_DISCONNECTED, Logs.LogType.Info);
                }
                else
                {
                    logs.AddLog(Constants.CONNECTION_DISCONNECTED_ERROR, Logs.LogType.Info);
                }
            }
        }

        /// <summary>
        /// Metoda przesyłająca do strumienia nazwę klienta TCP.
        /// </summary>
        private void SendName()
        {
            {
                byte[] buffer = encoder.GetBytes("//NAME// " + tcpClientName);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }


        /// <summary>
        /// Metoda wysyłająca wiadmość strumieniem
        /// </summary>
        /// <param name="message">treść wiadomości</param>
        public void SendMessage(string message)
        {
            if (tcpClient != null && tcpClient.Connected && message != "")
            {
                byte[] buffer = encoder.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }


    }
}
