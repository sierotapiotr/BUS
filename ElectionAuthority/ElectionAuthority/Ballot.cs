using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace ElectionAuthority
{
    class Ballot
    {
        /// <summary>
        /// Identyfikator SL
        /// </summary>
        private BigInteger sl;
        public BigInteger SL
        {
            get { return sl; }
        }

        /// <summary>
        /// Tokeny do ślepego podpisu
        /// </summary>
        private List<BigInteger> tokenList;
        public List<BigInteger> TokenList
        {
            get { return tokenList; }
            set { tokenList = value; }
        }

        /// <summary>
        /// Wykładniki potęgi do ślepego podpisu
        /// </summary>
        private List<BigInteger> exponentsList;
        public List<BigInteger> ExponentsList
        {
            get { return exponentsList; }
            set { exponentsList = value; }
        }

        /// <summary>
        /// every ballot has its own signature factor (such as tokens) - private key's exponent
        /// </summary>
        private List<BigInteger> signatureFactor;
        public List<BigInteger> SignatureFactor
        {
            get { return signatureFactor; }
            set { signatureFactor = value; }
        }

        /// <summary>
        /// Podpisane kolumny
        /// </summary>
        private BigInteger[] signedColumn;
        public BigInteger[] SignedColumn
        {
            get { return signedColumn; }
        }

        /// <summary>
        /// Zaślepione kolumny od Proxy
        /// </summary>
        private BigInteger[] blindColumn;
        public BigInteger[] BlindColumn
        {
            set { blindColumn = value; }
        }

        /// <summary>
        /// Odślepiona karta do podpisu
        /// </summary>
        private string[,] unblindedBallot;
        public string[,] UnblindedBallot
        {
            set { unblindedBallot = value; }
            get { return unblindedBallot; }
        }

        /// <summary>
        /// Permutacje danej karty, powiązane z jej SL
        /// </summary>
        private List<BigInteger> permutation;
        public List<BigInteger> Permutation
        {
            set { permutation = value; }
            get { return permutation; }
        }

        /// <summary>
        /// Odwrócone permutacje
        /// </summary>
        private List<BigInteger> inversePermutation;
        public List<BigInteger> InversePermutation
        {
            set { inversePermutation = value; }
            get { return inversePermutation; }
        }

        /// <summary>
        /// Konstruktor klasy Ballot
        /// </summary>
        /// <param name="SL">identyfikator SL</param>
        public Ballot(BigInteger SL)
        {
            this.sl = SL;

        }


        /// <summary>
        /// Ślepy podpis kolumnt
        /// </summary>
        public void SignColumn()
        {
            BigInteger[] signed = new BigInteger[Constants.BALLOT_SIZE];
            int i = 0;
            foreach (BigInteger column in blindColumn)
            {
                signed[i] = column.ModPow(signatureFactor[i], tokenList[i]);
                i++;
            }

            this.signedColumn = signed;
        }
    }
}
