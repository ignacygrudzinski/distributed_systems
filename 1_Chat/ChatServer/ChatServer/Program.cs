using System;
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
        private HashSet<string> clientNames { get; set; }
        private HashSet<StreamWriter> writers { get; set; }

        public static int Main(String[] args)
        {

            string address = "127.0.0.1";
            int maxClients = 10;
            int port = 11000;
            Server server = new Server();


            server.StartServer(address, maxClients, port);
            return 0;
        }


        // Incoming data from the client.  
        public static string data = null;

        public void StartServer(string address, int maxClients, int port)
        {

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
                    // Program is suspended while waiting for an incoming connection.  
                    Socket openConnection = listener.Accept();
                    ChatServer.SocketHandler socketHandler = new SocketHandler(openConnection, this);
                    new Thread(socketHandler.start).Start();
    
                //data = null;

                    //// An incoming connection needs to be processed.  
                    ////for (var x = 0; x<1024; x++)
                    ////{
                    ////    int bytesRec = handler.Receive(bytes);
                    ////    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    ////    if (data.IndexOf("<EOF>") > -1)
                    ////    {
                    ////        break;
                    ////    }
                    ////}

                    ////HANDLE BYTES
                    //handler.Receive(bytes);


                    //// Show the data on the console.  
                    //Console.WriteLine("Text received : {0}", System.Text.Encoding.UTF8.GetString(bytes));

                    //// Echo the data back to the client.  
                    //byte[] msg = Encoding.ASCII.GetBytes("oi fuck off mate");

                    //handler.Send(msg);
                    //handler.Shutdown(SocketShutdown.Both);
                    //handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }
    }
}