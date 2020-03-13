using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class UDPHandler
    {
        private Socket socket;
        private Server server;
        private string name;
        public EndPoint remoteEP { get; }

        public UDPHandler(Socket socket, Server server, string name, EndPoint remoteEP)
        {
            this.socket = socket;
            this.server = server;
            this.name = name;
            this.remoteEP = remoteEP;
            server.AddUDPClient(name, this);
        }

        //public void Start(object state)
        //{
        //    while (true)
        //    {
        //        byte[] buffer = new byte[1024];
        //        socket.Receive(buffer, buffer.Length, SocketFlags.None, );//, SocketFlags.None, ref remoteEP);
        //        server.SendToAll(name + " [UDP]: " + System.Text.Encoding.UTF8.GetString(buffer));
        //    }
        //}

        public void SendMessage(string msg)
        {
            byte[] buf = Encoding.UTF8.GetBytes(msg);
            socket.SendTo(buf, SocketFlags.None, remoteEP);
        }
    }
}
