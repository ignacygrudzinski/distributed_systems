using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatServer
{
    public class SocketHandler
    {
        private Socket socketTCP;
        private Socket socketUDP;
        private Server server;
        private string name;

        public SocketHandler(Socket socketTCP, Socket socketUDP, Server server)
        {
            this.socketTCP = socketTCP;
            this.socketUDP = socketUDP;
            this.server = server;
        }

        public void Start(object state)
        {
            Stream stream = new NetworkStream(socketTCP);
            StreamWriter output = new StreamWriter(stream);
            StreamReader input = new StreamReader(stream);
            output.AutoFlush = true;



            bool isNameValid = false;

            while (!isNameValid)
            {
                try
                {
                    //move to an add name function or whatevs
                    output.WriteLine("SERVER: What's your name?");

                    name = input.ReadLine();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        output.WriteLine("SERVER: Name can't be empty or whitespace. Try a different one!");
                    }
                    else if (server.AddClient(name, output) == false)
                    {
                        output.WriteLine("SERVER: Name already taken. Try a different one!");
                    }
                    else
                    {
                        isNameValid = true;
                    }

                }
                catch (IOException)
                {
                    socketTCP.Close();
                    return;
                }

            }


            UDPHandler udpChannel = new UDPHandler(socketUDP, server, name, socketTCP
                .RemoteEndPoint);

            //ThreadPool.QueueUserWorkItem(new WaitCallback(udpChannel.Start));

            string msg;

            while (true)
            {
                try
                {
                    msg = input.ReadLine();
                    if (msg == "!q")
                    {
                        Cleanup();
                        return;
                    }
                    else if (!string.IsNullOrWhiteSpace(msg))
                    {
                        server.SendToAll(name + ": " + msg);
                    }
                }
                catch (IOException)
                {
                    Cleanup();
                    return;
                }

            }

        }

        private void Cleanup()
        {
            server.RemoveClient(name);
            socketTCP.Close();
        }


    }
}
