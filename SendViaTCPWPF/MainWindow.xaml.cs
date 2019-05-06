using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;

namespace SendViaTCPWPF
{
    public partial class MainWindow : Window
    {
        private UDPListener Listener = new UDPListener();
        private bool IsStartStreaming = false;
        public MainWindow()
        {
            InitializeComponent();
            GetIpAdress();
            Listener.OnDataReceived += OnDataReceived;
        }

        private string GetIpAdress()
        {
            IPHostEntry iPHostEntry;
            iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress iPAddress in iPHostEntry.AddressList)
            {
                if (iPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    TextBlockIP.Text = iPAddress.ToString();
                }
            }
            return TextBlockIP.Text;
        }
        private int GetPort()
        {
            return Int32.Parse(TextBoxChoosenPort.Text);
        }
        private void TextBoxChoosenPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            ChangeUIControls();
        }

        private void ChangeUIControls()
        {
            if (IsStartStreaming)
            {
                Listener.Stop();
                IsStartStreaming = false;
                TextBoxChoosenPort.IsReadOnly = false;
                StartButton.Content = "START";
                StartButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
                TextBlockServerStatus.Text = "Closed";
                TestImage.Source = null;
                //TestImage.Visibility = Visibility.Hidden;
            }
            else
            {
                Listener.Start(GetPort(), GetIpAdress(), "\r\n");
                IsStartStreaming = true;
                TextBoxChoosenPort.IsReadOnly = true;
                StartButton.Content = "STOP";
                StartButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
                TextBlockServerStatus.Text = "Opened";
                //TestImage.Visibility = Visibility.Visible;
            }
        }

        private void OnDataReceived(string obj)
        {
            string screenshot = obj.Split('#')[1];

            var bytes = Convert.FromBase64String(screenshot);
            var stream = new MemoryStream(bytes);
            stream.Seek(0, SeekOrigin.Begin);
            TestImage.Dispatcher.Invoke(new Action(() => {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                TestImage.Source = bitmapImage;
            }));
        }
    }
}
