﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Proxy
{

    /// <summary>
    /// extention functions in program, used to more complicated operations
    /// </summary>
    public static class Extentions
    {
        /// <summary>
        /// shuffling list elements
        /// </summary>
        /// <typeparam name="T">list to shuffle</typeparam>
        /// <param name="list">shuffled list</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
