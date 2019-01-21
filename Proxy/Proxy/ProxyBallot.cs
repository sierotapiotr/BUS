using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Digests;

namespace Proxy
{
    /// <summary>
    /// Pojedyncza karta do głosowania
    /// </summary>
    class ProxyBallot
    {
        /// <summary>
        /// Wyświetla logi w polu dziennika logów
        /// </summary>
        private Logs logs;                                              

        /// <summary>
        /// Klucz publiczny do ślepego podpisu
        /// </summary>
        private RsaKeyParameters pubKey;                                
    
        /// <summary>
        /// Klucz prywatny do ślepego podpisu
        /// </summary>
        private RsaKeyParameters privKey;                             

        /// <summary>
        /// Czynnik do zaślepiania
        /// </summary>
        private BigInteger[] r;    
        
        /// <summary>
        /// Numer seryjny SL (od EA)
        /// </summary>
        private BigInteger sl;
        public BigInteger SL
        {
            get { return sl; }
        }
        /// <summary>
        /// Numer seryjny SR
        /// </summary>
        private BigInteger sr;

        /// <summary>
        /// Pozycje "tak" i "nie"
        /// </summary>
        private string yesNoPos;                                          
        public string YesNoPos
        {
            set { yesNoPos = value; }
        }
        
        /// <summary>
        /// Głos od Votera
        /// </summary>
        private int[,] vote;                                            
        public int[,] Vote
        {
            set { vote = value; }
        }
        
        /// <summary>
        /// Macierz "ballot matrix"
        /// </summary>
        private int[,] ballotMatrix;    
        
        /// <summary>
        /// Zapis kolumn w formacie string
        /// </summary>                
        private List<string> columns;

        /// <summary>
        /// Podpisane kolumny od EA
        /// </summary>
        private List<BigInteger> signedColumns;
        public List<BigInteger> SignedColumns
        {
            set { signedColumns = value; }
            get { return signedColumns; }

        }

        /// <summary>
        /// Potwierdzenie wybrane przez Votera
        /// </summary>
        private int confirmationColumn;
        public int ConfirmationColumn
        {
            get { return confirmationColumn; }
            set { confirmationColumn = value; }
        }
        
        /// <summary>
        /// Lista tokenów
        /// </summary>
        private List<BigInteger> tokensList;
        public List<BigInteger> TokensList
        {
            set { tokensList = value; }
            get { return tokensList; }
        }

        /// <summary>
        /// Lista eksponent do odślepiania/sprawdzenia podpisu (od EA)
        /// </summary>
        private List<BigInteger> exponentsList;
        public List<BigInteger> ExponentsList
        {
            set { exponentsList = value; }
            get { return exponentsList; }
        }

        /// <summary>
        /// Konstruktor klasy ProxyBallot
        /// </summary>
        /// <param name="logs">dziennik logów</param>
        /// <param name="SL">numer seryjny SL</param>
        /// <param name="SR">numer seryjny SR</param>
        public ProxyBallot(Logs logs, BigInteger SL, BigInteger SR)
        {
            this.sl =  SL;
            this.sr = SR;
            this.logs = logs;
            this.tokensList = new List<BigInteger>();
            this.exponentsList = new List<BigInteger>();
            
            //init keyPair generator
            KeyGenerationParameters para = new KeyGenerationParameters(new SecureRandom(), 1024);
            RsaKeyPairGenerator keyGen = new RsaKeyPairGenerator();
            keyGen.Init(para);
            r = null;
            
            //generate key pair and get keys
            AsymmetricCipherKeyPair keypair = keyGen.GenerateKeyPair();
            privKey = (RsaKeyParameters)keypair.Private;
            pubKey = (RsaKeyParameters)keypair.Public;

        }

        
        /// <summary>
        /// Zaślepienie danych przed wysłaniem do EA
        /// </summary>
        /// <returns>blinded columns </returns>
        public BigInteger[] BlindDataToSend()
        {
            BigInteger[] toSend = new BigInteger[Constants.BALLOT_SIZE];
            r = new BigInteger[Constants.BALLOT_SIZE];
            //blinding columns, prepare to signature

            int i=0;
            foreach (string column in columns)
            {
                BigInteger toBlind = new BigInteger(column);
                BigInteger e = pubKey.Exponent;
                BigInteger d = privKey.Exponent;

                SecureRandom random = new SecureRandom();
                byte[] randomBytes = new byte[10];
                
                //BigInteger n = pubKey.Modulus;
                BigInteger n = tokensList[i];
                BigInteger gcd = null;
                BigInteger one = new BigInteger("1");

                //check that gcd(r,n) = 1 && r < n && r > 1
                do
                {
                    random.NextBytes(randomBytes);
                    r[i] = new BigInteger(1, randomBytes);
                    gcd = r[i].Gcd(n);
                    Console.WriteLine("gcd: " + gcd);
                }
                while (!gcd.Equals(one) || r[i].CompareTo(n) >= 0 || r[i].CompareTo(one) <= 0);

                //********************* BLIND ************************************
                BigInteger b = ((r[i].ModPow(e, n)).Multiply(toBlind)).Mod(n);
                toSend[i] = b;

                i++;
            }
            return toSend;
        }

        /// <summary>
        /// Odślepienie danych (dla jednej karty)
        /// </summary>
        /// <param name="signedData">podpisane dane od EA</param>
        public string[] UnblindSignedData(BigInteger[] signedData)
        {
            string[] unblinded = new string[Constants.BALLOT_SIZE];

            for (int i = 0; i < signedData.Length; i++)
            {
                BigInteger explicitData = new BigInteger(columns[i]);
                BigInteger n = tokensList[i];
                BigInteger e = exponentsList[i];

                BigInteger signed = ((r[i].ModInverse(n)).Multiply(signedData[i])).Mod(n);
                
                BigInteger check = signed.ModPow(e, n);
                int correctUnblindedColumns = 0; //used for knowing if all columns are unblinded correctly
                if(explicitData.Equals(check))
                {
                    correctUnblindedColumns += 1;
                    String str = check.ToString();
                    String correctString =  CheckZeros(str);
                    unblinded[i] = correctString;

                    if (correctUnblindedColumns == Constants.BALLOT_SIZE)
                        this.logs.AddLog(Constants.ALL_COLUMNS_UNBLINDED_CORRECTLY, Logs.LogType.Info);
                }
                else{
                    this.logs.AddLog(Constants.WRONG_SIGNATURE, Logs.LogType.Error);
                }
            }
            return unblinded;
        }

        /// <summary>
        /// Dopełnienie początku głosu zerami (zjadanymi przez konwersję string na BigInteger i z powrotem)
        /// </summary>
        /// <param name="str">początkowy głos</param>
        /// <returns>uzupełniony głos</returns>
        private string CheckZeros(string str)
        {
            if (str.Length == this.vote.GetLength(0))
                return str;
            else
            {
                int neccessaryZeros = this.vote.GetLength(0) - str.Length;
                string zeros = null;
                for (int i = 0; i < neccessaryZeros; i++)
                {
                    zeros += "0";
                }
                string column = zeros + str;

                return column;
            }       
        }


        /// <summary>
        /// Tworzenie "ballot matrix" z podziałem na kolumny
        /// </summary>
        public void GenerateAndSplitBallotMatrix()
        {
            
            string[] position = this.yesNoPos.Split(':');
            this.ballotMatrix = new int[this.vote.GetLength(0), this.vote.GetLength(1)];
            this.columns = new List<string>();
            for (int i = 0; i < Constants.NUM_OF_CANDIDATES; i++)
            {
                for (int j = 0; j < Constants.BALLOT_SIZE; j++)
                {
                    //mark every non-clicked "No" button
                    if (vote[i, j] != 1 && j != Convert.ToInt32(position[i]))
                    {
                        this.ballotMatrix[i, j] = 1;
                    }
                }
            }

            //rewrite colums from ballot matrix to another array
            for (int j = 0; j < Constants.BALLOT_SIZE; j++)
            {
                string temp = null;
                for (int i = 0; i < Constants.NUM_OF_CANDIDATES; i++)
                {
                    temp += ballotMatrix[i, j];
                }
                columns.Add(temp);
            }
        }
    }
}
