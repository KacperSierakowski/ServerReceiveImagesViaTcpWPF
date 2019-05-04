using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
            WhatIsIpAdress();
            //SecondThread();
        }
        private string WhatIsIpAdress()
        {
            IPHostEntry iPHostEntry;
            string localIp = "?";
            iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress iPAddress in iPHostEntry.AddressList)
            {
                if (iPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    localIp = iPAddress.ToString();
                    TextBlockIP.Dispatcher.Invoke(new Action(() => TextBlockIP.Text = localIp), DispatcherPriority.Render);
                }
            }
            return localIp;
        }
        private int WhatIsChoosenPort()
        {
            int Port = 8888;
            TextBoxChoosenPort.Dispatcher.Invoke(new Action(() => Port = Int32.Parse(TextBoxChoosenPort.Text)), DispatcherPriority.Render);
            return Port;
        }
        private void TextBoxChoosenPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }
        int clientID = 1;
        private void SecondThread()
        {
            Thread thread1 = new Thread(() =>
            {
                TcpListener Listener = null;
                try
                {
                    string ip = WhatIsIpAdress();
                    Listener = new TcpListener(IPAddress.Parse(ip), 8888);
                    Listener.Start();
                    TestImage.Dispatcher.Invoke(new Action(() => TextBlockServerStatus.Text = "Listening..."), DispatcherPriority.Render);
                    while (true)
                    {
                        TcpClient client = Listener.AcceptTcpClient();
                        TestImage.Dispatcher.Invoke(new Action(() => TextBlockClientId.Text = clientID.ToString()), DispatcherPriority.Render);
                        Thread clientThread = new Thread(() => ProcessClientRequest(client, clientID));
                        clientThread.Start();
                        clientID++;
                    }
                }
                catch (Exception e)
                {
                    TestImage.Dispatcher.Invoke(new Action(() => TextBlockServerStatus.Text = "Lost connection with client" + e.ToString()), DispatcherPriority.Render);
                }
            });
            thread1.Start();
        }
        private void ProcessClientRequest(TcpClient client, int clientId)
        {
            try
            {
                var networkStream = (client.GetStream());
                string filePath = "O:/Pas Oriona/Kariera/Nowy folder/test" + "ClientId" + clientId.ToString() + ".jpeg";
                string ImageInString = "";
                FileStream fileStream = File.OpenWrite(filePath);
                networkStream.CopyTo(fileStream);
                
                fileStream.Flush();
                fileStream.Close();
                TestImage.Dispatcher.Invoke(new Action(() => TestImage.Source = new BitmapImage(new Uri(filePath))), DispatcherPriority.Render);
            }
            catch (Exception e)
            {
                TestImage.Dispatcher.Invoke(new Action(() => TextBlockServerStatus.Text = "Lost connection with client" + e.ToString()), DispatcherPriority.Render);
            }
        }

        public static System.Drawing.Image StringToImage(this string base64String)
        {
            if (String.IsNullOrWhiteSpace(base64String))
                return null;

            var bytes = Convert.FromBase64String(base64String);
            var stream = new MemoryStream(bytes);
            return System.Drawing.Image.FromStream(stream);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SecondThread();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TestImage.Dispatcher.Invoke(new Action(() => TextBlockClientId.Text = "clientID" + e.ToString()), DispatcherPriority.Render);
        }


        private void SocketForOneClient()
        {
            Thread thread1 = new Thread(() =>
            {

                string ip = WhatIsIpAdress();
                int port = WhatIsChoosenPort();
                IPEndPoint iPEnd = new IPEndPoint(IPAddress.Parse(ip), port);
                TextBlockServerStatus.Dispatcher.Invoke(new Action(() => TextBlockServerStatus.Text = "Listening..."), DispatcherPriority.Render);
                string filePath = "O:/Pas Oriona/Kariera/Nowy folder/test.jpg";

                var listener = new TcpListener(iPEnd);
                listener.Start();

                using (var incoming = listener.AcceptTcpClient())
                using (var networkStream = incoming.GetStream())
                using (var fileStream = File.OpenWrite(filePath))
                {
                    networkStream.CopyTo(fileStream);

                    fileStream.Flush();
                    fileStream.Close();
                    TestImage.Dispatcher.Invoke(new Action(() => TestImage.Source = new BitmapImage(new Uri(filePath))), DispatcherPriority.Render);

                }
                listener.Stop();
            });
            thread1.Start();
        }

    }
}
