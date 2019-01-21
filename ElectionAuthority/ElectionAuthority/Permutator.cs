using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;
using Org.BouncyCastle.Math;

namespace ElectionAuthority
{
    /// <summary>
    /// Wykonuje permutacje listy kandydatów
    /// </summary>
    class Permutator
    {
        /// <summary>
        /// Wyświetla logi w polu dziennika logów
        /// </summary>
        private Logger logs;

        /// <summary>
        /// Konstruktor klasy Permutator
        /// </summary>
        /// <param name="logs">logs instance</param>
        public Permutator(Logger logs)
        {
            this.logs = logs;
        }

        /// <summary>
        /// Wykonanie pojedynczej permutacji
        /// </summary>
        /// <param name="n">długość permutacji</param>
        /// <returns>zwraca listę liczb (wygenerowaną permutację)</returns>
        public List<BigInteger> PermutateOnce(int n)
        {
            List<BigInteger> permutation = new List<BigInteger>();
            for (int i = 1; i <= n; i++)
            {
                permutation.Add(new BigInteger(i.ToString()));
            }

            Shuffler.Shuffle(permutation);
            return permutation;
        }

        /// <summary>
        /// Stworzenie macierzy permutacji do późniejszego wykorzystania przy jej odwracaniu
        /// </summary>
        /// <param name="permutation">permutation</param>
        /// <returns></returns>
        public int[,] CreateMatrix(List<BigInteger> permutation)
        {
            /*
             Example:
             
            start        |  1  |  2  |  3  |  4  |  5  |
            -------------|-----|-----|-----|-----|-----|
            permutation  |  3  |  2  |  5  |  1  |  4  |             

            matrix
            
                      0  |  0  |  1  |  0  |  0
                    -----|-----|-----|-----|-----
                      0  |  1  |  0  |  0  |  0
                    -----|-----|-----|-----|-----
                      0  |  0  |  0  |  0  |  1
                    -----|-----|-----|-----|-----
                      1  |  0  |  0  |  0  |  0
                    -----|-----|-----|-----|-----
                      0  |  0  |  0  |  1  |  0
             */


            int n = permutation.Count;
            int[,] matrix = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = 0;
                }

                matrix[i, permutation[i].IntValue - 1] = 1;
            }

            return matrix;
        }

        /// <summary>
        /// Transponowanie macierzy do wykorzystania przy odwracaniu permutacji 
        /// </summary>
        /// <param name="m">macierz permutacji</param>
        /// <returns>zwraca transponowaną macierz permutacji</returns>
        public int[,] TransposeMatrix(int[,] m)
        {
            int[,] transposed = new int[m.GetLength(0), m.GetLength(1)];
            for (int i = 0; i < m.GetLength(0); i++)
                for (int j = 0; j < m.GetLength(0); j++)
                    transposed[j, i] = m[i, j];
            return transposed;
        }

        /// <summary>
        /// Odwracanie permutacji
        /// </summary>
        /// <param name="permutation">odwracana permutacja</param>
        /// <returns>zwraca odwróconą permutację</returns>
        public List<BigInteger> InversePermutation(List<BigInteger> permutation)
        {
            int[,] matrix = CreateMatrix(permutation);
            int[,] matrixTr = TransposeMatrix(matrix);
            List<BigInteger> inversePermutation = new List<BigInteger>();

            for (int i = 0; i < matrixTr.GetLength(1); i++)
            {
                for (int j = 0; j < matrixTr.GetLength(0); j++)
                {
                    if (matrixTr[i, j] == 1)
                    {
                        inversePermutation.Add(new BigInteger((j + 1).ToString()));
                    }
                }
            }
            return inversePermutation;
        }
    }
}
