using System;

namespace SOMLibrary
{
    /// <summary>
    /// Little parts of the SOM.
    /// </summary>
    public interface INeuron
    {
        int X { get; set; }
        int Y { get; set; }
        IVector Weights { get; }

        double Distance(INeuron neuron);
        void SetWeight(int index, double value);
        double GetWeight(int index);
        void UpdateWeights(IVector input, double distanceDecay, double learningRate);
    }
    public class Neuron : INeuron
    {
        public int X { get; set; }
        public int Y { get; set; }
        public IVector Weights { get; }

        /// <summary>
        /// Common constructor for learning.
        /// </summary>
        /// <param name="numOfWeights"></param>
        public Neuron(int numOfWeights)
        {
            var random = new Random();
            Weights = new Vector();

            for (int i = 0; i < numOfWeights; i++)
            {
                Weights.Add(random.NextDouble());
            }
        }

        /// <summary>
        /// Cool constructor for visualization.
        /// </summary>
        /// <param name="weights"></param>
        public Neuron(Vector weights)
        {
            Weights = new Vector();
            for (int i = 0; i < weights.Count; i++)
            {
                Weights.Add(weights[i]);
            }
        }

        public double Distance(INeuron neuron)
        {
            return  Math.Pow((X - neuron.X), 2) + Math.Pow((Y - neuron.Y), 2);
        }

        public void SetWeight(int index, double value)
        {
            if (index >= Weights.Count)
                throw new ArgumentException("Wrong index!");

            Weights[index] = value;
        }

        public double GetWeight(int index)
        {
            if (index >= Weights.Count)
                throw new ArgumentException("Wrong index!");

            return Weights[index];
        }

        public void UpdateWeights(IVector input, double distanceDecay, double learningRate)
        {
            if (input.Count != Weights.Count)
                throw new ArgumentException("Wrong input!");

            for (int i = 0; i < Weights.Count; i++)
            {
                Weights[i] += distanceDecay * learningRate * (input[i] - Weights[i]);
            }
        }

        public override string ToString()
        {
            string outweights = X + ";" + Y + ";";
            for (int i = 0; i < Weights.Count - 1; i++)
            {
                outweights += Weights[i] + ";";
            }

            outweights += Weights[Weights.Count - 1];
            return outweights;
        }
    }
}