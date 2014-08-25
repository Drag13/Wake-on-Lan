using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WakeUpLan_1_1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitEvents();
            init_Context();
            InitState();
        }

        private void init_Context()
        {
            ContextMenu context = new ContextMenu();
            MenuItem _add = new MenuItem();
            _add.Header = "Add new MAC";
            _add.Click += _add_Click;

            MenuItem _del = new MenuItem();
            _del.Header = "Delete MAC";
            _del.Click += _del_Click;

            MenuItem _WakeItUp = new MenuItem();
            _WakeItUp.Header = "Wake it UP!";
            _WakeItUp.Click += _WakeItUp_Click;

            context.Items.Add(_add);
            context.Items.Add(_del);
            context.Items.Add(_WakeItUp);

            NameListBox.ContextMenu = context;
        }
        private void InitState() 
        {
            EntryLibrary.DownloadAndRefresh(NameListBox, MacTextBox);
        }
        
        private void InitEvents() 
        {
            EntryLibrary.Entry_added += EntryLibrary_Entry_added;
            NameListBox.SelectionChanged += NameListBox_SelectionChanged;
        }

        void NameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((NameListBox.Items == null)&&(NameListBox.SelectedIndex<0)) { return; }
            string name = (string)NameListBox.SelectedItem;
            MacTextBox.Text=EntryLibrary.GetMacByName(name);
        }

        void EntryLibrary_Entry_added(string name, string mac)
        {
            NameListBox.Items.Add(name);
            MacTextBox.Text = mac;
        }
      

        void _WakeItUp_Click(object sender, RoutedEventArgs e)
        {
            string mac = MacTextBox.Text;
            EntryLibrary.SendMagic(mac);
        }

        void _del_Click(object sender, RoutedEventArgs e)
        {
            string name_to_del = (string)NameListBox.SelectedItem;
            if ((name_to_del == null)||(name_to_del=="")) return;
            if (EntryLibrary.Remove(name_to_del) == 404)  return; 
            NameListBox.Items.Remove(NameListBox.SelectedItem);
            if (NameListBox.Items != null) { NameListBox.SelectedIndex = 0; }
        }

        void _add_Click(object sender, RoutedEventArgs e)
        {
            Window1 w = new Window1();
            w.ShowDialog();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            EntryLibrary.WriteToDiskFlag = false;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            EntryLibrary.WriteToDiskFlag = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ToAllFlag.IsChecked) { EntryLibrary.SendMagicToEveryBody(); return; }
            string mac = MacTextBox.Text;
            EntryLibrary.SendMagic(mac);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
