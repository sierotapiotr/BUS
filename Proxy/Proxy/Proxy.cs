using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Proxy
{
    /// <summary>
    /// Główna logika programu, wykonuje działania "Proxy" wg. algorytmu "Scratch, Click and Vote: E2E voting over the Internet"
    /// </summary>
    class Proxy
    {
        /// <summary>
        /// Wyświetla logi w polu dziennika logów
        /// </summary>
        private Logs logs;

        /// <summary>
        /// Konfiguracja (wczytywana z pliku)
        /// </summary>
        private Configuration configuration;

        /// <summary>
        /// Główne okno aplikacji (do sterowania przyciskami)
        /// </summary>
        private Form1 form;

        /// <summary>
        /// Serwer do komunikacji z Voterami
        /// </summary>
        private Server server;
        public Server Server
        {
            get { return server; }
        }

        /// <summary>
        /// Klient do komunikacji z EA
        /// </summary>
        private Client client;
        public Client Client
        {
            get { return client; }
        }
        /// <summary>
        /// Lista numerów seryjnych SR
        /// </summary>
        private List<BigInteger> SRList;

        /// <summary>
        /// Zbiór kart do głosowania od Voterów
        /// </summary>
        private Dictionary<string, ProxyBallot> proxyBallots; 

        /// <summary>
        /// Liczba Voterów
        /// </summary>
        private int numberOfVoters;

        /// <summary>
        /// Zestawienie SL i Tokenów (od EA)
        /// </summary>
        private Dictionary<BigInteger, List<List<BigInteger>>> serialNumberTokens; 
        public Dictionary<BigInteger, List<List<BigInteger>>> SerialNumberTokens
        {
            get { return this.serialNumberTokens; }
            set {this.serialNumberTokens = value;}
        }

        /// <summary>
        /// Połączenie SL i SR
        /// </summary>
        private Dictionary<BigInteger, BigInteger> serialNumberAndSR;
 
        /// <summary>
        /// Liczba wysłanych SL i SR (liczba zgłoszeń)
        /// </summary>
        private static int numOfSentSLandSR = 0;
 

        /// <summary>
        /// Pozycje głosów na "tak" i na "nie"
        /// </summary>
        private List<string> yesNoPosition; 

        /// <summary>
        /// Konstruktor klasy Proxy
        /// </summary>
        /// <param name="logs">logs instance</param>
        /// <param name="conf">laoded configuration</param>
        /// <param name="form">form appplication</param>
        public Proxy(Logs logs, Configuration conf, Form1 form)
        {
            this.logs = logs;
            this.configuration = conf;
            this.form = form;
            this.server = new Server(this.logs,this);
            this.client = new Client(this.logs, this);

            this.serialNumberTokens = new Dictionary<BigInteger, List<List<BigInteger>>>();
            this.SRList = new List<BigInteger>();
            this.serialNumberAndSR = new Dictionary<BigInteger, BigInteger>();
            this.proxyBallots = new Dictionary<string, ProxyBallot>();
        }

        /// <summary>
        /// Generacja numerów seryjnych SR
        /// </summary>
        public void GenerateSR()
        {
            this.numberOfVoters = this.configuration.NumOfVoters;
            this.SRList = SerialNumberGenerator.generateListOfSerialNumber(this.numberOfVoters, Constants.NUMBER_OF_BITS_SR);
            logs.AddLog(Constants.SR_GEN_SUCCESSFULLY, Logs.LogType.Info);

        }

        /// <summary>
        /// Generacja losowych pozycji "tak" i "nie"
        /// </summary>
        public void GenerateYesNoPosition()
        {
            this.yesNoPosition = new List<string>();
            this.yesNoPosition = SerialNumberGenerator.getYesNoPosition(this.configuration.NumOfVoters, this.configuration.NumOfCandidates);
            logs.AddLog(Constants.YES_NO_POSITION_GEN_SUCCESSFULL, Logs.LogType.Info);
            SaveYesNoPositionToFile();
            logs.AddLog(Constants.YES_NO_POSITION_SAVED_TO_FILE, Logs.LogType.Info);

        }

        /// <summary>
        /// Połączenie SR i SL
        /// </summary>
        public void ConnectSRandSL()
        {
            for(int i=0; i<this.SRList.Count; i++)
            {
                this.serialNumberAndSR.Add(serialNumberTokens.ElementAt(i).Key, SRList[i]);
            }
        }

        /// <summary>
        /// Wysłanie SL i SR do Votera
        /// </summary>
        /// <param name="name">voter ID</param>
        public void SendSLAndSR(string name)
        {
            if (this.serialNumberAndSR != null && this.serialNumberAndSR.Count != 0)
            {
                BigInteger SL = this.serialNumberAndSR.ElementAt(numOfSentSLandSR).Key;
                BigInteger SR = this.serialNumberAndSR.ElementAt(numOfSentSLandSR).Value;
                List<BigInteger> tokensList = this.serialNumberTokens[SL][0];
                List<BigInteger> exponentesList = this.serialNumberTokens[SL][1];

                this.proxyBallots.Add(name, new ProxyBallot(this.logs, SL, SR));
                this.proxyBallots[name].TokensList = tokensList;
                this.proxyBallots[name].ExponentsList = exponentesList;

                string position = this.yesNoPosition.ElementAt(numOfSentSLandSR);
                this.proxyBallots[name].YesNoPos = position;

                string msg = Constants.SL_AND_SR + "&" + SL.ToString() + "=" + SR.ToString();
                numOfSentSLandSR += 1;
                this.server.SendMessage(name, msg);
            }
            else
            {
                this.logs.AddLog(Constants.ERROR_SEND_SL_AND_SR, Logs.LogType.Error);
            }
            

        }

        /// <summary>
        /// Wyłączenie guzika "Połącz z EA"
        /// </summary>
        public void disableConnectElectionAuthorityButton()
        {
            this.form.Invoke(new MethodInvoker(delegate()
                {
                    this.form.disableConnectElectionAuthorityButton();
                }));
        }

        /// <summary>
        /// Zapisanie pozycji "tak" i "nie" 
        /// </summary>
        private void SaveYesNoPositionToFile()
        {
            if (this.yesNoPosition != null)
            {
                string[] yesNoPositionStrTable = this.yesNoPosition.ToArray();
                System.IO.File.WriteAllLines(@"yesNoPositions.txt", yesNoPositionStrTable);
            }
        }

        /// <summary>
        /// Zapisanie głosu od Votera i przekazanie go do EA (zaślepionego)
        /// </summary>
        /// <param name="message">głos Votera</param>
        public void SaveVote(string message)
        {
            //message = 'name;first_row;second_row .....;last_row'
            //first_row = 'x:y:z:v'
            int[,] vote = new int[this.configuration.NumOfCandidates, 4];
            string[] words = message.Split(';');
            string name = words[0];
            for (int i = 1; i < words.Length-1; i++)
            {
                string[] row = words[i].Split(':'); 
                for (int k = 0; k < row.Length; k++)
                {
                    vote[i-1,k] = Convert.ToInt32(row[k]);
                    Console.WriteLine(vote[i - 1, k]);
                }

            }

            this.proxyBallots[name].Vote = vote;
            this.proxyBallots[name].ConfirmationColumn = Convert.ToInt32(words[words.Length - 1]);
            this.logs.AddLog(Constants.VOTE_RECEIVED + name, Logs.LogType.Info);
            this.proxyBallots[name].GenerateAndSplitBallotMatrix();
            this.logs.AddLog(Constants.BALLOT_MATRIX_GEN + name, Logs.LogType.Info);
            BigInteger[] blindProxyBallot = this.proxyBallots[name].BlindDataToSend();
            //Console.WriteLine("blind proxy ballot = " + blindProxyBallot);
            
            string SL = this.proxyBallots[name].SL.ToString();
            string tokens = PrepareTokens(this.proxyBallots[name].SL);
            string columns = PrepareBlindProxyBallot(blindProxyBallot);
            //Console.WriteLine(columns);
            //string pubKeyModulus = this.proxyBallots[name].PubKey.Modulus.ToString();
            //msg = BLIND_PROXY_BALLOT&name;pubKeyModulus;SL_number;token1,token2,token3,token4;col1,col2,col3,col4
            string msg = Constants.BLIND_PROXY_BALLOT + "&" + name + ";"  + SL + ";" + tokens + columns ;

            this.client.SendMessage(msg);
        }


        /// <summary>
        /// Przygotowanie zaślepionej "ballot matrix"
        /// </summary>
        /// <param name="blindProxyBallot">proxy ballot</param>
        /// <returns>proxy ballot as string (ready to send)</returns>
        private string PrepareBlindProxyBallot(BigInteger[] blindProxyBallot)
        {
            string columns = null;
            for (int i = 0; i < blindProxyBallot.Length; i++)
            {
                if (i != blindProxyBallot.Length - 1)
                    columns = columns + blindProxyBallot[i].ToString() + ",";
                else
                    columns += blindProxyBallot[i].ToString();
            }

            return columns;
        }

        /// <summary>
        /// Przygotowanie tokenów do wysłania
        /// </summary>
        /// <param name="SL">numer seryjny SL</param>
        /// <returns>wiadomość z tokenami</returns>
        private string PrepareTokens(BigInteger SL)
        {
            string tokens = null;
            List<BigInteger> tokenList = this.serialNumberTokens[SL][0];
            for (int i = 0; i < tokenList.Count; i++)
            {
                if (i != tokenList.Count - 1)
                    tokens = tokens + tokenList[i].ToString() + ",";
                else
                    tokens = tokens + tokenList[i].ToString() + ";";
            }

            List<BigInteger> exponentsList = this.serialNumberTokens[SL][1];
            for (int i = 0; i < exponentsList.Count; i++)
            {
                if (i != exponentsList.Count - 1)
                    tokens = tokens + exponentsList[i].ToString() + ",";
                else
                    tokens = tokens + exponentsList[i].ToString() + ";";
            }

            return tokens;
        }

        /// <summary>
        /// Zapisanie podpisanej przez EA karty
        /// </summary>
        /// <param name="message">podpisana karta</param>
        public void SaveSignedBallot(string message)
        {
            string[] words = message.Split(';');

            foreach (string s in words)
            {
                Console.WriteLine(s);
            }


            string name = words[0];

            string[] signedColumns = words[1].Split(',');
            List<BigInteger> signedColumnsList = new List<BigInteger>();
            foreach (string s in signedColumns)
            {
                BigInteger big = new BigInteger(s);
                signedColumnsList.Add(big);
            }

            this.proxyBallots[name].SignedColumns = signedColumnsList;
            this.logs.AddLog(Constants.SIGNED_COLUMNS_RECEIVED, Logs.LogType.Info);

            this.SendSignedColumnToVoter(name);
            this.UnblindSignedBallotMatrix(name);
        }


        /// <summary>
        /// Wysłanie podpisanej kolumny do Votera (jego potwierdzenie)
        /// </summary>
        /// <param name="name"></param>
        private void SendSignedColumnToVoter(string name)
        {
            int confirmation = this.proxyBallots[name].ConfirmationColumn - 1;
            string token = this.proxyBallots[name].TokensList[confirmation].ToString(); 

            BigInteger signedBlindColumn = this.proxyBallots[name].SignedColumns[confirmation];
            string signedBlindColumnStr = signedBlindColumn.ToString();
            string message = Constants.SIGNED_COLUMNS_TOKEN + "&" + signedBlindColumnStr + ";" + token;
            this.server.SendMessage(name, message);
        }

        /// <summary>
        /// Odślepienie podpisanej "ballot matrix" i przekazanie jej EA
        /// </summary>
        /// <param name="name">voter name</param>
        private void UnblindSignedBallotMatrix(string name)
        {

            BigInteger[]  signedColumns = this.proxyBallots[name].SignedColumns.ToArray();
            string[] strUnblindedBallotMatrix = this.proxyBallots[name].UnblindSignedData(signedColumns);
            Console.WriteLine("odslepiona ballotMatrix");
            string unblinedColumns = null;
            for (int i =0; i<strUnblindedBallotMatrix.Length;i++)
            {
                Console.WriteLine(strUnblindedBallotMatrix[i]);
                if (i!= strUnblindedBallotMatrix.Length -1)
                    unblinedColumns = unblinedColumns + strUnblindedBallotMatrix[i] + ",";
                else
                    unblinedColumns += strUnblindedBallotMatrix[i];
            }

            string message = Constants.UNBLINED_BALLOT_MATRIX + "&" + name + ";" + unblinedColumns;

            this.client.SendMessage(message);
        }
    }
}
