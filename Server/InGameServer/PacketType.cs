namespace Move.Packet
{
    //  先頭1Byteのパケット種類
    public enum PacketType : byte
    {
        Message        = 0,  //  Stringパケット
        AssignPlayerId = 1,  //  プレイヤーID割り振りパケット
        Login          = 2,  //  ログイン要求パケット
        GoMatch        = 3,  //  マッチ通知用パケット
        MatchSuccess   = 4,  //  マッチ成功パケット
        SpawnPlayer    = 5,  //  プレイヤー生成パケット
        Position       = 6,  //  座標パケット
    }
}