namespace Move.Packet
{
    //  先頭1Byteのパケット種類
    public enum PacketType : byte
    {
        Message      = 0,    //  Stringパケット
        Position     = 1,    //  座標パケット
        GoMatch      = 2,    //  マッチ通知用パケット
        MatchSuccess = 3,    //  マッチ成功パケット
    }
}