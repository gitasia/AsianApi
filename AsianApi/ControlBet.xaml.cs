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

namespace AsianApi
{
    /// <summary>
    /// Interaction logic for ControlBet.xaml
    /// </summary>
    public partial class ControlBet : Window
    {
        public ControlBet()
        {
            InitializeComponent();
            begin_control.Minimum = Int32.Parse(UCTable.min_begin_control);
            end_control.Maximum = Int32.Parse(UCTable.max_end_control);
            if (UCTable.begin_control != "") begin_control.Value = Int32.Parse(UCTable.begin_control);
            if (UCTable.end_control != "") end_control.Value = Int32.Parse(UCTable.end_control);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            UCTable.begin_control = ""; // begin_control.Minimum.ToString();
            UCTable.end_control = ""; // end_control.Maximum.ToString();
            Close();
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            UCTable.begin_control = begin_control.Value.ToString();
            UCTable.end_control = end_control.Value.ToString();
            DialogResult = true;
            Close();
        }
    }
}
