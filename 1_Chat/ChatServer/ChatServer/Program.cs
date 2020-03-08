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
        private ConcurrentDictionary<string, StreamWriter> TCPWriters;
        private ConcurrentDictionary<string, UDPHandler> UDPWriters;
        private ConcurrentDictionary<EndPoint, string> IPs;


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
            IPEndPoint localEndPointUDP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            Socket UDPsocket = new Socket(ipAddress.AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);
            UDPsocket.ExclusiveAddressUse = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(UDPDeMultiplexer), UDPsocket);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(maxClients);
                UDPsocket.Bind(localEndPointUDP);

                // Start listening for connections.  
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Socket openConnection = listener.Accept();
                    // TODO: Refactor handler,make it a method maybe
                    SocketHandler socketHandler = new SocketHandler(openConnection, UDPsocket, this);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(socketHandler.Start));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public void UDPDeMultiplexer(object socket)
            //finds out who sent UDP message and sends it further
        {
            Socket udpSocket = (Socket)socket;
            while (true)
            {
                byte[] buffer = new byte[16894];
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0); //this API is swinging  wildly between C and Java sockets and I'm not a fan of that
                udpSocket.ReceiveFrom(buffer, ref ep);
                Console.WriteLine(((IPEndPoint)ep).Port);
                string name = GetNameUDP(ep);
                SendToAll(name + " [UDP]: " + System.Text.Encoding.UTF8.GetString(buffer));
            }
        }

        public bool AddClient(string name, StreamWriter writer)
        {
            var res = TCPWriters.TryAdd(name, writer);
            if (res)
                Console.WriteLine("TCP: client " + name + "\tconnected!");
            return res;
        }

        public bool AddUDPClient(string name, UDPHandler client)
        {
            var res = UDPWriters.TryAdd(name, client);
            res &= IPs.TryAdd(client.remoteEP, name);
            if (res)
            {
                Console.WriteLine("UDP: client " + name + "\tconnected!");
                Console.WriteLine(((IPEndPoint)client.remoteEP).Port);

            }
            return res;
        }

        public bool RemoveClient(string name)
        {
            bool res = TCPWriters.TryRemove(name, out _);
            res &= UDPWriters.TryRemove(name, out _);
            //remove from IPs
              Console.WriteLine("DISCONNECTED: " + name);
            SendToAll("SERVER: " + name + " disconnected");
            return res;
        }


        public void SendToAll(string message)
        {
            foreach (var entry in TCPWriters)
            {
                try { entry.Value.WriteLine(message); }
                catch (IOException)
                {
                    RemoveClient(entry.Key);
                }
            }
        }

        string GetNameUDP(EndPoint remoteEP)
        {
            string name;
            if (!IPs.TryGetValue(remoteEP, out name))
            {
                //TODO: honestly no idea what to throw here
                return "unknown client";
            }
            return name;
        }

        public void SendToAllUDP(string message)
        {
            foreach (var entry in UDPWriters)
            {
                entry.Value.SendMessage(message);
            }
        }

        public Server()
        {
            TCPWriters = new ConcurrentDictionary<string, StreamWriter>();
            UDPWriters = new ConcurrentDictionary<string, UDPHandler>();
            IPs = new ConcurrentDictionary<EndPoint, string>();
        }

    }
}