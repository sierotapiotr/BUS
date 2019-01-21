using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Org.BouncyCastle.Math;

namespace Proxy
{
    /// <summary>
    /// Klasa reprezentująca klienta TCP (połączenie z Election authority)
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
        private TcpClient client;
        /// <summary>
        /// Strumień sieciowy do przesyłania wiadomości
        /// </summary>
        private NetworkStream stream;
        /// <summary>
        /// Wątek, w którym uruchomiony jest klient
        /// </summary>
        private Thread clientThread;
        /// <summary>
        /// Wyświetla logi w polu dziennika logów
        /// </summary>
        private Logs logs;
        /// <summary>
        /// Do pomocy przy towrzeniu wiadomości do Voterów
        /// </summary>
        private Proxy proxy;

        /// <summary>
        /// Konstruktor klasy Client
        /// </summary>
        public Client(Logs logs, Proxy proxy)
        {
            this.encoder = new ASCIIEncoding();
            this.logs = logs;
            this.proxy = proxy;
        }

        /// <summary>
        /// Połączenie z serwerem EA
        /// </summary>
        public bool Connect(string ip, string port)
        {

            client = new TcpClient();
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
                client.Connect(new IPEndPoint(ipAddress, dstPort));
            }
            catch { }

            if (client.Connected)
            {
                stream = client.GetStream();
                clientThread = new Thread(new ThreadStart(DisplayMessageReceived));
                clientThread.Start();
                SendMyName();
                logs.AddLog(Constants.CONNECTION_PASS, Logs.LogType.Info);
                return true;
            }
            else
            {
                client = null;
                logs.AddLog(Constants.CONNECTION_FAILED, Logs.LogType.Error);
                return false;
            }
        }

        /// <summary>
        /// Wysłanie swojej nazwy
        /// </summary>
        private void SendMyName()
        {
            {
                byte[] buffer = encoder.GetBytes("//NAME// " + "PROXY");
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }

        /// <summary>
        /// Odczytywanie otrzymanych od EA wiadomości
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
                string strMessage = encoder.GetString(message, 0, bytesRead);
                ParseMessageFromEA(strMessage);
            }
            if (client != null)
            {
                DisconnectFromElectionAuthority(true);
            }
        }

        /// <summary>
        /// Rozłączenie z serwerem EA
        /// </summary>
        /// <param name="error"></param>
        public void DisconnectFromElectionAuthority(bool error = false)
        {
            if (client != null)
            {
                try
                {
                    client.GetStream().Close();
                    client.Close();
                    client = null;
                }
                catch (Exception)
                {
                    Console.WriteLine("Problems with disconnecting from EA");
                }
                if (!error)
                {
                    logs.AddLog(Constants.CONNECTION_DISCONNECTED, Logs.LogType.Info);
                }
                else
                {
                    logs.AddLog(Constants.CONNECTION_DISCONNECTED_ERROR, Logs.LogType.Error);
                    //form.Invoke(new MethodInvoker(delegate() { form.buttonsEnabled(); }));
                }
            }
        }

        /// <summary>
        /// parsing SL and tokens recived from EA
        /// </summary>
        /// <param name="msg">message</param>
        /// <returns>result of parsing process</returns>
        private bool parseSLTokensDictionaryFromEA(string msg)
        {
            //msg = FIRST_SL=tokensList[0],tokensList[1],tokensList[2]....:exponentsList[0],exponentsList[1],exponentsList[2]....;SECOND_SL
            Dictionary<BigInteger, List<List<BigInteger>>> dict = new Dictionary<BigInteger, List<List<BigInteger>>>();

            string[] dictionaryElem = msg.Split(';');
            for (int i = 0; i < dictionaryElem.Length; i++)
            {

                string[] words = dictionaryElem[i].Split('=');
                BigInteger SL = new BigInteger(words[0]);
                List<List<BigInteger>> mainList = new List<List<BigInteger>>();

                string[] token = words[1].Split(':');
                //token[0] contains tokenList 
                //token[1] contains exponentsList


                string[] tokenList = token[0].Split(',');
                List<BigInteger> firstList = new List<BigInteger>();
                foreach (string str in tokenList)
                {
                    firstList.Add(new BigInteger(str));
                }
                mainList.Add(firstList);


                string[] exponentsList = token[1].Split(',');
                List<BigInteger> secondList = new List<BigInteger>();
                foreach (string str in exponentsList)
                {
                    secondList.Add(new BigInteger(str));
                }
                mainList.Add(secondList);


                dict.Add(SL, mainList);


            }

            this.proxy.SerialNumberTokens = dict;
            this.proxy.ConnectSRandSL();
            return true;
        }


        /// <summary>
        /// parsing message from ea
        /// </summary>
        /// <param name="msg">message</param>
        public void ParseMessageFromEA(string msg)
        {
            string[] elem = msg.Split('&');
            switch (elem[0])
            {
                case Constants.SL_TOKENS:
                    if (parseSLTokensDictionaryFromEA(elem[1]))
                        SendMessage(Constants.SL_RECEIVED_SUCCESSFULLY + "&");
                    this.logs.AddLog(Constants.SL_RECEIVED, Logs.LogType.Info);
                    break;
                case Constants.CONNECTED:
                    this.proxy.disableConnectElectionAuthorityButton();
                    this.logs.AddLog(Constants.PROXY_CONNECTED_TO_EA, Logs.LogType.Info);
                    break;
                case Constants.SIGNED_PROXY_BALLOT:
                    this.proxy.SaveSignedBallot(elem[1]);
                    break;
            }
        }

        /// <summary>
        /// Wysyłanie wiadomości do EA
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(string msg)
        {
            if (client != null && client.Connected && msg != "")
            {
                byte[] buffer = encoder.GetBytes(msg);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }
    }
}
