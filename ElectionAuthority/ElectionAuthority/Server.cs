using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Threading;
using System.Net;

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
        public bool StartServer(string stringPort)
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
        /// displays received message from client 
        /// </summary>
        /// <param name="client">name of client, each client has it own name which is loaded from config file</param>
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
                if (clients[clientSocket].Equals("NEW"))
                {
                    updateClientName(clientSocket, signal);
                    sendMessage(clients[clientSocket], Constants.CONNECTED);
                }
                else
                {
                    logs.AddLog(signal, true, Constants.LOG_MESSAGE, true); //do usuniecia ale narazie widzim co leci w komuniakcji
                    this.parser.parseMessage(signal);
                }
            }
            if (sListener != null)
            {
                try
                {
                    clientSocket.GetStream().Close();
                    clientSocket.Close();
                    clients.Remove(clientSocket);
                }
                catch (Exception)
                {
                    Console.WriteLine("Troubles with displaying received message");
                }

                logs.addLog(Constants.DISCONNECTED_NODE, true, Constants.LOG_ERROR, true);
            }

        }
        /// <summary>
        /// stops server
        /// </summary>
        public void stopServer()
        {
            try
            {

                foreach (TcpClient clientSocket in clients.Keys.ToList())
                {
                    clientSocket.GetStream().Close();
                    clientSocket.Close();
                    clients.Remove(clientSocket);
                }
                if (sListener != null)
                {
                    sListener.Stop();
                }
                sListener = null;
                sThread = null;
            }
            catch (Exception)
            {
                Console.WriteLine("Exception during closing server -  ElectionAuthority");
            }
        }
        /// <summary>
        /// sends message to client 
        /// </summary>
        /// <param name="name">name of client which we want to send a message</param>
        /// <param name="msg">message which we want to send</param>
        public void sendMessage(string name, string msg)
        {
            if (sListener != null)
            {
                NetworkStream stream = null;
                TcpClient client = null;
                List<TcpClient> clientsList = clients.Keys.ToList();
                for (int i = 0; i < clientsList.Count; i++)
                {
                    if (clients[clientsList[i]].Equals(name))
                    {
                        client = clientsList[i];
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
        /// updates a client name
        /// </summary>
        /// <param name="client">tcp socket which client is connected to</param>
        /// <param name="signal">client's name</param>
        private void updateClientName(TcpClient client, string signal)
        {
            if (signal.Contains("//NAME// "))
            {
                string[] tmp = signal.Split(' ');
                clients[client] = tmp[1];
            }
        }

        /// <summary>
        /// parses message
        /// </summary>
        /// <param name="msg">recived message</param>
        /// <returns>parsing result</returns>
        public bool handleMessage(string msg)
        {

            string[] words = msg.Split('&');
            switch (words[0])

            {
                case Constants.SL_RECEIVED_SUCCESSFULLY:
                    this.logs.addLog(Constants.SL_AND_SR_SENT_SUCCESSFULLY, true, Constants.LOG_INFO, true);
                    this.electionAuthority.disableSendSLTokensAndTokensButton();
                    return true;
                case Constants.GET_CANDIDATE_LIST:
                    string[] str = words[1].Split('=');
                    string name = str[0];
                    BigInteger SL = new BigInteger(str[1]);
                    this.electionAuthority.getCandidateListPermuated(name, SL);
                    return true;
                case Constants.BLIND_PROXY_BALLOT:
                    this.electionAuthority.saveBlindBallotMatrix(words[1]);
                    return true;
                case Constants.UNBLINED_BALLOT_MATRIX:
                    this.electionAuthority.saveUnblindedBallotMatrix(words[1]);
                    return true;
            }


            return false;
        }
    }
}
