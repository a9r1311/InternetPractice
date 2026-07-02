namespace Move.Packet
{
    using LiteNetLib;

    public struct PacketContext    //  ˆø”’Zk—p\‘¢‘Ì
    {
        public readonly NetPeer Peer { get; }
        public readonly NetPacketReader Reader { get; }

        public PacketContext(NetPeer peer, NetPacketReader reader)
        {
            Peer = peer;
            Reader = reader;
        }
    }
}