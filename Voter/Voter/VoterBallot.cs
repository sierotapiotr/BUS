using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Math;

namespace Voter
{
    /// <summary>
    /// Klasa reprezentujaca kartę do głosowania z dodatkowymi danymi
    /// </summary>
    class VoterBallot
    {
        /// <summary>
        /// Liczba kandydatów
        /// </summary>
        private int numberOfCandidates;

        /// <summary>
        /// Liczba głosujących
        /// </summary>
        private int numberOfVotes;

        /// <summary>
        /// Głosy zapisane w tablicy
        /// </summary>
        private int[,] voted;

        /// <summary>
        /// numer seryjny SL
        /// </summary>
        private BigInteger sl;

        /// <summary>
        /// numer seryjny SR
        /// </summary>
        private BigInteger sr;

        /// <summary>
        /// token
        /// </summary>
        private BigInteger token;

        /// <summary>
        /// podpisana na ślepo kolumna
        /// </summary>
        private BigInteger signedBlindColumn;

        // GETTERS & SETTERS
        public int[,] Voted
        {
            get { return voted; }
        }

        public BigInteger SL
        {
            set { sl = value; }
            get { return sl; }
        }

        public BigInteger SR
        {
            set { sr = value; }
            get { return sr; }
        }

        public BigInteger Token
        {
            set { token = value; }
            get { return token; }
        }

        public BigInteger SignedBlindColumn
        {
            set { signedBlindColumn = value; }
            get { return signedBlindColumn; }
        }


        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="numberOfCand"></param>
        public VoterBallot(int numberOfCand)
        {
            numberOfCandidates = numberOfCand;
            numberOfVotes = 0;
            voted = new int[numberOfCand, Constants.BALLOTSIZE];
        }


        /// <summary>
        /// Metoda służąca oddaniu głosu
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Vote(int x, int y)
        {
            if (VoteInRowDone(x, y))
            {
                return false;
            }
            else
            {
                voted[x, y] = 1;
                numberOfVotes += 1;
                return true;
            }
        }


        /// <summary>
        /// Metoda sprawdza oddanie głosu w danym rzędzie
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool VoteInRowDone(int x, int y)
        {
            for (int i = 0; i < Constants.BALLOTSIZE; i++)
            {
                if (voted[x, i] != 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Metoda sprawdza całkowite oddanie głosu
        /// </summary>
        /// <returns></returns>
        public bool VoteDone()
        {
            if (numberOfVotes == this.numberOfCandidates)
            {
                return true;
            }
            else
                return false;
        }

    }
}
