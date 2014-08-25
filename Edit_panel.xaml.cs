using System.Windows;
using System.Windows.Input;


namespace WakeUpLan_1_1
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void Btn_add_Click(object sender, RoutedEventArgs e)
        {
            string name = TB_Name.Text;
            string mac = TB_Mac.Text;
            if (EntryLibrary.Safe_Add(name, mac) == 404) return;
            var user_choice = MessageBox.Show("New MAC added! Do you want to add one more MAC?", "Wunderbar!", MessageBoxButton.YesNo);
            if (user_choice.ToString() == "No") this.Close();
        }

        private void Btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

    }
}



