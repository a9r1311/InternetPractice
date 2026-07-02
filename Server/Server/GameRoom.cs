namespace Move.Server
{
    public struct GameRoom    // マッチ部屋定義構造体
    {
        public int RoomId;         //  部屋ID
        public int MaxPlayers;     //  最大人数
        public string RoomName;    //  部屋名

        // 部屋に入っているプレイヤーのPeerID（LiteNetLibのId）を格納する最軽量のリスト
        public List<int> PlayerPeerIds;
    }
}
