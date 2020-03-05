using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer
{

    public class Server
    {
        //private HashSet<string> clientNames { get; set; }
        private ConcurrentDictionary<string, StreamWriter> clients;

        public static int Main(String[] args)
        {
            //TODO: move to object properties and yeet the TCP part into another thread
            string address = "127.0.0.1";
            int maxClients = 10;
            int port = 11000;
            Server server = new Server();


            server.StartServer(address, maxClients, port);
            return 0;
        }



        public void StartServer(string address, int maxClients, int port)
        {
            ThreadPool.SetMaxThreads(maxClients, maxClients + 1);
            // TODO: implement using anything but localhost
            IPAddress ipAddress = IPAddress.Parse(address);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(maxClients);

                // Start listening for connections.  
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Socket openConnection = listener.Accept();
                    // TODO: Refactor handler,make it a method maybe
                    SocketHandler socketHandler = new SocketHandler(openConnection, this);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(socketHandler.start));

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public bool AddClient(string name, StreamWriter writer)
        {
            return clients.TryAdd(name, writer);

        }

        public bool RemoveClient(string name)
        {
            StreamWriter writer;
            bool res = clients.TryRemove(name, out writer);
            Console.WriteLine("DISCONNECTED: " + name);
            SendToAll("SERVER: " + name + " disconnected");
            writer.Dispose();
            return res;
        }


        public void SendToAll(string message)
        {
            foreach (var entry in clients)
            {
                try { entry.Value.WriteLine(message); }
                catch (IOException)
                {
                    RemoveClient(entry.Key);
                }
            }
        }

        public Server()
        {
            this.clients = new ConcurrentDictionary<string, StreamWriter>();
        }

    }
}