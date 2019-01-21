using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Org.BouncyCastle.Math;

namespace Proxy
{
    /// <summary>
    /// Generator numerów seryjnych
    /// </summary>
    class SerialNumberGenerator
    {
        /// <summary>
        /// Instancja sng (singleton)
        /// </summary>
        private SerialNumberGenerator sng = null;

        /// <summary>
        /// Lista numerów seryjnych
        /// </summary>
        private List<BigInteger> listOfSerialNumbers;

        /// <summary>
        /// Licznik numerów seryjnych
        /// </summary>
        int counter = 0;

        /// <summary>
        /// Prywatny konstruktor domyślny (singleton)
        /// </summary>
        private SerialNumberGenerator(){}


        /// <summary>
        /// Prywatny (singleton) konstruktor klasy SerialNumberGeneratorSerialNumberGenerator
        /// </summary>
        /// <param name="numberOfSerials">number of serials to generate</param>
        /// <param name="numberOfBits">bit size of serial</param>
        /// <returns>list of serial numbers</returns>
        private SerialNumberGenerator(int numberOfSerials, int numberOfBits)
        {
            listOfSerialNumbers = new List<BigInteger>();
            Random random = new Random();
            byte[] data = new byte[numberOfBits];
            random.NextBytes(data);

            BigInteger startValue = new BigInteger(data);

            for (int i = 0; i < numberOfSerials; i++)
            {
                if (i == 0)
                {
                    listOfSerialNumbers.Add(startValue.Add(new BigInteger("1")));
                }
                else
                {
                    listOfSerialNumbers.Add(listOfSerialNumbers[i - 1].Add(new BigInteger("1")));
                }

            }

            Shuffler.Shuffle(listOfSerialNumbers);
        }

        /// <summary>
        /// Funkcja zwracająca instancję SerialNumberGenerator (singleton)
        /// </summary>
        public SerialNumberGenerator GetInstance()
        {
            if (sng == null)
            {
                sng = new SerialNumberGenerator(Constants.NUM_OF_CANDIDATES, Constants.NUMBER_OF_BITS_SR);
            }
            return sng;
        }

        /// <summary>
        /// Przekazanie nowego numeru SR
        /// </summary>
        /// <returns>SR</returns>
        public BigInteger getNextSr()
        {
            BigInteger nextSr = listOfSerialNumbers[counter];
            counter++;

            return nextSr;
        }

        /// <summary>
        /// Tworzenie nowych numeró seryjnych
        /// </summary>
        /// <param name="numberOfSerials">quantity of serial numbers</param>
        /// <param name="numberOfBits">bit length of serial number</param>
        /// <returns>list of shuffled serial numbers</returns>
        public static List<BigInteger> generateListOfSerialNumber(int numberOfSerials, int numberOfBits)
        {

            List<BigInteger> listOfSerialNumber = new List<BigInteger>();
            //Random random = new Random();
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] data = new byte[numberOfBits];
            random.GetBytes(data);

            BigInteger startValue = new BigInteger(data);
            for (int i = 0; i < numberOfSerials; i++)
            {
                if (i == 0)
                {
                    listOfSerialNumber.Add(startValue.Add(new BigInteger("1")));
                }
                else 
                {
                    listOfSerialNumber.Add(listOfSerialNumber[i - 1].Add(new BigInteger("1")));
                }

            }

            Shuffler.Shuffle(listOfSerialNumber);
            return listOfSerialNumber;
        }

        /// <summary>
        /// Przekazanie pozycji "tak" i "nie"
        /// </summary>
        /// <param name="numberOfVoters">quantity of voters</param>
        /// <param name="numberOfCandidates">quantity of candidates></param>
        /// <returns>list of yes no positions</returns>
        public static List<string> getYesNoPosition(int numberOfVoters, int numberOfCandidates)
        {
            Random rnd = new Random();
            List<string> list = new List<string>();
            int range = 4;
            for (int k = 0; k < numberOfVoters; k++)
            {
                string str = null;
                for (int i = 0; i < numberOfCandidates; i++)
                {
                    int random = rnd.Next(0, range);
                    if (i != numberOfCandidates - 1) //string looks like "number:number:number:number". 
                                        //It will be easy to split
                        str = str + random.ToString() + ":";
                    else
                        str += random.ToString();
                }
                list.Add(str);
            }
            
            return list;
        }
    }
}
