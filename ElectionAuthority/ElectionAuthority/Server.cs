using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Threading;
using System.Net;
using Org.BouncyCastle.Math;

namespace ElectionAuthority
{
    /// <summary>
    /// Zapewnia komunikacje poprzez połączenia TCP
    /// </summary>
    class Server
    {
        private TcpListener sListener;
        private Thread sThread;
        private Dictionary<TcpClient, string> clients;
        private ASCIIEncoding encoder;

        /// <summary>
        /// Instancja obiektu zapisującego logi
        /// </summary>
        private Logger logs;

        /// <summary>
        /// Połączenie z główny obiektem EA w celu przekazania otrzymanych danych
        /// </summary>
        private ElectionAuthority electionAuthority;

        /// <summary>
        /// Konstruktor klasy Server 
        /// </summary>
        /// <param name="logs">do zapisywania logów</param>
        /// <param name="electionAuthority">do połączenia z głównym obiektem EA</param>
        public Server(Logger logs, ElectionAuthority electionAuthority)
        {
            clients = new Dictionary<TcpClient, string>();
            this.encoder = new ASCIIEncoding();
            this.logs = logs;
        }

        /// <summary>
        /// Rozpoczęcie działania serwera
        /// </summary>
        /// <param name="port">port na którym nasłuchuje połączeń</param>
        /// <returns>zwraca prawdę gdy serwer zaczął poprawnie nasłuchiwać</returns>
        public bool Start(string stringPort)
        {
            int port = Convert.ToInt32(stringPort);
            Console.WriteLine("[Port used to communication] >> " + port);
            if (sListener == null && sThread == null)
            {
                try
                {
                    this.sListener = new TcpListener(IPAddress.Any, port);
                    this.sThread = new Thread(new ThreadStart(ListenForClients));
                    this.sThread.Start();
                    logs.AddLog(Constants.LOG_SERVER_START, Logger.LogType.Info);
                }
                catch (Exception exp)
                {
                    Console.WriteLine("[Server start] >> " + exp);
                    logs.AddLog(Constants.LOG_SERVER_START_ERROR, Logger.LogType.Error);
                    logs.AddExpToFile(exp);
                    return false;
                }
                
                return true;
            }
            else
            {
                logs.AddLog(Constants.LOG_SERVER_START_ERROR, Logger.LogType.Error);
                return false;
            }
        }

        /// <summary>
        /// Nasłuchuje połączeń od klientów TCP, tworzy nowe wątki do komunikacji z nimi
        /// </summary>
        private void ListenForClients()
        {
            try
            {
                this.sListener.Start();
            }
            catch (SocketException exp)
            {
                Console.WriteLine("[Server listener] >> " + exp);
                logs.AddExpToFile(exp);
            }
            while (true)
            {
                try
                {
                    TcpClient clientSocket = this.sListener.AcceptTcpClient();
                    clients.Add(clientSocket, "NEW");
                    Thread thread = new Thread(new ParameterizedThreadStart(HandleCommunication));
                    thread.Start(clientSocket);
                }
                catch
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Obsługa komunikacji z klientami TCP 
        /// </summary>
        /// <param name="clientTcp">klient z którym przeprowadzana jest komunikacja (uzyskany od listenera) </param>
        private void HandleCommunication(object clientTcp)
        {
            TcpClient client = (TcpClient)clientTcp;
            NetworkStream stream = client.GetStream();

            byte[] bytesMsg = new byte[4096];
            int bytesRead;

            while (stream.CanRead)
            {
                bytesRead = 0;
                try
                {
                    bytesRead = stream.Read(bytesMsg, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0) // brak nowych wiadomości
                {
                    break;
                }

                string msg = encoder.GetString(bytesMsg, 0, bytesRead);
                if (clients[client].Equals("NEW"))  // pierwsza wiadomość od klienta, zawiera jego nazwę
                {
                    if (msg.Contains("//NAME// ")) // zmiana nazwy z NEW na przysłaną przez klienta
                    {
                        string[] tmp = msg.Split(' ');
                        clients[client] = tmp[1];
                    }
                    SendMessage(clients[client], Constants.MSG_NEW_CLIENT);  // wysłanie potwierdzenia
                }
                else
                {
                    logs.AddLog(Constants.LOG_CLIENT_MSG + clients[client] + " :", Logger.LogType.Info);
                    logs.AddLog(msg, Logger.LogType.Message, false);
                    HandleMessage(msg);
                }
            }
            if (sListener != null)
            {
                try
                {
                    client.GetStream().Close();
                    client.Close();
                    clients.Remove(client);
                    logs.AddLog(Constants.LOG_CLIENT_DISCONNECT, Logger.LogType.Info);
                }
                catch (Exception exp)
                {
                    Console.WriteLine("[Server client " + clients[client] + " ] >> " + exp);
                    logs.AddExpToFile(exp);
                    logs.AddLog(Constants.LOG_CLIENT_DISCONNECT_ERROR + clients[client], Logger.LogType.Error);
                }  
            }
        }
        /// <summary>
        /// Wyłączenie serwera
        /// </summary>
        public void Stop()
        {
            try
            {
                foreach (TcpClient client in clients.Keys.ToList())
                {
                    client.GetStream().Close();
                    client.Close();
                    clients.Remove(client);
                }
                if (sListener != null)
                {
                    sListener.Stop();
                }
                sListener = null;
                sThread = null;
                logs.AddLog(Constants.LOG_SERVER_CLOSE, Logger.LogType.Info);
            }
            catch (Exception exp)
            {
                Console.WriteLine("[Server closing] >> " + exp);
                logs.AddExpToFile(exp);
                logs.AddLog(Constants.LOG_SERVER_CLOSE_ERROR, Logger.LogType.Error);
            }
        }

        /// <summary>
        /// Wysyłanie wiadomości klientom 
        /// </summary>
        /// <param name="name">nazwa klienta</param>
        /// <param name="msg">wiadomość</param>
        public void SendMessage(string name, string msg)
        {
            if (sListener != null)
            {
                NetworkStream stream = null;
                TcpClient client = null;
                List<TcpClient> clientsList = clients.Keys.ToList();
                foreach (KeyValuePair<TcpClient, string> cl in clients)
                {
                    if (cl.Value == name)
                    {
                        client = cl.Key;
                        break;
                    }
                }

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
                        clients.Remove(client);
                    }
                }
            }
        }

        /// <summary>
        /// Obsługuje otrzymane wiadomości
        /// </summary>
        /// <param name="msg">wiadomośc</param>
        /// <returns>zwraca fałsz jeśli nie potrafi obsłużyć wiadomości</returns>
        public bool HandleMessage(string msg)
        {
            string[] parts = msg.Split('&');
            switch (parts[0])
            {
                case Constants.SL_RECEIVED_SUCCESSFULLY:
                    this.logs.AddLog(Constants.LOG_SL_SENT, Logger.LogType.Info);
                    this.electionAuthority.disableSendSLTokensAndTokensButton();
                    return true;
                case Constants.GET_CANDIDATE_LIST:
                    string[] str = parts[1].Split('=');
                    string name = str[0];
                    BigInteger SL = new BigInteger(str[1]);
                    this.electionAuthority.getCandidateListPermuated(name, SL);
                    return true;
                case Constants.BLIND_PROXY_BALLOT:
                    this.electionAuthority.saveBlindBallotMatrix(parts[1]);
                    return true;
                case Constants.UNBLINDED_BALLOT_MATRIX:
                    this.electionAuthority.saveUnblindedBallotMatrix(parts[1]);
                    return true;
            }
            return false;
        }
    }
}
