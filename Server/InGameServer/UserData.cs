using System.Runtime.InteropServices;

namespace InGameServer.User
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UserData
    {
        // --- 基本情報 (8バイト) ---
        public int UserId;  // ユーザーの一意ID
        public int Rating;  // マッチング用レート (Eloなど)

        // --- ステータス (4バイト) ---
        // 0: 未ログイン, 1: 待機中, 2: 対戦中, 3: 離脱中
        public byte Status;
        public byte Flags;  // 予備フラグ(未使用)

        public short Padding;  // 8Byte倍数に合わせるための調整用

        // --- マッチング用ターゲット (4バイト) ---
        public int TargetId;  // マッチング相手のID

        // --- 堅牢性用チェックサム (8バイト) ---
        public long Checksum;  // 不正なメモリ書き換えを検知するための署名
    }
}