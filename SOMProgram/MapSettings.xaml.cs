using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using SOMLibrary;
using MessageBox = System.Windows.MessageBox;

namespace SOMProgram
{
    /// <summary>
    /// Логика взаимодействия для MapSettings.xaml
    /// </summary>


    public partial class MapSettings : Page
    {
        public static System.Drawing.Color FirstColor = System.Drawing.Color.Black;
        public static System.Drawing.Color SecondColor = System.Drawing.Color.White;
        public static int NumberOfIterations = 500;
        public static double LearningRate = 0.4;
        public static int NumberOfNeurons = 10;
        public static SoMap Som;

        public MapSettings()
        {
            InitializeComponent();
            FirstColor = System.Drawing.Color.Black;
            SecondColor = System.Drawing.Color.White;
            NumberOfIterations = 500;
            LearningRate = 0.4;
            NumberOfNeurons = 10;
            if (WelcomePage.ifSaved)
            {
                NeuronNumberBox.Text = "";
                IterationNumberBox.Text = "";
                LearningRateTextBox.Text = "";
                NeuronNumberBox.IsEnabled = false;
                IterationNumberBox.IsEnabled = false;
                LearningRateTextBox.IsEnabled = false;
                CheckButton.IsEnabled = false;
                BackButton.IsEnabled = false;
                NextButton.IsEnabled = true;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NeuronNumberBox.IsEnabled = true;
            IterationNumberBox.IsEnabled = true;
            LearningRateTextBox.IsEnabled = true;
            NextButton.IsEnabled = false;
            CheckButton.IsEnabled = true;
            NeuronNumberBox.Text = "";
            IterationNumberBox.Text = "";
            LearningRateTextBox.Text = "";
        }


        private void BackToStartButton_Click(object sender, RoutedEventArgs e) => NavigationService.Content = new WelcomePage();

        private void FirstColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                FirstColor = colorDialog.Color;
                FirstColorButton.Background = new SolidColorBrush(Color.FromArgb(FirstColor.A, FirstColor.R, FirstColor.G, FirstColor.B));
            }
        }

        private void SecondColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                SecondColor = colorDialog.Color;
                SecondColorButton.Background = new SolidColorBrush(Color.FromArgb(SecondColor.A, SecondColor.R, SecondColor.G, SecondColor.B));
            }
        }



        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            NeuronNumberBox.IsEnabled = false;
            IterationNumberBox.IsEnabled = false;
            LearningRateTextBox.IsEnabled = false;
            NextButton.IsEnabled = true;
            CheckButton.IsEnabled = false;
            try
            {
                NumberOfNeurons = (int)Math.Sqrt(int.Parse(NeuronNumberBox.Text));
                if (NumberOfNeurons * NumberOfNeurons < 20 || NumberOfNeurons * NumberOfNeurons > 5000)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                NeuronNumberBox.Text = 100.ToString();

            }

            try
            {
                LearningRate = double.Parse(LearningRateTextBox.Text);
                if (LearningRate > 1 || LearningRate <= 0)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                LearningRateTextBox.Text = (0.4).ToString();
            }

            try
            {
                NumberOfIterations = Int32.Parse(IterationNumberBox.Text);
                if (NumberOfIterations > 8000 || NumberOfIterations < 0)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                IterationNumberBox.Text = 500.ToString();
            }

        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WelcomePage.ifSaved)
                {
                    NavigationService.Content = new VisualizationPage();
                }
                else if (Som == null)
                {
                    UpdateLayout();
                    Som = new SoMap(NumberOfNeurons, NumberOfNeurons, WelcomePage.Dimension, NumberOfIterations, LearningRate);
                    var inputTrainArray = WelcomePage.Input.ToArray();
                    Som.Train(inputTrainArray);
                    NextButton.IsEnabled = true;
                }
                else
                {
                    NavigationService.Content = new VisualizationPage();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Необходим полный доступ к директории", "Exception", MessageBoxButton.OK);
            }

        }
    }
}
