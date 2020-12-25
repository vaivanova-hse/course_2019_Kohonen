using System.Windows;
using System.Windows.Navigation;

namespace SOMProgram
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //NextButton.Content = "\u23Ed Далее";
            //BackButton.Content = "\u23EE Назад";
            //MainButton.Content = "\u26B0	";
            this.MainWindowFrame.Navigate(new WelcomePage());
        }

        private void Page_Navigated(object sender, NavigationEventArgs e)
        {

        }

    }
}