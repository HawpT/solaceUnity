using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Threading;

namespace Assets._scripts
{
    

    class Server
    {
        static Socket listenerSocket;
        static List<ClientData> _clients;
        static List<string> usernames;
        public static Queue<string> OutgoingMessages;
        
        public static string StartServer(string newIP, string port)
        {
            //Networking stuff
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenerSocket.SetSocketOption(SocketOptionLevel.Socket , SocketOptionName.ReuseAddress, true);
            _clients = new List<ClientData>();
            OutgoingMessages = new Queue<string>();
            usernames = new List<string>();
            string returnMessage = "";

            IPEndPoint ip;
            try
            {
                ip = new IPEndPoint(IPAddress.Parse(newIP), int.Parse(port));
                returnMessage = "Server live!";
            }
            catch
            {
                ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
                returnMessage = "Invalid IP. Defaulting to 127.0.0.1:8000";
            }

            try
            {
                listenerSocket.Bind(ip);

                Thread listenThread = new Thread(ListenThread);

                listenThread.Start();
                returnMessage = "Server live!";
            }
            catch(Exception e)
            {
                returnMessage = "Hosting Failed. " + e.ToString();
            }

            return returnMessage;
        }

        static void ListenThread()
        {
            while (true)
            {
                listenerSocket.Listen(0);

                _clients.Add(new ClientData(listenerSocket.Accept()));
                Thread.Sleep(100);
            }
        }

        //disconnect from all active connections
        public static void DisconnectAll()
        {
            foreach(ClientData i in _clients)
            {
                i.clientSocket.Close();
            }
            listenerSocket.Disconnect(true);
        }

        //receiving data from the clients
        public static void Data_IN(object cSocket)
        {
            Socket clientSocket = (Socket)cSocket;

            byte[] Buffer;
            int readBytes;

            //loop the thread while the connection is open
            while (clientSocket.Connected)
            {
                try
                {
                    Buffer = new byte[clientSocket.SendBufferSize];

                    readBytes = clientSocket.Receive(Buffer);

                    if (readBytes > 0)
                    {
                        //data handling
                        Packet packet = new Packet(Buffer);

                        DataManager(packet);
                    }

                }
                catch(Exception ex)
                {
                    ex.ToString();
                }
                Thread.Sleep(200);
            }
        }

        //handles the types of packets coming in and out
        public static void DataManager(Packet p)
        {
            //respond to the packet
            switch (p.packetType)
            {

                //Imediately respond to all of our current clients with the message we just received
                case PacketType.Command:

                    //save the username of the person who sent the command
                    string senderName = "";
                    foreach (ClientData client in _clients)
                    {
                        if (client.id == p.senderID)
                            senderName = client.username;
                    }
                    //BROADCAST TO EVERYONE EXCEPT SENDER
                    foreach (ClientData client in _clients)
                    {
                        //Send to everyone except the original sender
                        if (client.id != p.senderID)
                        {
                            //THIS IS KEY
                            //The response contains the command, and the username attached to it to be used
                            Packet temp = new Packet(PacketType.Command, p.newCommand, senderName);
                            client.clientSocket.Send(temp.ToBytes());
                        }
                    }
                    break;
                
                //Register the usernames
                case PacketType.Registration:
                    foreach(ClientData client in _clients)
                    {
                        //Send to everyone except the original sender
                        if (client.id == p.senderID)
                        {
                            client.username = p.newCommand;
                            usernames.Add(client.username);
                        }
                    }
                    break;

                default:

                    break;
            }
        }
    }
    
    //data structure for incoming connections
    class ClientData
    {
        public Socket clientSocket;
        public Thread clientThread;
        public string id;
        public string username;

        public ClientData()
        {
            id = Guid.NewGuid().ToString();
            clientThread = new Thread(Server.Data_IN);
            clientThread.Start(clientSocket);
            SendRegistrationPacket();
        }

        public ClientData(Socket clientSocket)
        {
            id = Guid.NewGuid().ToString();
            this.clientSocket = clientSocket;
            clientThread = new Thread(Server.Data_IN);
            clientThread.Start(clientSocket);
            SendRegistrationPacket();
        }

        public void SendRegistrationPacket()
        {
            Packet p = new Packet(PacketType.Registration, "New Connection Established.", id);
            clientSocket.Send(p.ToBytes());
        }
    }
}
