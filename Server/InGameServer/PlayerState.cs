namespace InGameServer
{
    internal enum PlayerState
    {
        Connected,  // 接続した直後
        LoggedIn,   // ログイン成功（ユーザー認証済み）
        Matching,   // マッチング待ち
        InGame      // ゲーム中
    }
}
