﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Org.BouncyCastle.Math;

namespace ElectionAuthority
{
    /// <summary>
    /// represents all permutation's method
    /// </summary>
    class Permutation
    {
        /// <summary>
        /// allows to collect and display logs
        /// </summary>
        private Logs logs;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logs">logs instance</param>
        public Permutation(Logs logs)
        {
            this.logs = logs;
        }

        /// <summary>
        /// generate ONE permutation
        /// </summary>
        /// <param name="candidateQuantity">quantity of candidates</param>
        /// <returns></returns>
        public List<BigInteger> generatePermutation(int candidateQuantity)
        {
            List<BigInteger> permutation = new List<BigInteger>();
            for (int i = 1; i <= candidateQuantity; i++)
            {
                permutation.Add(new BigInteger(i.ToString()));
            }
            //shuffle BigInt List
            Extentions.Shuffle(permutation);
            return permutation;
        }

        //we create a permutation matrix in way described on "http://pl.wikipedia.org/wiki/Permutacja"
        /// <summary>
        /// Generate permutation martix, using hamming posistion to represent one integer
        /// </summary>
        /// <param name="permutation">permutation</param>
        /// <returns></returns>
        private int[,] generatePermutationMatrix(List<BigInteger> permutation)
        {
            int candidateQuantity = permutation.Count;
            //Function allow us to get a reverse permutation. I follow the method shown at: "http://pl.wikipedia.org/wiki/Permutacja"
            int[,] tab = new int[candidateQuantity, candidateQuantity];
            int[] defaultList = new int[candidateQuantity];
            //prepare default sequence = {1....m}
            for (int i = 0; i < candidateQuantity; i++)
            {
                defaultList[i] = i + 1;
            }
            //prepare default matrix with 0 
            for (int i = 0; i < candidateQuantity; i++)
            {
                for (int j = 0; j < candidateQuantity; j++)
                {
                    tab[i, j] = 0;
                }
            }

            //we have to put "1" in each A(i,j)
            for (int i = 0; i < candidateQuantity; i++)
            {
                tab[defaultList[i] - 1, permutation[i].IntValue - 1] = 1;
            }
            return tab;
        }

        /// <summary>
        /// transpose matrix 
        /// </summary>
        /// <param name="m">permutation matrix</param>
        /// <returns>transpone permutation matrix</returns>
        private int[,] transposeMatrix(int[,] m)
        {
            int[,] temp = new int[m.GetLength(0), m.GetLength(1)];
            for (int i = 0; i < m.GetLength(0); i++)
                for (int j = 0; j < m.GetLength(0); j++)
                    temp[j, i] = m[i, j];
            return temp;
        }

        /// <summary>
        /// Find inverse permuatation using a table method
        /// </summary>
        /// <param name="permutation">permutation to inverse</param>
        /// <returns>inverse permutation</returns>
        public List<BigInteger> getInversePermutation(List<BigInteger> permutation)
        {
            int[,] tab = generatePermutationMatrix(permutation);

            int[,] tabInv = transposeMatrix(tab);
            List<BigInteger> inversePermutation = new List<BigInteger>();

            for (int i = 0; i < tabInv.GetLength(1); i++)
            {
                for (int j = 0; j < tabInv.GetLength(0); j++)
                {
                    if (tabInv[i, j] == 1)
                    {
                        inversePermutation.Add(new BigInteger((j + 1).ToString()));
                    }
                }
            }
            return inversePermutation;
        }
	
    }
}
