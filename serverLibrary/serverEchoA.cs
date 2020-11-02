using ServerEchoLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace TCPAsynchClasses
{
    public class serverEchoA : serverEcho
    {
        public delegate void TransmissionDataDelegate(NetworkStream stream);
        public serverEchoA(IPAddress IP, int port) : base(IP, port)
        {

        }
        /// <summary>
        ///     Funkcja sprawdzająca dane do "logowania". Dane zapisane w pliku porównywane z danymi przesłanymi przez użytkownika
        /// </summary>
        /// <param name="s1">Login</param>
        /// <param name="s2">Hasło</param>
        /// <returns>
        /// True - udane logowanie
        /// False - nieudane logowanie, niepoprawne dane
        /// </returns>
        private bool checkLogin(string s1, string s2)
        {
            bool r1 = false;
            bool r2 = false;

            string data = File.ReadAllText("test.txt");
            string[] separator = { " ", "\n" , "\r", "\t"};
            string[] tab = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            for(int i = 0;i<tab.Length;i-=-2)
            {
                r1 = tab.GetValue(i).Equals(s1.Replace("\0", string.Empty));
                r2 = tab.GetValue(i+1).Equals(s2.Replace("\0", string.Empty));
                if (r1 && r2) break;
            }
            return r1 && r2;
        }

        protected override void AcceptClient()
        {
            while (true)
            {
                TcpClient tcpClient = TcpListener.AcceptTcpClient();
                Stream = tcpClient.GetStream();
                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(BeginDataTransmission);
                //callback style

                var task = Task.Run(() => transmissionDelegate.Invoke(Stream));


                //transmissionDelegate.BeginInvoke(Stream, TransmissionCallback, tcpClient);

                // async result style
                //IAsyncResult result = transmissionDelegate.BeginInvoke(Stream, null, null);
                ////operacje......
                //while (!result.IsCompleted) ;
                ////sprzątanie
            }
        }
        private void TransmissionCallback(IAsyncResult ar)
        {
            // sprzątanie
        }
        protected override void BeginDataTransmission(NetworkStream stream)
        {
            byte[] buffer1 = new byte[Buffer_size];
            byte[] buffer2 = new byte[Buffer_size];
            byte[] buffer3 = new byte[Buffer_size];
            byte[] m1 = Encoding.ASCII.GetBytes("\nLogowanie pomyslne\n");
            byte[] m2 = Encoding.ASCII.GetBytes("\nLogowanie nieudane\n");
            byte[] m3 = Encoding.ASCII.GetBytes("\nJestes juz zalogowany\n");
            bool active = false;
            string s1 = "", s2 = "";
            while (true)
            {
                Array.Clear(buffer1, 0, buffer1.Length);
                Array.Clear(buffer2, 0, buffer2.Length);
                Array.Clear(buffer3, 0, buffer3.Length);
                try
                {
                    int message_size = 0;
                    while(message_size < 3)
                    {
                        message_size = stream.Read(buffer1, 0, Buffer_size);
                    }
                    stream.Write(buffer1, 0, message_size);
                    s1 = Encoding.UTF8.GetString(buffer1, 0, buffer1.Length);
                    message_size = 0;

                    while (message_size < 3)
                    {
                        message_size = stream.Read(buffer2, 0, Buffer_size);
                    }
                    stream.Write(buffer2, 0, message_size);
                    s2 = Encoding.UTF8.GetString(buffer2, 0, buffer2.Length);
                    message_size = 0;

                    if (!active)
                    {
                        active = checkLogin(s1, s2);
                        if(active) stream.Write(m1, 0, m1.Length);
                        else stream.Write(m2, 0, m2.Length);
                    }
                    else
                    {
                        stream.Write(m3, 0, m3.Length);
                    }
                    
                }
                catch (IOException e)
                {
                    break;
                }
            }
        }
        public override void Start()
        {
            StartListening();
            //transmission starts within the accept function
            AcceptClient();
        }
    }
}

