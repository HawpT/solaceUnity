using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Assets._scripts
{

    class Client
    {
        public static Socket master;
        public static string id;
        public static string username;
        public static Queue<Packet> messages;

        public Client()
        {
            messages = new Queue<Packet>();
        }

        public static string StartClient(string newIP, string port, string newUser)
        {
            string returnMessage = "";
            //Networking stuff
            master = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            messages = new Queue<Packet>();
            username = newUser;

            //attempt to resolve an IP with the one the user gave us or Default to localhost:8000
            IPEndPoint ip;
            try
            {
                ip = new IPEndPoint(IPAddress.Parse(newIP), int.Parse(port));
                
            }
            catch
            {
                ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
                returnMessage += newIP + ":" + port + " is not a valid address. Defaulting to 127.0.0.1:8000";
            }

            //attempt the connection
            try
            {
                master.Connect(ip);
                returnMessage += "Connected to server!";
            }
            catch
            {
                returnMessage += "\nCould not connect to " + ip.ToString();
            }

            //start our data gathering thread
            Thread t = new Thread(Data_IN);
            t.Start();

            if (master.Connected)
                return returnMessage;
            else
                return "Connection Problem.";
        }

        //send a message to the server, return false if sending failed
        public static bool SendCommand(string newCommand)
        {
            //attempt sending a new packet to the server
            try
            {
                Packet p = new Packet(PacketType.Command, newCommand, id);

                master.Send(p.ToBytes());
            }
            catch (Exception e)
            {
                e.ToString();
                return false;
            }

            return true;
        }

        public static void Disconnect()
        {
            master.Close();
        }

        //thread that listens for data from the server
        static void Data_IN()
        {
            byte[] Buffer;
            int readBytes;

            while (true)
            {
                Buffer = new byte[master.SendBufferSize];
                readBytes = master.Receive(Buffer);
                
                if (readBytes > 0)
                {
                    DataManager(new Packet(Buffer));
                }
                Thread.Sleep(200);
            }
        }

        //handle incoming packets
        static void DataManager(Packet p)
        {
            switch (p.packetType)
            {
                //register a username with the server
                case PacketType.Registration:
                    id = p.senderID;
                    master.Send((new Packet(PacketType.Registration, username, id)).ToBytes());
                    break;

                case PacketType.Command:
                    //enQ the raw packet! :PPPPP
                    messages.Enqueue(p);
                    break;

                default:
                    break;
            }
        }
    }
}
