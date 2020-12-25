using System;
using System.Collections.Generic;
using System.Linq;

namespace SOMLibrary
{
    public interface IVector : IList<double>
    {
        double EuclidianDistance(IVector vector);
    }
    /// <summary>
    /// Easy way to find distance.
    /// </summary>
    public class Vector : List<double>, IVector
    {
        /// <summary>
        /// Calculate Euclidian distance.
        /// </summary>
        /// <param name="vector">Vector to calculate.</param>
        /// <returns>Euclidian distance</returns>
        public double EuclidianDistance(IVector vector)
        {
            if (vector.Count != Count)
                throw new ArgumentException("Not the same size");

            return Math.Sqrt(this.Select(x => Math.Pow(x - vector[this.IndexOf(x)], 2)).Sum());
        }
    }
}