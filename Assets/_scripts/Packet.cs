using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Assets._scripts
{

    //Packet is a data structure which holds information to be sent across the TCP connection
    [Serializable]
    class Packet
    {
        public string newCommand;
        public string senderID;
        public PacketType packetType;

        //constuctors
        public Packet(PacketType type, string senderID)
        {
            newCommand = "";
            this.senderID = senderID;
            this.packetType = type;
        }

        public Packet(PacketType type, string newMessage, string senderID)
        {
            newCommand = "";
            this.senderID = senderID;
            newCommand = newMessage;
            this.packetType = type;
        }

        public Packet(byte[] packetbytes)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(packetbytes);

            Packet p = (Packet)bf.Deserialize(ms);
            ms.Close();
            this.newCommand = p.newCommand;
            this.senderID = p.senderID;
            this.packetType = p.packetType;
        }

        //translate the contents of the packet to bytes
        public byte[] ToBytes()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            bf.Serialize(ms, this);
            byte[] bytes = ms.ToArray();
            ms.Close();

            return bytes;
        }
    }

    //the types of packets
    public enum PacketType{ Registration, Command }

}
