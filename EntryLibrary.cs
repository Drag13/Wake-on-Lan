using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Globalization;
using System.Net.Sockets;



namespace WakeUpLan_1_1
{
    [Serializable]
    sealed class Entry
    {
        private string mac;
        private string name;

        public string Mac { get { return mac; } set { mac = value; } }
        public string Name { get { return name; } set { name = value; } }

        public Entry(string _name, string _mac)
        {
            mac = _mac;
            name = _name;
        }


    }
    static class EntryLibrary
    {

        private static List<Entry> Entry_list = new List<Entry>(30);
        public static event Action<string, string> Entry_added = delegate { }; // do not tuch this :D
        private const int max_name_length = 25;
        private const int max_list_capacity = 25;
        
        private static bool write = true; 
        public static bool WriteToDiskFlag { get { return write; } set { write = value; } }

        public static int Safe_Add(string name, string mac)
        {
            if (CheckIt(name, mac) == 404) { return 404; }
            Entry_list.Add(new Entry(name, mac));
            if (write) SaveLibrary();
            Entry_added.Invoke(name, mac);
            return 200;
        }

        #region Check_Block
        private static int CheckIt(string name, string mac)
        {
            name = name.ToUpper();
            mac = mac.ToUpper();
            if ((Check_Name(name) == 404) || (Check_mac(mac) == 404)) { return 404; }
            else return 200;
        }

        private static int Check_Name(string _name)
        {
            if ((_name.Length == 0) || (_name == null)) { MessageBox.Show("Please, enter the name.", "Error!!", MessageBoxButton.OK, MessageBoxImage.Error); return 404; }
            if (_name.Length > max_name_length) { MessageBox.Show("The name can not be more than " + max_name_length.ToString() + " 25 characters.", "Error!!", MessageBoxButton.OK, MessageBoxImage.Error); return 404; }
            if (Entry_list.Count > max_list_capacity) { MessageBox.Show("Mac list is full. Please delete some entries", "Error!", MessageBoxButton.OK, MessageBoxImage.Error); return 404; }
            if (Check_Name_for_uniq(_name) == 404) { return 404; }
            return 200;
        }

        private static int Check_mac(string _mac)
        {
            if ((_mac.Length == 0) || (_mac == null)) { MessageBox.Show("No MAC adress entered.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error); return 404; }
            if (_mac.Length != 12) { MessageBox.Show("MAC should has 12 symbols without spaces or defices", "Error!", MessageBoxButton.OK, MessageBoxImage.Error); return 404; }

            try
            {
                for (int i = 0; i < 12; i = i + 2)
                {
                    byte test = byte.Parse(_mac.Substring(i, 2), NumberStyles.HexNumber);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Your MAC is wrong, please check it once more time.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return 404;
            }
            return 200;

        }

        private static int Check_Name_for_uniq(string name)
        {
            if (Entry_list == null) return 200;
            for (int i = 0; i < Entry_list.Count; i++)
            {
                if (Entry_list[i].Name == name) { MessageBox.Show("MAC with entered name already exist.", "ERROR!"); return 404; }
            }

            return 200;
        }

        #endregion Check_Block

        public static int Remove(string mac_name_for_del)
        {
            if ((mac_name_for_del == null) || (mac_name_for_del == "")) { return 404; }
            if ((Entry_list == null) || (Entry_list.Count == 0)) { return 404; }
            for (int i = 0; i < Entry_list.Count; i++)
            {
                if (Entry_list[i].Name == mac_name_for_del)
                {
                    Entry_list.RemoveAt(i);
                    if (write) SaveLibrary();
                    return 200;
                }
            }
            return 404;
        }

        public static string GetMacByName(string name)
        {
            if ((name == "") || (name == null)) { return null; }
            if ((Entry_list == null) || (Entry_list.Count == 0)) { return null; }

            for (int i = 0; i < Entry_list.Count; i++)
            {
                if (Entry_list[i].Name == name) { return Entry_list[i].Mac; }
            }
            return null;
        }

        public static void DownloadAndRefresh(ListBox names, TextBox mac)
        {
            Entry_list = DownloadLibrary();
            foreach (var item in Entry_list)
            {
                names.Items.Add(item.Name);
            }
            names.SelectedIndex = 0;
        }


        private static List<Entry> DownloadLibrary()
        {
            List<Entry> Temp_list = new List<Entry>(100);
            try
            {
                using (Stream stream = File.Open("lib.dat", FileMode.OpenOrCreate))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    ShellForLibrary downloadedShell = (ShellForLibrary)formatter.Deserialize(stream);
                    Temp_list = downloadedShell.Library;
                }
            }
            catch (Exception)
            {
                Temp_list = new List<Entry> { new Entry("Empty MAC", "000000000000") };
            }
            return Temp_list;
        }


        private static void SaveLibrary()
        {

            if (Entry_list != null)
                try
                {
                    using (Stream stream = File.Open("lib.dat", FileMode.Create))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, new ShellForLibrary(Entry_list));
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to save your data. Check your rights.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        public static void SendMagic(string mac)
        {
            if (Check_mac(mac) == 404) { return; }

            WOLClass client = new WOLClass();
            client.Connect(new IPAddress(0xffffffff), 0x2fff);
            client.SetClientToBrodcastMode();
            int counter = 0;
            byte[] bytes = new byte[1024];

            for (int y = 0; y < 6; y++)
                bytes[counter++] = 0xFF;

            for (int y = 0; y < 16; y++)
            {
                int i = 0;
                for (int z = 0; z < 6; z++)
                {
                    bytes[counter++] = byte.Parse(mac.Substring(i, 2), NumberStyles.HexNumber);
                    i += 2;
                }
            }
            client.Send(bytes, 1024);  //!!!
            Console.WriteLine("\"maxburov\"  - thanks");
        }

        public static void SendMagicToEveryBody()
        {
            if ((Entry_list == null) || (Entry_list.Count < 1)) { MessageBox.Show("You have no MAC adresses to send them magick!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            foreach (var item in Entry_list)
            {
                SendMagic(item.Mac);
            }
        }

    }


    [Serializable]
  sealed class ShellForLibrary
    {

        private List<Entry> Entry_list = new List<Entry>(100);

        public ShellForLibrary(List<Entry> entry_list)
        {
            Entry_list = encode_list(entry_list);
        }

        public List<Entry> Library
        {
            get
            {
                return decode_list();  
            }

        }
        private List<Entry> decode_list()  //Feel free to write your own encoding system to save MAC privacy
        {
            return Entry_list;
        }

        private List<Entry> encode_list(List<Entry> list_for_encode) //Feel free to write your own encoding system to save MAC privacy
        {
            return list_for_encode;
        }

    }


    class WOLClass : UdpClient
    {
        public WOLClass() : base() {}
        
        public void SetClientToBrodcastMode()
        {
            if (this.Active)
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
        }
    }


}
