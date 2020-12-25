using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SOMLibrary
{
    /// <summary>
    /// Class that do everything for SOM.
    /// </summary>

    public class SoMap
    {
        internal INeuron[,] Matrix;
        public int Height;
        public int Width;
        internal double MatrixRadius;
        internal double NumberOfIterations;
        internal double TimeConstant;
        internal double LearningRate;
        public double[,] Umatrix;
        public int Dim;
        public Color MaxColor = Color.Black;
        public Color MinColor = Color.White;
        public Color[,] GreyMatrix;
        public Color[][,] ColorAtlas;

        /// <summary>
        /// Common constructor for learning.
        /// </summary>
        /// <param name="width">Width of the map, eq to height.</param>
        /// <param name="height">Height of the map, eq to width.</param>
        /// <param name="inputDimension">Dimension of data.</param>
        /// <param name="numberOfIterations">Learning iterations.</param>
        /// <param name="learningRate">Speed of learning.</param>
        public SoMap(int width, int height, int inputDimension, int numberOfIterations, double learningRate)
        {
            Width = width;
            Height = height;
            Matrix = new INeuron[Width, Height];
            Umatrix = new double[Width, Height];
            NumberOfIterations = numberOfIterations;
            LearningRate = learningRate;
            Dim = inputDimension;

            MatrixRadius = Math.Max(Width, Height) / 2;
            TimeConstant = NumberOfIterations / Math.Log(MatrixRadius);

            InitializeConnections(inputDimension);
        }

        /// <summary>
        /// Cool constructor for already learned maps to visualize.
        /// </summary>
        /// <param name="path">Path to map data.</param>
        public SoMap(string path)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture =
                new System.Globalization.CultureInfo("en-US");
            using (var reader = new StreamReader(path))
            {
                Width = (int)Math.Sqrt(File.ReadAllLines(path).Length);
                Height = Width;
                Matrix = new INeuron[Width, Height];
                Umatrix = new double[Width, Height];
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    var values = line.Split(';');

                    int x = int.Parse(values[0]);
                    int y = int.Parse(values[1]);

                    Dim = values.Length - 2;

                    var inputVector = new Vector();

                    for (int i = 2; i < values.Length; i++)
                    {
                        inputVector.Add(double.Parse(values[i]));
                    }
                    Matrix[x, y] = new Neuron(inputVector) { X = x, Y = y }; ;
                }
            }
        }

        /// <summary>
        /// Learning method for map. Magic is here.
        /// </summary>
        /// <param name="input">Training input vector.</param>
        public void Train(Vector[] input)
        {
            Random rnd = new Random();
            int iteration = 0;
            var learningRate = LearningRate;

            int[] alreadyTaken = new int[input.Length];
            for (int i = 0; i < alreadyTaken.Length; i++)
            {
                alreadyTaken[i] = 0;
            }

            int k = 0;

            while (iteration < NumberOfIterations)
            {
                int index;
                var currentRadius = CalculateNeighborhoodRadius(iteration);

                if (!alreadyTaken.Contains(k))
                {
                    k += 1;
                }

                do
                {
                    index = rnd.Next(input.Length);
                } while (alreadyTaken[index] != k);

                alreadyTaken[index] += 1;

                var currentInput = input[index];
                var bmu = CalculateBmu(currentInput);


                (int xStart, int xEnd, int yStart, int yEnd) = GetRadiusIndexes(bmu, currentRadius);


                for (int x = xStart; x < xEnd; x++)
                {
                    for (int y = yStart; y < yEnd; y++)
                    {
                        var processingNeuron = Matrix[x, y];
                        var distance = bmu.Distance(processingNeuron);

                        if (distance <= Math.Pow(currentRadius, 2.0))
                        {
                            var distanceDrop = GetDistanceDrop(distance, currentRadius);
                            processingNeuron.UpdateWeights(currentInput, distanceDrop, learningRate);
                        }
                    }
                }

                iteration++;
                learningRate = LearningRate * Math.Exp(-(double)iteration / NumberOfIterations);
            }
        }

        /// <summary>
        /// Get indexes of neighbor neurons.
        /// </summary>
        /// <param name="bmu">Best matching unit.</param>
        /// <param name="currentRadius">Radius of neighborhood.</param>
        /// <returns>Indexes of neighborhood.</returns>

        internal (int xStart, int xEnd, int yStart, int yEnd) GetRadiusIndexes(INeuron bmu, double currentRadius)
        {
            var xStart = (int)(bmu.X - currentRadius - 1);
            xStart = (xStart < 0) ? 0 : xStart;

            var xEnd = (int)(xStart + (currentRadius * 2) + 1);
            if (xEnd > Width) xEnd = Width;

            var yStart = (int)(bmu.Y - currentRadius - 1);
            yStart = (yStart < 0) ? 0 : yStart;

            var yEnd = (int)(yStart + (currentRadius * 2) + 1);
            if (yEnd > Height) yEnd = Height;

            return (xStart, xEnd, yStart, yEnd);
        }

        internal double CalculateNeighborhoodRadius(double iteration)
        {
            return MatrixRadius * Math.Exp(-iteration / TimeConstant);
        }

        internal double GetDistanceDrop(double distance, double radius)
        {
            return Math.Exp(-(Math.Pow(distance, 2.0) / (2 * Math.Pow(radius, 2.0))));
        }

        internal INeuron CalculateBmu(IVector input)
        {
            INeuron bmu = Matrix[0, 0];
            double bestDist = input.EuclidianDistance(bmu.Weights);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    var distance = input.EuclidianDistance(Matrix[i, j].Weights);
                    if (distance < bestDist)
                    {
                        bmu = Matrix[i, j];
                        bestDist = distance;
                    }
                }
            }

            return bmu;
        }

        private void InitializeConnections(int inputDimension)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Matrix[i, j] = new Neuron(inputDimension) { X = i, Y = j };
                }
            }
        }

        /// <summary>
        /// Make u-matrix by calculating weights of neighbors.
        /// </summary>
        public void MakeUMatrix()
        {
            double mean = 0;

            Umatrix = new double[Width, Height];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    var v = Matrix[i, j];
                    double distances = 0;
                    int count = 0;

                    // neuron above
                    if (i - 1 >= 0)
                    {
                        distances += v.Weights.EuclidianDistance(Matrix[i - 1, j].Weights);
                        count += 1;
                    }

                    // neuron below
                    if (i + 1 <= Width - 1)
                    {
                        distances += v.Weights.EuclidianDistance(Matrix[i + 1, j].Weights);
                        count += 1;
                    }

                    // neuron left
                    if (j - 1 >= 0)
                    {
                        distances += v.Weights.EuclidianDistance(Matrix[i, j - 1].Weights);
                        count += 1;
                    }

                    // neuron right
                    if (j + 1 <= Height - 1)
                    {
                        distances += v.Weights.EuclidianDistance(Matrix[i, j + 1].Weights);
                        count += 1;
                    }

                    // neuron left up diagonal
                    if (j - 1 >= 0 && i - 1 >= 0)
                    {
                        distances += v.Weights.EuclidianDistance(Matrix[i - 1, j - 1].Weights);
                        count += 1;
                    }

                    // neuron right up diagonal
                    if (j + 1 <= Height - 1 && i - 1 >= 0)
                    {
                        distances += v.Weights.EuclidianDistance(Matrix[i - 1, j + 1].Weights);
                        count += 1;
                    }

                    // neuron left down diagonal
                    if (j - 1 >= 0 && i + 1 <= Width - 1)
                    {
                        distances += v.Weights.EuclidianDistance(Matrix[i + 1, j - 1].Weights);
                        count += 1;
                    }

                    // neuron right down diagonal
                    if (j + 1 <= Height - 1 && i + 1 <= Width - 1)
                    {
                        distances += v.Weights.EuclidianDistance(Matrix[i + 1, j + 1].Weights);
                        count += 1;
                    }

                    Umatrix[i, j] = (distances / count);
                    mean += Umatrix[i, j];

                }
            }

            // This is for normalization and good visual.
            // Kill problems with colors.

            mean /= Umatrix.Length;
            int belowMean = 0;
            int aboveMean = 0;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (Umatrix[i, j] > mean)
                    {
                        aboveMean += 1;
                    }
                    else
                    {
                        belowMean += 1;
                    }
                }
            }

            if (aboveMean > belowMean)
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        Umatrix[i, j] = Math.Log(Umatrix[i, j]);

                        if (Umatrix[i, j] < 0)
                        {
                            Umatrix[i, j] = 0;
                        }
                    }
                }
            }

            else
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        Umatrix[i, j] = Math.Pow(Umatrix[i, j], 0.35);

                        if (Umatrix[i, j] < 0)
                        {
                            Umatrix[i, j] = 0;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Get color for certain value.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <param name="min">Minimum value in array.</param>
        /// <param name="max">Maximum value in array.</param>
        /// <returns></returns>
        private Color HeatMapColor(double value, double min, double max)
        {
            Color firstColour = MaxColor;
            Color secondColour = MinColor;

            int rOffset = Math.Max(firstColour.R, secondColour.R);
            int gOffset = Math.Max(firstColour.G, secondColour.G);
            int bOffset = Math.Max(firstColour.B, secondColour.B);

            int deltaR = Math.Abs(firstColour.R - secondColour.R);
            int deltaG = Math.Abs(firstColour.G - secondColour.G);
            int deltaB = Math.Abs(firstColour.B - secondColour.B);

            double val = (value - min) / (max - min);

            int r = rOffset - Convert.ToByte(deltaR * (1 - val));
            int g = gOffset - Convert.ToByte(deltaG * (1 - val));
            int b = bOffset - Convert.ToByte(deltaB * (1 - val));

            return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }


        /// <summary>
        /// Make colorful visualization. Not only Gray.
        /// </summary>
        /// <param name="maxCol">Max dist.</param>
        /// <param name="minCol">Min dist.</param>
        /// <param name="path">Path to save color map.</param>
        public void MakeGrayMatrix(Color maxCol, Color minCol, string path)
        {
            MaxColor = maxCol;
            MinColor = minCol;

            GreyMatrix = new Color[Width, Height];
            double maximum = Double.MinValue;
            double minimum = Double.MaxValue;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (Umatrix[i, j] < minimum)
                    {
                        minimum = Umatrix[i, j];
                    }

                    if (Umatrix[i, j] > maximum)
                    {
                        maximum = Umatrix[i, j];
                    }
                }
            }

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    GreyMatrix[i, j] = HeatMapColor(Umatrix[i, j], minimum, maximum);
                }
            }

            // Painting and saving map.

            int size = 10000 / (Width * Width);

            Bitmap bmp = new Bitmap(Width * size, Width * size);

            for (int y = 0; y < Width * size;)
            {
                for (int x = 0; x < Width * size;)
                {
                    for (int i = 0; i < size; i++)
                    {
                        for (int j = 0; j < size; j++)
                        {

                            bmp.SetPixel(x + i, y + j, GreyMatrix[x / size, y / size]);
                        }
                    }
                    x += size;
                }
                y += size;
            }

            bmp.Save(path + "\\matrix_map.jpg");
        }


        /// <summary>
        /// Make and save colorful ATLAS.
        /// </summary>
        /// <param name="param">What parameters must be done and saved.</param>
        /// <param name="path">Path to saving.</param>
        public void MakeColorAtlas(int[] param, string path)
        {
            ColorAtlas = new Color[Dim][,];

            for (int s = 0; s < param.Length; s++)
            {
                ColorAtlas[param[s]] = new Color[Width, Height];

                double[,] paramWeigh = new double[Width, Height];

                double mean = 0;

                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        var v = Matrix[i, j].GetWeight(param[s]);
                        double distances = 0;
                        int count = 0;

                        // neuron above
                        if (i - 1 >= 0)
                        {
                            distances += Math.Abs(v - Matrix[i - 1, j].GetWeight(param[s]));
                            count += 1;
                        }

                        // neuron below
                        if (i + 1 <= Width - 1)
                        {
                            distances += Math.Abs(v - Matrix[i + 1, j].GetWeight(param[s]));
                            count += 1;
                        }

                        // neuron left
                        if (j - 1 >= 0)
                        {
                            distances += Math.Abs(v - Matrix[i, j - 1].GetWeight(param[s]));
                            count += 1;
                        }

                        // neuron right
                        if (j + 1 <= Height - 1)
                        {
                            distances += Math.Abs(v - Matrix[i, j + 1].GetWeight(param[s]));
                            count += 1;
                        }

                        // neuron left up diagonal
                        if (j - 1 >= 0 && i - 1 >= 0)
                        {
                            distances += Math.Abs(v - Matrix[i - 1, j - 1].GetWeight(param[s]));
                            count += 1;
                        }

                        // neuron right up diagonal
                        if (j + 1 <= Height - 1 && i - 1 >= 0)
                        {
                            distances += Math.Abs(v - Matrix[i - 1, j + 1].GetWeight(param[s]));
                            count += 1;
                        }

                        // neuron left down diagonal
                        if (j - 1 >= 0 && i + 1 <= Width - 1)
                        {
                            distances += Math.Abs(v - Matrix[i + 1, j - 1].GetWeight(param[s]));
                            count += 1;
                        }

                        // neuron right down diagonal
                        if (j + 1 <= Height - 1 && i + 1 <= Width - 1)
                        {
                            distances += Math.Abs(v - Matrix[i + 1, j + 1].GetWeight(param[s]));
                            count += 1;
                        }

                        paramWeigh[i, j] = (distances / count);
                        mean += paramWeigh[i, j];
                    }
                }


                double maximum = Double.MinValue;
                double minimum = Double.MaxValue;



                mean /= paramWeigh.Length;
                int belowMean = 0;
                int aboveMean = 0;

                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        if (paramWeigh[i, j] > mean)
                        {
                            aboveMean += 1;
                        }
                        else
                        {
                            belowMean += 1;
                        }
                    }
                }

                if (aboveMean > belowMean)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        for (int j = 0; j < Height; j++)
                        {
                            paramWeigh[i, j] = Math.Log(paramWeigh[i, j], 3);
                            if (paramWeigh[i, j] < 0)
                            {
                                paramWeigh[i, j] = 0;
                            }
                        }
                    }
                }

                else
                {
                    for (int i = 0; i < Width; i++)
                    {
                        for (int j = 0; j < Height; j++)
                        {
                            paramWeigh[i, j] = Math.Pow(paramWeigh[i, j], 0.5);

                            if (paramWeigh[i, j] < 0)
                            {
                                paramWeigh[i, j] = 0;
                            }
                        }
                    }
                }

                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        if (paramWeigh[i, j] < minimum)
                        {
                            minimum = paramWeigh[i, j];
                        }

                        if (paramWeigh[i, j] > maximum)
                        {
                            maximum = paramWeigh[i, j];
                        }
                    }
                }

                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        ColorAtlas[param[s]][i, j] = HeatMapColor(paramWeigh[i, j], minimum, maximum);

                    }
                }


                int size = 10000 / (Width * Width);

                Bitmap bmp = new Bitmap(Width * size, Width * size);

                for (int y = 0; y < Width * size;)
                {
                    for (int x = 0; x < Width * size;)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            for (int j = 0; j < size; j++)
                            {
                                bmp.SetPixel(x + i, y + j, ColorAtlas[param[s]][x / size, y / size]);
                            }
                        }
                        x += size;
                    }
                    y += size;
                }

                bmp.Save(path + "\\" + s + "_parameter_of_the_map.jpg");

            }
        }

        /// <summary>
        /// Saving map that had already learned.
        /// </summary>
        /// <param name="path">Path to saving.</param>
        public void SaveMap(string path)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture =
                new System.Globalization.CultureInfo("en-US");
            using (var writer = new StreamWriter(path + "\\saving_som.csv"))
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        writer.WriteLine(Matrix[i, j]);
                    }
                }
            }
        }
    }
}