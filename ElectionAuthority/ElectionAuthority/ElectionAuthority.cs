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
        /// Kodowanie do komunikacji sieciowej
        /// </summary>
        ASCIIEncoding encoder;

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
        private List<String> candidateList;

        /// <summary>
        /// Konfiguracja (wczytywana z pliku)
        /// </summary>
        private Config configuration;

        /// <summary>
        /// Permutajca PI
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
        /// Lista tokenów na potrzeby zaślepiania (4 tokeny na jeden SL)
        /// </summary>
        private List<List<BigInteger>> tokensList;

        /// <summary>
        /// Lista eksponent na potrzeby zaślepiania
        /// </summary>
        private List<List<BigInteger>> exponentsList;

        /// <summary>
        /// Lista czynników na potrzeby podpisywania
        /// </summary>
        private List<List<BigInteger>> signatureFactors;

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
        /// Zbiór kart do głosowania
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
        /// Konstruktor EA
        /// </summary>
        /// <param name="logs">do zapisywania logów</param>
        /// <param name="configuration">do wczytania konfiguracji</param>
        /// <param name="form">okno aplikacji do sterowania przyciskami</param>
        public ElectionAuthority(Logger logs, Config configuration, Form1 form)
        {
            this.encoder = new ASCIIEncoding();
            this.logs = logs;
            this.configuration = configuration;
            this.form = form;

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
        /// Generuje numery seryjne SL
        /// </summary>
        /// <param name="n">liczba SL do wygenerowania/param>
        /// <param name="bytes">liczba bajtów w pojedynczym SL</param>
        /// <returns>list of serial numbers</returns>
        public static List<BigInteger> GenerateListOfSerialNumber(int n, int bytes)
        {

            List<BigInteger> listOfSerialNumber = new List<BigInteger>();
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] data = new byte[bytes];
            random.GetBytes(data);

            BigInteger startValue = new BigInteger(data);
            for (int i = 0; i < n; i++)
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
        /// Tworzenie permutacji list kandydatów
        /// </summary>
        private void Permutate()
        {
            permutationsList = new List<List<BigInteger>>();
            for (int i = 0; i < this.numberOfVoters; i++)
            {
                this.permutationsList.Add(new List<BigInteger>(this.permutator.PermutateOnce(candidateList.Count)));
            }

            ConnectSerialNumbersAndPermutations();
            GenerateInversePermutations();
            //GeneratePermutationTokens();
            BlindPermutation(permutationsList);              // Send commited permutation to Auditor (???)
            logs.AddLog(Constants.LOG_PERMUTATION_GEN, Logger.LogType.Info);
        }


        /// <summary>
        /// Odwracanie wszystkich permutacji
        /// </summary>
        private void GenerateInversePermutations()
        {
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
        private void generateSerialNumber()
        {
            serialNumberList = new List<BigInteger>();
            serialNumberList = GenerateListOfSerialNumber(this.numberOfVoters, Constants.SL_BIT_SIZE);

            logs.AddLog(Constants.LOG_SL_GEN, Logger.LogType.Info);
        }

        /// <summary>
        /// Generuje pre-tokeny do późniejszego stworzenia tokenów potwierdzenia
        /// </summary>
        /// <param name="n">liczba tokenów do wygenerowania</param>
        /// <param name="bits">rozmiar tokenu</param>
        /// <returns>list of pre tokens</returns>
        public static List<AsymmetricCipherKeyPair> GeneratePreTokens(int n, int bits)
        {
            List<AsymmetricCipherKeyPair> preTokens = new List<AsymmetricCipherKeyPair>();
            for (int i = 0; i < n; i++)
            {
                KeyGenerationParameters para = new KeyGenerationParameters(new SecureRandom(), bits);
                //generate the RSA key pair
                RsaKeyPairGenerator keyGen = new RsaKeyPairGenerator();
                //initialise the KeyGenerator with a random number.
                keyGen.Init(para);
                AsymmetricCipherKeyPair keypair = keyGen.GenerateKeyPair();
                preTokens.Add(keypair);
            }

            return preTokens;
        }

        /// <summary>
        /// Tworzenie tokenów
        /// </summary>
        private void GenerateTokens()
        {
            this.tokensList = new List<List<BigInteger>>();
            this.exponentsList = new List<List<BigInteger>>();
            this.signatureFactors = new List<List<BigInteger>>();

            for (int i = 0; i < this.numberOfVoters; i++)
            {
                List<AsymmetricCipherKeyPair> preToken = new List<AsymmetricCipherKeyPair>(GeneratePreTokens(4, Constants.TOKEN_BIT_SIZE));
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
                this.signatureFactors.Add(signFactor);
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
        /// connects serial number and tokens
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

            logs.addLog(Constants.SL_CONNECTED_WITH_TOKENS, true, Constants.LOG_INFO);
        }

        /// <summary>
        /// generates data for voting (serial numbers, tokens, permutations)
        /// </summary>
        public void generateDate()
        {
            generateSerialNumber();
            generateTokens();
            generatePermutation();

        }

        /// <summary>
        /// Sends SL and Tokens to proxy
        /// </summary>
        public void sendSLAndTokensToProxy()
        {
            //before sending we have to convert dictionary to string. We use our own conversion to recoginize message in proxy and reparse it to dictionary

            string msg = Constants.SL_TOKENS + "&";
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
            this.serverProxy.sendMessage(Constants.PROXY, msg);
        }

        /// <summary>
        /// disable button causes sending tokens an SLs
        /// </summary>
        public void disableSendSLTokensAndTokensButton()
        {
            this.form.Invoke(new MethodInvoker(delegate ()
            {
                this.form.disableSendSLTokensAndTokensButton();
            }));

        }

        /// <summary>
        /// permutes candidate list for concrete voter and for his/her SL number
        /// </summary>
        /// <param name="name"></param>
        /// <param name="SL"></param>
        public void getCandidateListPermuated(string name, BigInteger SL)
        {
            List<BigInteger> permutation = new List<BigInteger>();
            permutation = this.dictionarySLPermuation[SL];

            List<String> candidateList = new List<string>();

            for (int i = 0; i < this.candidateDefaultList.Count; i++)
            {
                int index = permutation[i].IntValue;
                candidateList.Add(candidateDefaultList[index - 1]);
            }

            string candidateListString = Constants.CANDIDATE_LIST_RESPONSE + "&";

            for (int i = 0; i < candidateList.Count; i++)
            {
                if (i < candidateList.Count - 1)
                    candidateListString += candidateList[i] + ";";
                else
                    candidateListString += candidateList[i];
            }

            this.serverClient.sendMessage(name, candidateListString);

        }

        /// <summary>
        /// saves blind ballot matrix recived from proxy
        /// </summary>
        /// <param name="message"></param>
        public void saveBlindBallotMatrix(string message)
        {
            //saving data recived from Proxy

            string[] words = message.Split(';');

            //1st parameter = name of voter
            string name = words[0];

            //2nd = SL of VOTER
            BigInteger SL = new BigInteger(words[1]);

            //Then tokens
            List<BigInteger> tokenList = new List<BigInteger>();
            string[] strTokens = words[2].Split(',');
            for (int i = 0; i < strTokens.Length; i++)
            {
                tokenList.Add(new BigInteger(strTokens[i]));
            }

            //Exponent list (used for blind signature)
            List<BigInteger> exponentList = new List<BigInteger>();
            string[] strExpo = words[3].Split(',');
            for (int i = 0; i < strExpo.Length; i++)
            {
                exponentList.Add(new BigInteger(strExpo[i]));
            }

            //and at least voted colums
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

            this.logs.addLog(Constants.BLIND_PROXY_BALLOT_RECEIVED + name, true, Constants.LOG_INFO, true);

            this.signColumn(name);
        }

        /// <summary>
        /// Signs columns (EA signature)
        /// </summary>
        /// <param name="name">Voter name</param>
        private void signColumn(string name)
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

            string msg = Constants.SIGNED_PROXY_BALLOT + "&" + name + ";" + signColumns;
            this.serverProxy.sendMessage(Constants.PROXY, msg);
            this.logs.addLog(Constants.SIGNED_BALLOT_MATRIX_SENT, true, Constants.LOG_INFO, true);
        }


        /// <summary>
        /// saves unblinded ballot matrix (vote)
        /// </summary>
        /// <param name="message">string message recived from proxy</param>
        public void saveUnblindedBallotMatrix(string message)
        {
            //the same as previous saving
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
            this.logs.addLog(Constants.UNBLINED_BALLOT_MATRIX_RECEIVED, true, Constants.LOG_INFO, true);
        }


        /// <summary>
        /// disables proxy 
        /// </summary>
        public void disbaleProxy()
        {
            try
            {
                this.serverProxy.stopServer();
            }
            catch (Exception)
            {
                this.logs.addLog(Constants.UNABLE_TO_STOP_VOTING, true, Constants.LOG_ERROR, true);
            }

            this.logs.addLog(Constants.VOTIGN_STOPPED, true, Constants.LOG_INFO, true);

        }

        /// <summary>
        /// counting votes EA send to voter unblinded permutation (and then private key) so Audiotr
        ///        can check RSA formula
        /// </summary>
        public void countVotes()
        {
            //counting votes

            /*ublindPermutation - EA send to voter unblinded permutation (and then private key) so Audiotr
                can check RSA formula*/
            unblindPermutation(permutationsList);

            this.finalResults = new int[this.candidateDefaultList.Count];
            initializeFinalResults();

            for (int i = 0; i < this.ballots.Count; i++)
            {
                int signleVote = checkVote(i);
                if (signleVote != -1)
                {
                    this.finalResults[signleVote] += 1;
                }
            }

            this.announceResultsOfElection();
        }

        /// <summary>
        /// announce results of election
        /// </summary>
        private void announceResultsOfElection()
        {

            int maxValue = this.finalResults.Max();
            int maxIndex = this.finalResults.ToList().IndexOf(maxValue);
            int winningCandidates = 0;
            string winners = null;
            string resultOfVoting = null;
            for (int i = 0; i < this.finalResults.Length; i++)
            {
                resultOfVoting = resultOfVoting + this.candidateDefaultList[i] + " received: " + this.finalResults[i] + " votes" + Environment.NewLine;
                if (this.finalResults[i] == maxValue)
                {
                    winningCandidates += 1; // a few candidates has the same number of votes.
                    winners = winners + this.candidateDefaultList[i] + " ";
                }
            }

            if (winningCandidates == 1)
            {
                this.form.Invoke(new MethodInvoker(delegate ()
                {

                    MessageBox.Show(resultOfVoting + "Winner of the election is: " + winners);
                }));

            }
            else
            {
                this.form.Invoke(new MethodInvoker(delegate ()
                {

                    MessageBox.Show(resultOfVoting + "There is no one winner. Candidates on first place ex aequo: " + winners);
                }));

            }

        }

        /// <summary>
        /// count vote from one voter
        /// </summary>
        /// <param name="voterNumber">voter number in voters array</param>
        /// <returns>vote for candidate</returns>
        private int checkVote(int voterNumber)
        {
            Ballot ballot = this.ballots.ElementAt(voterNumber).Value;
            string[,] vote = ballot.UnblindedBallot;
            Console.WriteLine("Voter number " + voterNumber);
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

        /// <summary>
        /// initialize finale results
        /// </summary>
        private void initializeFinalResults()
        {
            for (int i = 0; i < this.finalResults.Length; i++)
            {
                this.finalResults[i] = 0;
            }
        }



        //Auditor's functions

        /// <summary>
        /// blinds permutations (all of them), RSA formula (bit commitment)
        /// </summary>
        /// <param name="permutationList">permutation list</param>
        public void blindPermutation(List<List<BigInteger>> permutationList)
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
        /// unblind permutation, checking permutations RSA (auditor checks all of the permutations)
        /// </summary>
        /// <param name="permutationList">permutation list</param>
        public void unblindPermutation(List<List<BigInteger>> permutationList)
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
                logs.addLog(Constants.BIT_COMMITMENT_OK, true, Constants.LOG_INFO, true);
            }
            else
            {
                logs.addLog(Constants.BIT_COMMITMENT_FAIL, true, Constants.LOG_ERROR, true);
            }
        }
    }
}
