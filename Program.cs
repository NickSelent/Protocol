using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    static class Program
    {
        private static Socket icmpSocket;
        private static byte[] receiveBuffer = new byte[236];
        private static byte[] sendBuffer = new byte[236];
        private static EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        private static EndPoint targetedSender = new IPEndPoint(IPAddress.Parse("192.168.0.14"), 0);
        private static string pathNew = @"d:\0\a.zip";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            CreateIcmpSocket();
            while (true) { Thread.Sleep(10); }
        }

        private static void CreateIcmpSocket()
        {
            icmpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            //System.Net.Sockets.Socket
            icmpSocket.Bind(new IPEndPoint(IPAddress.Parse("192.168.0.14"), 0));
            // Uncomment to receive all ICMP message (including destination unreachable).
            // Requires that the socket is bound to a particular interface( NOT IPAddress.ANY )
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                icmpSocket.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 });
            }
            BeginReceiveFrom();
        }

        private static void BeginReceiveFrom()
        {
            icmpSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, ReceiveCallback, null);
            
        }


        private static void ReceiveCallback(IAsyncResult ar)
        {

            int len = icmpSocket.EndReceiveFrom(ar, ref remoteEndPoint);
            //Console.WriteLine(string.Format("{0} Received {1} bytes from {2}", DateTime.Now, len, remoteEndPoint));
            //only log traffic from special sender
            if (remoteEndPoint.Equals(targetedSender))
                LogIcmp(receiveBuffer, len);

            BeginReceiveFrom();
        }

        private static void LogIcmp(byte[] buffer, int length)
        {
            //for (int i = 0; i < length; i++)
            //{
            //    Console.WriteLine(String.Format("{0:X2} : {1} : {2}", buffer[i], Convert.ToChar(buffer[i]), i));
            //}

            byte[] newbuffer = new byte[length - 36];
            byte[] cntbuffer = new byte[8];
            Buffer.BlockCopy(buffer, 28, cntbuffer, 0, 8);
            Buffer.BlockCopy(buffer, 36, newbuffer, 0, newbuffer.Length);
            //string answer = ASCIIEncoding.ASCII.GetString(newbuffer);
            Console.WriteLine(BitConverter.ToInt64(cntbuffer,0));//  newbuffer.Length);

            //Console.WriteLine("");


            using (FileStream fsNew = new FileStream(pathNew, FileMode.Append, FileAccess.Write))
            {
                fsNew.Write(newbuffer, 0, newbuffer.Length);
            }

            //System.Threading.Thread.Sleep(1000);

        }
    }
}