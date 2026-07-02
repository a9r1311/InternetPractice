namespace Move.Packet
{
    //  パケット先頭1Byteに詰める識別用Enum
    public enum PacketType : byte
    {
        Message  = 0,        //  Stringパケット
        Position = 1,        //  座標パケット
        GoMatch = 2,         //  マッチ通知用パケット
        MatchSuccess = 3,    //  マッチ成功パケット
    }
}