using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using BUZZ.Core.Models;

namespace BUZZ.Core.Multiboxing
{
    /// <summary>
    /// Interaction logic for PullerView.xaml
    /// </summary>
    public partial class PullerView : UserControl
    {
        private PullerViewModel CurrentViewModel;

        public PullerView(BuzzCharacter buzzCharacter)
        {
            InitializeComponent();
            CurrentViewModel = new PullerViewModel(buzzCharacter);
            this.DataContext = CurrentViewModel;
        }
    }
}
