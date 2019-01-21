using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Windows.Forms;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;
using System.Security.Cryptography;

namespace ElectionAuthority
{
    /// <summary>
    /// Główna logika programu, wykonuje działania "Election Authority" wg. algorytmu "Scratch, Click & Vote: E2E voting over the Internet"
    /// </summary>
    class ElectionAuthority
    {
        /// <summary>
        /// Wyświetla logi w polu dziennika logów
        /// </summary>
        private Logger logs;

        /// <summary>
        /// Główne okno aplikacji (do sterowania przyciskami)
        /// </summary>
        private Form1 form;

        /// <summary>
        /// Serwer dla głosujących
        /// </summary>
        private Server serverForVoters;
        public Server ServerForVoters
        {
            get { return serverForVoters; }
        }

        /// <summary>
        /// Serwer dla Proxy
        /// </summary>
        private Server serverForProxy;
        public Server ServerForProxy
        {
            get { return serverForProxy; }
        }

        /// <summary>
        /// Lista kandydatów
        /// </summary>
        private List<String> candidateDefaultList;

        /// <summary>
        /// Konfiguracja (wczytywana z pliku)
        /// </summary>
        private Config configuration;

        /// <summary>
        /// Do wykonywania permutacji
        /// </summary>
        private Permutator permutator;

        /// <summary>
        /// Lista permutacji
        /// </summary>
        private List<List<BigInteger>> permutationsList;

        /// <summary>
        /// Lista odwróconych permutacji
        /// </summary>
        private List<List<BigInteger>> inversePermutationList;

        /// <summary>
        /// Lista numerów seryjnych SL
        /// </summary>
        private List<BigInteger> serialNumberList;

        /// <summary>
        /// Lista tokenów (4 tokeny na jeden SL)
        /// </summary>
        private List<List<BigInteger>> tokensList;

        /// <summary>
        /// Lista eksponent do zaślepiania (wysyłana Proxy)
        /// </summary>
        private List<List<BigInteger>> exponentsList;

        /// <summary>
        /// Lista czynników do podpisu
        /// </summary>
        private List<List<BigInteger>> signatureFactor;

        /// <summary>
        /// Połączenia SL i permutacji
        /// </summary>
        private Dictionary<BigInteger, List<BigInteger>> dictionarySLPermuation;

        /// <summary>
        /// Połączenia SL i odwróconych permutacji
        /// </summary>
        private Dictionary<BigInteger, List<BigInteger>> dictionarySLInversePermutation;

        /// <summary>
        /// Połączenia SL i tokenów
        /// </summary>
        private Dictionary<BigInteger, List<List<BigInteger>>> dictionarySLTokens;

        /// <summary>
        /// Zbiór kart do głosowania od Voterów
        /// </summary>
        private Dictionary<string, Ballot> ballots;

        /// <summary>
        /// Liczba głosujących
        /// </summary>
        private int numberOfVoters;

        /// <summary>
        /// Wyniki głosowania
        /// </summary>
        private int[] finalResults;

        /// <summary>
        /// Audytor sprawdzający poprawność głosowania 
        /// </summary>
        private Auditor auditor;

        /// <summary>
        /// Klucz prywatny do zobowiązania bitowego (dla permutacji)
        /// </summary>
        private RsaKeyParameters privKey;

        /// <summary>
        /// Klucz publiczny do zobowiązania bitowego (dla permutacji)
        /// </summary>
        private RsaKeyParameters pubKey;

        /// <summary>
        /// Tokeny do ślepego podpisu
        /// </summary>
        private List<BigInteger> permutationModulusList;

        /// <summary>
        /// Eksponenty do ślepego podpisu
        /// </summary>
        private List<BigInteger> permutationExponentsList;


        /// <summary>
        /// Konstruktor EA
        /// </summary>
        /// <param name="logs">do zapisywania logów</param>
        /// <param name="configuration">do wczytania konfiguracji</param>
        /// <param name="form">okno aplikacji do sterowania przyciskami</param>
        public ElectionAuthority(Logger logs, Config configuration, Form1 form)
        {
            this.logs = logs;
            this.configuration = configuration;
            this.form = form;
            candidateDefaultList = new List<string>();
            candidateDefaultList = this.configuration.Candidates;

            this.serverForVoters = new Server(this.logs, this);
            this.serverForProxy = new Server(this.logs, this);

            this.numberOfVoters = Convert.ToInt32(this.configuration.NumberOfVoters);
            permutator = new Permutator(this.logs);
            this.ballots = new Dictionary<string, Ballot>();

            this.auditor = new Auditor(this.logs);

            //init key pair generator (for RSA bit-commitment)
            KeyGenerationParameters para = new KeyGenerationParameters(new SecureRandom(), 1024);
            RsaKeyPairGenerator keyGen = new RsaKeyPairGenerator();
            keyGen.Init(para);


            //generate key pair and get keys (for bit-commitment)
            AsymmetricCipherKeyPair keypair = keyGen.GenerateKeyPair();
            privKey = (RsaKeyParameters)keypair.Private;
            pubKey = (RsaKeyParameters)keypair.Public;
        }

        /// <summary>
        /// Przygotowanie potrzebnyc danych - SL, tokenów i permutacji listy kandydatów
        /// </summary>
        public void GenerateData()
        {
            GenerateSerialNumber();
            GenerateTokens();
            GeneratePermutation();
        }

        /// <summary>
        /// Tworzenie permutacji list kandydatów
        /// </summary>
        private void GeneratePermutation()
        {
            //generating permutation and feeling List
            permutationsList = new List<List<BigInteger>>();
            for (int i = 0; i < this.numberOfVoters; i++)
            {
                this.permutationsList.Add(new List<BigInteger>(this.permutator.PermutateOnce(candidateDefaultList.Count)));
            }

            ConnectSerialNumberAndPermutation();
            GenerateInversePermutation();
            GeneratePermutationTokens();
            BlindPermutation(permutationsList);              //Send commited permutation to Auditor
            logs.AddLog(Constants.LOG_PERMUTATION_GEN, Logger.LogType.Info);

        }

        /// <summary>
        /// Pomocnicza funkcja do generacji numerów seryjnych SL
        /// </summary>
        /// <param name="n">liczba SL do wygenerowania/param>
        /// <param name="bytes">liczba bajtów w pojedynczym SL</param>
        /// <returns>zwraca listę numerów seryjnych</returns>
        public static List<BigInteger> GenerateListOfSerialNumber(int numberOfSerials, int numberOfBits)
        {
            List<BigInteger> listOfSerialNumber = new List<BigInteger>();
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] data = new byte[numberOfBits];
            random.GetBytes(data);

            BigInteger startValue = new BigInteger(data);
            for (int i = 0; i < numberOfSerials; i++)
            {
                if (i == 0)
                {
                    listOfSerialNumber.Add(startValue.Add(new BigInteger(1.ToString())).Abs());
                }
                else
                {
                    listOfSerialNumber.Add((listOfSerialNumber[i - 1].Add(new BigInteger(1.ToString()))).Abs());
                }

            }

            Shuffler.Shuffle(listOfSerialNumber);
            return listOfSerialNumber;
        }

        /// <summary>
        /// Generuje pre-tokeny do późniejszego stworzenia tokenów
        /// </summary>
        /// <param name="n">liczba tokenów do wygenerowania</param>
        /// <param name="bits">rozmiar tokenu</param>
        /// <returns>zwraca listę pre-tokenów</returns>
        public static List<AsymmetricCipherKeyPair> GeneratePreTokens(int numberOfSerials, int numberOfBits)
        {
            List<AsymmetricCipherKeyPair> preTokens = new List<AsymmetricCipherKeyPair>();
            for (int i = 0; i < numberOfSerials; i++)
            {
                KeyGenerationParameters para = new KeyGenerationParameters(new SecureRandom(), numberOfBits);
                RsaKeyPairGenerator keyGen = new RsaKeyPairGenerator();
                //initialise the KeyGenerator with a random number.
                keyGen.Init(para);
                AsymmetricCipherKeyPair keypair = keyGen.GenerateKeyPair();
                preTokens.Add(keypair);
            }

            return preTokens;
        }


        /// <summary>
        /// Generacja danych do ślepego podpisu kart głosujących
        /// </summary>
        private void GeneratePermutationTokens()
        {
            this.permutationModulusList = new List<BigInteger>();
            this.permutationExponentsList = new List<BigInteger>();


            for (int i = 0; i < this.numberOfVoters; i++)
            {
                List<AsymmetricCipherKeyPair> preToken = new List<AsymmetricCipherKeyPair>(GeneratePreTokens(1, Constants.TOKEN_BIT_SIZE));

                RsaKeyParameters publicKey = (RsaKeyParameters)preToken[0].Public;
                RsaKeyParameters privKey = (RsaKeyParameters)preToken[0].Private;
                permutationModulusList.Add(publicKey.Modulus);
                permutationExponentsList.Add(publicKey.Exponent);
            }
            Console.WriteLine("Permutation tokens generated");
        }

        /// <summary>
        /// Odwracanie permutacji
        /// </summary>
        private void GenerateInversePermutation()
        {
            //using mathematics to generate inverse permutation for our List
            this.inversePermutationList = new List<List<BigInteger>>();
            for (int i = 0; i < this.numberOfVoters; i++)
            {
                this.inversePermutationList.Add(this.permutator.InversePermutation(this.permutationsList[i]));
            }
            logs.AddLog(Constants.LOG_INVERSE_GEN, Logger.LogType.Info);
            ConnectSerialNumberAndInversePermutation();

        }

        /// <summary>
        /// Połączenie SL i odwróconych permutacji 
        /// </summary>
        private void ConnectSerialNumberAndInversePermutation()
        {
            dictionarySLInversePermutation = new Dictionary<BigInteger, List<BigInteger>>();
            for (int i = 0; i < this.serialNumberList.Count; i++)
            {
                dictionarySLInversePermutation.Add(this.serialNumberList[i], this.inversePermutationList[i]);
            }
        }

        /// <summary>
        /// Tworzenie numerów seryjnych SL
        /// </summary>
        private void GenerateSerialNumber()
        {
            serialNumberList = new List<BigInteger>();
            serialNumberList = GenerateListOfSerialNumber(this.numberOfVoters, Constants.SL_BIT_SIZE);

            logs.AddLog(Constants.LOG_SL_GEN, Logger.LogType.Info);
        }

        /// <summary>
        /// Tworzenie tokenów
        /// </summary>
        private void GenerateTokens()
        {

            //preparing Big Integers for RSA blind signature (token have to fulfil requirments) 
            this.tokensList = new List<List<BigInteger>>();
            this.exponentsList = new List<List<BigInteger>>();
            this.signatureFactor = new List<List<BigInteger>>();


            for (int i = 0; i < this.numberOfVoters; i++)
            { // we use the same method like to generate serial number, there is another random generator used inside this method
                List<AsymmetricCipherKeyPair> preToken = new List<AsymmetricCipherKeyPair>(GeneratePreTokens(Constants.NUMBER_OF_TOKENS, Constants.TOKEN_BIT_SIZE));
                List<BigInteger> tokens = new List<BigInteger>();
                List<BigInteger> exps = new List<BigInteger>();
                List<BigInteger> signFactor = new List<BigInteger>();

                foreach (AsymmetricCipherKeyPair token in preToken)
                {
                    RsaKeyParameters publicKey = (RsaKeyParameters)token.Public;
                    RsaKeyParameters privKey = (RsaKeyParameters)token.Private;
                    tokens.Add(publicKey.Modulus);
                    exps.Add(publicKey.Exponent);
                    signFactor.Add(privKey.Exponent);
                }
                this.tokensList.Add(tokens);
                this.exponentsList.Add(exps);
                this.signatureFactor.Add(signFactor);
            }

            logs.AddLog(Constants.LOG_TOKEN_GEN, Logger.LogType.Info);
            ConnectSerialNumberAndTokens();

        }

        /// <summary>
        /// Połączenie SL i permutacji
        /// </summary>
        private void ConnectSerialNumberAndPermutation()
        {
            dictionarySLPermuation = new Dictionary<BigInteger, List<BigInteger>>();
            for (int i = 0; i < this.serialNumberList.Count; i++)
            {
                dictionarySLPermuation.Add(this.serialNumberList[i], this.permutationsList[i]);
            }
        }

        /// <summary>
        /// Połączenie SL oraz tokenów, eksponent i czynników (do zaślepiania i podpisywania)
        /// </summary>
        private void ConnectSerialNumberAndTokens()
        {
            this.dictionarySLTokens = new Dictionary<BigInteger, List<List<BigInteger>>>();
            for (int i = 0; i < this.serialNumberList.Count; i++)
            {
                List<List<BigInteger>> tokens = new List<List<BigInteger>>();
                tokens.Add(tokensList[i]);
                tokens.Add(exponentsList[i]);
                tokens.Add(signatureFactor[i]);
                this.dictionarySLTokens.Add(this.serialNumberList[i], tokens);
            }
        }



        /// <summary>
        /// Wysłanie SL i tokenów do Proxy
        /// </summary>
        public void SendSLAndTokensToProxy()
        {
            string msg = Constants.MSG_SL_TOKENS_FOR_PROXY + "&";
            for (int i = 0; i < this.serialNumberList.Count; i++)
            {

                msg = msg + this.serialNumberList[i].ToString() + "=";
                for (int j = 0; j < this.tokensList[i].Count; j++)
                {
                    if (j == this.tokensList[i].Count - 1)
                        msg = msg + this.tokensList[i][j].ToString() + ":";

                    else
                        msg = msg + this.tokensList[i][j].ToString() + ",";

                }

                for (int j = 0; j < this.exponentsList[i].Count; j++)
                {
                    if (j == this.exponentsList[i].Count - 1)
                        msg += this.exponentsList[i][j].ToString();

                    else
                        msg = msg + this.exponentsList[i][j].ToString() + ",";

                }

                if (i != this.serialNumberList.Count - 1)
                    msg += ";";

            }
            this.serverForProxy.SendMessage("PROXY", msg);
            this.logs.AddLog(Constants.LOG_SL_TOKENS_SENT, Logger.LogType.Info);
        }

        /// <summary>
        /// Wysyła do Votera permutowaną listę kandydatów
        /// </summary>
        /// <param name="name">nazwa (identyfikator) Votera któremu wysyłana jest lista</param>
        /// <param name="SL">numer seryjny</param>
        public void SendCandidateListPermutated(string name, BigInteger SL)
        {
            List<BigInteger> permutation = new List<BigInteger>();
            permutation = this.dictionarySLPermuation[SL];

            List<String> candidateList = new List<string>();

            for (int i = 0; i < this.candidateDefaultList.Count; i++)
            {
                int index = permutation[i].IntValue;
                candidateList.Add(candidateDefaultList[index - 1]);
            }

            string candidateListString = Constants.MSG_PERMUTATED_LIST + "&";

            for (int i = 0; i < candidateList.Count; i++)
            {
                if (i < candidateList.Count - 1)
                    candidateListString += candidateList[i] + ";";
                else
                    candidateListString += candidateList[i];
            }

            this.serverForVoters.SendMessage(name, candidateListString);
            this.logs.AddLog(Constants.LOG_CANDIDATES_SENT + name, Logger.LogType.Info);

        }

        /// <summary>
        /// Odczytanie zaślepionej karty z głosem otrzymanej od Proxy
        /// </summary>
        /// <param name="message">wiadomość od Proxy</param>
        public void ReadBlindBallotMatrix(string message)
        {
            //saving data recived from Proxy

            string[] words = message.Split(';');

            //1st parameter = name of voter
            string name = words[0];

            //2nd = SL of VOTER
            BigInteger SL = new BigInteger(words[1]);

            //Tokens
            List<BigInteger> tokenList = new List<BigInteger>();
            string[] strTokens = words[2].Split(',');
            for (int i = 0; i < strTokens.Length; i++)
            {
                tokenList.Add(new BigInteger(strTokens[i]));
            }

            //Exponent list --- obsolete
            List<BigInteger> exponentList = new List<BigInteger>();
            string[] strExpo = words[3].Split(',');
            for (int i = 0; i < strExpo.Length; i++)
            {
                exponentList.Add(new BigInteger(strExpo[i]));
            }

            //Voted colums (ballot matrix)
            BigInteger[] columns = new BigInteger[4];
            string[] strColumns = words[4].Split(',');
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new BigInteger(strColumns[i]);
            }

            this.ballots.Add(name, new Ballot(SL));

            this.ballots[name].BlindColumn = columns;
            this.ballots[name].Permutation = this.dictionarySLPermuation[SL];
            this.ballots[name].InversePermutation = this.dictionarySLInversePermutation[SL];
            this.ballots[name].TokenList = tokenList;
            this.ballots[name].ExponentsList = exponentList;
            this.ballots[name].SignatureFactor = this.dictionarySLTokens[SL][2];

            this.logs.AddLog(Constants.LOG_BLIND_MATRIX_RCV + name, Logger.LogType.Info);

            this.SendSignedColumns(name);
        }

        /// <summary>
        /// Odesłanie Proxy podpisanych kolumn zaślepionej "ballot matrix"
        /// </summary>
        /// <param name="name">nazwa (identyfikator) Votera</param>
        private void SendSignedColumns(string name)
        {
            //msg = BLIND_PROXY_BALLOT&name;signCol1,signCol2,signCol3,signCol4
            this.ballots[name].signColumn();
            string signColumns = null;

            for (int i = 0; i < this.ballots[name].SignedColumn.Length; i++)
            {
                if (i == this.ballots[name].SignedColumn.Length - 1)
                    signColumns += this.ballots[name].SignedColumn[i].ToString();
                else
                    signColumns = signColumns + this.ballots[name].SignedColumn[i].ToString() + ",";
            }

            string msg = Constants.MSG_SIGNED_FOR_PROXY + "&" + name + ";" + signColumns;
            this.serverForProxy.SendMessage("PROXY", msg);
            this.logs.AddLog(Constants.LOG_SIGNED_MATRIX_SENT, Logger.LogType.Info);
        }


        /// <summary>
        /// Odczytanie odślepionej karty z głosem otrzymanej od Proxy
        /// </summary>
        /// <param name="message">wiadomość od Proxy</param>
        public void ReadUnblindedBallotMatrix(string message)
        {
            string[] words = message.Split(';');

            string name = words[0];
            string[] strUnblinedColumns = words[1].Split(',');

            string[,] unblindedBallot = new string[this.candidateDefaultList.Count, Constants.BALLOT_SIZE];
            for (int i = 0; i < strUnblinedColumns.Length; i++)
            {
                for (int j = 0; j < strUnblinedColumns[i].Length; j++)
                {
                    unblindedBallot[j, i] = strUnblinedColumns[i][j].ToString();
                }
            }

            string[,] unblindedUnpermuatedBallot = new string[this.candidateDefaultList.Count, Constants.BALLOT_SIZE];
            BigInteger[] inversePermutation = this.ballots[name].InversePermutation.ToArray();
            for (int i = 0; i < unblindedUnpermuatedBallot.GetLength(0); i++)
            {
                string strRow = inversePermutation[i].ToString();
                int row = Convert.ToInt32(strRow) - 1;
                for (int j = 0; j < unblindedUnpermuatedBallot.GetLength(1); j++)
                {
                    unblindedUnpermuatedBallot[i, j] = unblindedBallot[row, j];
                }
            }

            this.ballots[name].UnblindedBallot = unblindedUnpermuatedBallot;
            this.logs.AddLog(Constants.LOG_UNBLINED_MATRIX_RCV, Logger.LogType.Info);
        }


        /// <summary>
        /// Wyłączenie serwerów
        /// </summary>
        public void StopServers()
        {
            this.serverForProxy.Stop();
            this.serverForVoters.Stop();
        }

        /// <summary>
        /// Zliczanie głosów
        /// </summary>
        public void CountVotes()
        {
            //counting votes

            UnblindPermutation(permutationsList); // auditor's function

            this.finalResults = new int[this.candidateDefaultList.Count];
            for (int i = 0; i < this.finalResults.Length; i++)
            {
                this.finalResults[i] = 0;
            }

            for (int i = 0; i < this.ballots.Count; i++)
            {
                int signleVote = CheckVote(i);
                if (signleVote != -1)
                {
                    this.finalResults[signleVote] += 1;
                }
            }

            this.ShowResults();
        }

        /// <summary>
        /// Przedstawienie wyników
        /// </summary>
        private void ShowResults()
        {
            int maxValue = this.finalResults.Max();
            int maxIndex = this.finalResults.ToList().IndexOf(maxValue);
            int winningCandidates = 0;
            string winners = null;
            string finalResultsString = null;
            logs.AddLog(Constants.LOG_RESULTS, Logger.LogType.Result);
            for (int i = 0; i < this.finalResults.Length; i++)
            {
                logs.AddLog("   " + this.candidateDefaultList[i] + " : " + this.finalResults[i] + " głos/y/ów", Logger.LogType.Result, false);
                finalResultsString = finalResultsString + this.candidateDefaultList[i] + " : " + this.finalResults[i] + " głos/y/ów" + Environment.NewLine;
                if (this.finalResults[i] == maxValue)
                {
                    winningCandidates += 1; // a few candidates could have same number of votes
                    winners = winners + "    " + this.candidateDefaultList[i] + Environment.NewLine;
                }
            }

            // logs.AddLog(finalResultsString, Logger.LogType.Result);
            if (winningCandidates == 1)
            {
                logs.AddLog(Constants.LOG_ONE_WINNER + Environment.NewLine + winners, Logger.LogType.Result);
            }
            else
            {
                logs.AddLog(Constants.LOG_MULTI_WINNER + Environment.NewLine + winners, Logger.LogType.Result);
            }

        }

        /// <summary>
        /// Odczytanie głosu z pojedynczej karty
        /// </summary>
        /// <param name="n">numer porządkowy karty</param>
        /// <returns>vote for candidate</returns>
        private int CheckVote(int n)
        {
            Ballot ballot = this.ballots.ElementAt(n).Value;
            string[,] vote = ballot.UnblindedBallot;
            Console.WriteLine("Voter number " + n);
            int voteCastOn = -1;
            for (int i = 0; i < vote.GetLength(0); i++)
            {
                int numberOfYes = 0;
                for (int j = 0; j < vote.GetLength(1); j++)
                {
                    if (vote[i, j] == "1")
                        numberOfYes += 1;
                }


                if (numberOfYes == 3)
                {
                    voteCastOn = i;
                    break;
                }
            }

            return voteCastOn;
        }






        //Auditor's functions

        /// <summary>
        /// Zaślepienie permutacji / zobowiązanie bitowe (RSA - bit commitment)
        /// </summary>
        /// <param name="permutationList">lista permutacji</param>
        public void BlindPermutation(List<List<BigInteger>> permutationList)
        {

            int size = permutationList.Count;
            BigInteger[] toSend = new BigInteger[size];

            //preparing List of permutation to send
            int k = 0;
            string[] strPermuationList = new string[permutationList.Count];
            foreach (List<BigInteger> list in permutationList)
            {
                string str = null;
                foreach (BigInteger big in list)
                {
                    str += big.ToString();
                }
                strPermuationList[k] = str;
                k++;
            }

            //RSA formula (bit commitment)
            int i = 0;
            foreach (string str in strPermuationList)
            {
                BigInteger toBlind = new BigInteger(str);
                BigInteger e = pubKey.Exponent;
                BigInteger n = pubKey.Modulus;
                BigInteger b = toBlind.ModPow(e, n);
                toSend[i] = b;
                i++;
            }
            this.auditor.CommitedPermatation = toSend;
        }

        /// <summary>
        /// Odślepienie permutacji, sprawdzenie RSA
        /// </summary>
        /// <param name="permutationList">lista permutacji</param>
        public void UnblindPermutation(List<List<BigInteger>> permutationList)
        {
            int size = permutationList.Count;
            BigInteger[] toSend = new BigInteger[size];

            int k = 0;
            string[] strPermuationList = new string[permutationList.Count];

            foreach (List<BigInteger> list in permutationList)
            {
                string str = null;
                foreach (BigInteger big in list)
                {
                    str += big.ToString();
                }
                strPermuationList[k] = str;
                k++;
            }

            int i = 0;
            foreach (string str in strPermuationList)
            {
                BigInteger b = new BigInteger(str);
                toSend[i] = b;
                i++;
            }


            //checking permutations RSA (auditor checks all of the permutations)
            if (this.auditor.checkPermutation(this.privKey, this.pubKey, toSend))
            {
                logs.AddLog(Constants.AUDITOR_COMMITMENT_TRUE, Logger.LogType.Auditor);
            }
            else
            {
                logs.AddLog(Constants.AUDITOR_COMMITMENT_FALSE, Logger.LogType.Auditor);
            }
        }
    }
}

