﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Proxy
{
    class Client
    {
        private ASCIIEncoding encoder;
        private TcpClient client;
        private NetworkStream stream;
        private Thread clientThread;
        private Logs logs;
        private ParserEA parserEA;

        public Client(Logs logs, Proxy proxy)
        {
            this.encoder = new ASCIIEncoding();
            this.logs = logs;
            this.parserEA = new ParserEA(this.logs, proxy);
        }


        public bool connect(string ip, string port)
        {

            client = new TcpClient();
            IPAddress ipAddress;
            if (ip.Contains(Constants.LOCALHOST))
            {
                ipAddress = IPAddress.Loopback;
            }
            else
            {
                ipAddress = IPAddress.Parse(ip);
            }

            try
            {
                int dstPort = Convert.ToInt32(port);
                client.Connect(new IPEndPoint(ipAddress, dstPort));
            }
            catch { }

            if (client.Connected)
            {
                stream = client.GetStream();
                clientThread = new Thread(new ThreadStart(displayMessageReceived));
                clientThread.Start();
                sendMyName();
                logs.addLog(Constants.CONNECTION_PASS, true, Constants.LOG_INFO, true);
                return true;
            }
            else
            {
                client = null;
                logs.addLog(Constants.CONNECTION_FAILED, true, Constants.LOG_ERROR, true);
                return false;
            }
        }

        private void sendMyName()
        {
            {
                byte[] buffer = encoder.GetBytes("//NAME// " + "PROXY");
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }

        private void displayMessageReceived()
        {
            byte[] message = new byte[4096];
            int bytesRead;

            while (stream.CanRead)
            {
                bytesRead = 0;
                try
                {
                    bytesRead = stream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }
                string strMessage = encoder.GetString(message, 0, bytesRead);
                this.parserEA.parseMessageFromEA(strMessage);
            }
            if (client != null)
            {
                disconnectFromElectionAuthority(true);
            }
        }

        public void disconnectFromElectionAuthority(bool error = false)
        {
            if (client != null)
            {
                try
                {
                    client.GetStream().Close();
                    client.Close();
                    client = null;
                }
                catch (Exception)
                {
                    Console.WriteLine("Problems with disconnecting from EA");
                }
                if (!error)
                {
                    logs.addLog(Constants.CONNECTION_DISCONNECTED, true, Constants.LOG_INFO, true);
                }
                else
                {
                    logs.addLog(Constants.CONNECTION_DISCONNECTED_ERROR, true, Constants.LOG_ERROR, true);
                    //form.Invoke(new MethodInvoker(delegate() { form.buttonsEnabled(); }));
                }
            }
        }

        public void sendMessage(string msg)
        {
            if (client != null && client.Connected && msg != "")
            {
                byte[] buffer = encoder.GetBytes(msg);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }
    }
}
