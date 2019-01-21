using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace ElectionAuthority
{
    /// <summary>
    /// Sprawdza uczciwość EA (zobowiązanie bitowe i podpis)
    /// </summary>
    class Auditor
    {

        /// <summary>
        /// Wyświetla logi w polu dziennika logów
        /// </summary>
        private Logger logs;

        /// <summary>
        /// Zobowiązanie bitowe otrzymane od EA
        /// </summary>
        private BigInteger[] commitedPermutation;
        public BigInteger[] CommitedPermatation
        {
            set { commitedPermutation = value; }
            get { return commitedPermutation; }
        }

        /// <summary>
        /// Kontruktor klasy Auditor
        /// </summary>
        /// <param name="logs">do zapisywania logów</param>
        public Auditor(Logger logs)
        {
            this.logs = logs;
        }

        /// <summary>
        /// Sprawdzanie poprawności zobowiązania
        /// </summary>
        /// <param name="privateKey">użyty klucz prywatny</param>
        /// <param name="publicKey">użyty klucz publiczny</param>
        /// <param name="explicitPermutation">użyta permutacja (tekst jawny)</param>
        /// <returns></returns>
        public bool checkPermutation(RsaKeyParameters privateKey, RsaKeyParameters publicKey, BigInteger[] explicitPermutation)
        {
            int i = 0;
            foreach (BigInteger partPermutation in explicitPermutation)
            {

                // verify using RSA formula
                if (!partPermutation.Equals(commitedPermutation[i].ModPow(privateKey.Exponent, publicKey.Modulus)))
                {
                    return false;
                }
                i++;

            }

            return true;
        }
    }
}
