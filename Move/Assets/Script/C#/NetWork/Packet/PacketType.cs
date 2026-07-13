namespace Move.Packet
{
    //  パケット先頭1Byteに詰める識別用Enum
    public enum PacketType : byte
    {
        Message = 0,         // Stringパケット
        AssignPlayerId = 1,  // プレイヤーID割り振りパケット
        Login = 2,           // ログイン要求パケット
        GoMatch = 3,         // マッチ通知用パケット
        MatchSuccess = 4,    // マッチ成功パケット
        SpawnPlayer = 5,     // プレイヤー生成パケット
        Position = 6,        // 座標パケット
    }
}