using System;
using System.IO;
using System.Net.Sockets;

namespace ChatServer
{
    public class SocketHandler
    {
        private Socket socket;
        private Server server;
        private string name;
        // Data buffer for incoming data.  
        private byte[] bytes = new Byte[1024];

        public SocketHandler(Socket openConnection, Server server)
        {
            this.socket = openConnection;
            this.server = server;

        }

        public void start(object state)
        {
            Stream stream = new NetworkStream(socket);
            StreamWriter output = new StreamWriter(stream);
            StreamReader input = new StreamReader(stream);
            output.AutoFlush = true;

            bool isNameValid = false;

            while (!isNameValid)
            {
                try { name = input.ReadLine();

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
                    socket.Close();
                    return;
                }

            }

            //TODO: move to Server class
            Console.WriteLine("TCP: client " + name + "\tconnected!");


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
                    else
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
            socket.Close();
        }


    }
}
