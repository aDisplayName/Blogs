using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Monitors
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DpiViewModel _viewModel = new DpiViewModel();

        public MainWindow()
        {
            InitializeComponent();
            _viewModel.Reload();
            DataContext = _viewModel;

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

            _viewModel.Reload();

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _viewModel.Dispose();
        }
    }
}
