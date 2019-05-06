using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendViaTCPWPF
{
    class UDPListener
    {
        private UdpClient Client;
        private IPEndPoint GroupEP;
        private Thread ReceivingThread;
        private string Delimiter;
        public Action<string> OnDataReceived;

        public void Start(int port, string ipAddress, string delimiter = "")
        {
            Delimiter = delimiter;
            Client = new UdpClient(port);
            GroupEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            ReceivingThread = new Thread(OnMessageReceived);
            ReceivingThread.IsBackground = true;
            ReceivingThread.Start();
        }

        private void OnMessageReceived()
        {
            string allData = string.Empty;
            while(true)
            {
                try
                {
                    var receiveData = Client.Receive(ref GroupEP);
                    Encoding enc = Encoding.GetEncoding(1250);
                    UTF8Encoding utf8 = new UTF8Encoding();
                    byte[] dst = Encoding.Convert(enc, utf8, receiveData, 0, receiveData.Length);
                    var data = utf8.GetString(dst);
                    allData += data;

                    if (string.IsNullOrEmpty(data))
                        continue;

                    if (string.IsNullOrEmpty(Delimiter))
                    {
                        OnDataReceived?.Invoke(allData);
                        allData = string.Empty;
                    }

                    while (allData.Contains(Delimiter))
                    {
                        var index = allData.IndexOf(Delimiter);
                        var message = allData.Substring(0, index);
                        OnDataReceived?.Invoke(message);
                        allData = allData.Substring(index + Delimiter.Length);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        public void Stop()
        {
            Client.Close();
        }
    }
}
