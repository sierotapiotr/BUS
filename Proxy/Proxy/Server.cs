using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Proxy
{
    /// <summary>
    /// Klasa reprezentująca serwer TCP (połączenie z Voterami)
    /// </summary>
    class Server
    {
        /// <summary>
        /// Socket dla serwera
        /// </summary>
        private TcpListener serverSocket;

        /// <summary>
        /// Wątek serwera
        /// </summary>
        private Thread serverThread;

        /// <summary>
        /// Połączenie socketów i nazw/identyfikatorów Voterów
        /// </summary>
        private Dictionary<TcpClient, string> clientSockets;
        public Dictionary<TcpClient, string> ClientSockets
        {
            get { return clientSockets; }
        }
        /// <summary>
        /// Enkoder do przekształcania wiadomości ASCII - UTF-8
        /// </summary>
        private ASCIIEncoding encoder;
        /// <summary>
        /// Wyświetla logi w polu dziennika logów
        /// </summary>
        private Logs logs;
        /// <summary>
        /// Do pomocy przy towrzeniu wiadomości do Voterów
        /// </summary>
        private Proxy proxy;


        /// <summary>
        /// Konstruktor klasy Server
        /// </summary>
        /// <param name="logs">display messages in logs console</param>
        /// <param name="proxy">main logic of Proxy application</param>
        public Server(Logs logs, Proxy proxy)
        {
            clientSockets = new Dictionary<TcpClient, string>();
            this.encoder = new ASCIIEncoding();
            this.logs = logs;
            this.proxy = proxy;
        }


        /// <summary>
        /// Rozpoczęcie działania serwera
        /// </summary>
        /// <param name="port">port na którym nasłuchuje połączeń</param>
        /// <returns>zwraca prawdę gdy serwer zaczął poprawnie nasłuchiwać</returns>
        public bool StartServer(string port)
        {
            int runningPort = Convert.ToInt32(port);
            if (serverSocket == null && serverThread == null)
            {
                this.serverSocket = new TcpListener(IPAddress.Any, runningPort);
                this.serverThread = new Thread(new ThreadStart(ListenForClients));
                this.serverThread.Start();
                logs.AddLog(Constants.SERVER_STARTED_CORRECTLY, Logs.LogType.Info);
                return true;
            }
            else
            {
                logs.AddLog(Constants.SERVER_UNABLE_TO_START, Logs.LogType.Error);
                return false;
            }
        }
        /// <summary>
        /// Nasłuchuje połączeń od klientów TCP, tworzy nowe wątki do komunikacji z nimi
        /// </summary>
        private void ListenForClients()
        {
            this.serverSocket.Start();
            while (true)
            {
                try
                {
                    TcpClient clientSocket = this.serverSocket.AcceptTcpClient();
                    clientSockets.Add(clientSocket, Constants.UNKNOWN);
                    Thread clientThread = new Thread(new ParameterizedThreadStart(displayMessageReceived));
                    clientThread.Start(clientSocket);
                }
                catch
                {
                    break;
                }
            }
        }


        /// <summary>
        /// Wyświetlanie (i obsługiwanie) wiadomości od Voterów
        /// </summary>
        /// <param name="client"></param>
        private void displayMessageReceived(object client)
        {
            TcpClient clientSocket = (TcpClient)client;
            NetworkStream stream = clientSocket.GetStream();

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

                string signal = encoder.GetString(message, 0, bytesRead);
                if (clientSockets[clientSocket].Equals(Constants.UNKNOWN))
                {
                    updateClientName(clientSocket, signal); //clients as first message send his id
                    string msg = Constants.CONNECTION_SUCCESSFUL + "&";
                    SendMessage(clientSockets[clientSocket], msg);
                    logs.AddLog(Constants.VOTER_CONNECTED, Logs.LogType.Info);
                }
                else
                {
                    HandleMessageFromClient(signal);
                    logs.AddLog(signal, Logs.LogType.Message);
                }
            }
            if (serverSocket != null)
            {
                try
                {
                    clientSocket.GetStream().Close();
                    clientSocket.Close();
                    clientSockets.Remove(clientSocket);
                }
                catch
                {
                }
                logs.AddLog(Constants.DISCONNECTED_NODE, Logs.LogType.Error);
            }

        }

        /// <summary>
        /// Obsługuje otrzymanych wiadomości 
        /// </summary>
        /// <param name="msg">wiadomość</param>
        public void HandleMessageFromClient(string msg)
        {
            string[] elem = msg.Split('&');
            switch (elem[0])
            {
                case Constants.GET_SL_AND_SR:
                    this.proxy.SendSLAndSR(elem[1]);
                    break;
                case Constants.VOTE:
                    this.proxy.SaveVote(elem[1]);
                    break;

            }
        }

        /// <summary>
        /// Wyłączenie serwera
        /// </summary>
        public void StopServer()
        {
            foreach (TcpClient clientSocket in clientSockets.Keys.ToList())
            {
                clientSocket.GetStream().Close();
                clientSocket.Close();
                clientSockets.Remove(clientSocket);
            }
            if (serverSocket != null)
            {
                serverSocket.Stop();
            }
            serverSocket = null;
            serverThread = null;
        }

        /// <summary>
        /// Wysyłanie wiadomości klientom 
        /// </summary>
        /// <param name="name">client's name</param>
        /// <param name="msg">message</param>
        public void SendMessage(string name, string msg)
        {
            for (int i = 0; i < clientSockets.Count; i++)
            {
                Console.WriteLine("nazwy clientow " + clientSockets.ElementAt(i).Value.ToString()); 
            }


            if (serverSocket != null)
            {
                NetworkStream stream = null;
                TcpClient client = getTcpClient(name);
                
                

                if (client != null)
                {
                    if (client.Connected)
                    {
                        stream = client.GetStream();
                        byte[] buffer = encoder.GetBytes(msg);
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();
                    }
                    else
                    {
                        stream.Close();
                        clientSockets.Remove(client);
                    }
                }
            }
        }

        /// <summary>
        /// Zaktualizowanie nazwy/identyfikatora Votera
        /// </summary>
        /// <param name="client">TcpClient</param>
        /// <param name="signal">client name</param>
        private void updateClientName(TcpClient client, string signal)
        {
            if (signal.Contains("//NAME// "))
            {
                string[] tmp = signal.Split(' ');
                clientSockets[client] = tmp[1];
            }
        }

        /// <summary>
        /// Znaleźienie klienta TCP z Voterem o zadanej nazwie/identyfikatorze
        /// </summary>
        /// <param name="name">nazwa Votera</param>
        /// <returns>zwraca klienta TCP</returns>
        private TcpClient getTcpClient(string name)
        {
            TcpClient client = null;
            List<TcpClient> clientsList = clientSockets.Keys.ToList();
            for (int i = 0; i < clientsList.Count; i++)
            {
                if (clientSockets[clientsList[i]].Equals(name))
                {
                    client = clientsList[i];
                    return client;
                }
            }
            return null;
        }
    }
}
