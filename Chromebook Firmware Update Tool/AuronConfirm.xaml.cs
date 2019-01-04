using System;
using System.Windows;

namespace Chromebook_Firmware_Update_Tool
{
    /// <summary>
    /// Interaction logic for AuronConfirm.xaml
    /// </summary>
    public partial class AuronConfirm : Window
    {
        public MainWindow mainWindow;
        public AuronConfirm()
        {
            InitializeComponent();
        }

        private void c910_Click(object sender, RoutedEventArgs e)
        {
            this.mainWindow.model = "auron_yuna";
            this.Close();
        }

        private void c740_Click(object sender, RoutedEventArgs e)
        {
            this.mainWindow.model = "auron_paine";
            this.Close();
        }
    }
}
