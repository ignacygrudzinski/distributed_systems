using System;
using System.Net.Sockets;

namespace ChatServer
{
    public class SocketHandler
    {
        private Socket handler;
        private Server server;
        // Data buffer for incoming data.  
        private byte[] bytes = new Byte[1024];

        public SocketHandler(Socket openConnection, Server server)
        {
            this.handler = openConnection;
            this.server = server;

        }

        public void start()
        {
            Console.WriteLine("bruh");
        }
    }
}
