using Move.Packet;
using System;

namespace Move.Server
{
    //  データを受け取った時の処理をするクラス
    public sealed class OnDataReceive
    {
        public event Action OnReceiveMatchSuccess;    //  マッチ成功時の処理

        //  外部から呼ばれるAPI
        public void Receive(PacketContext packetContext)
        {
            try
            {
                PacketType packetType = (PacketType)packetContext.Reader.GetByte();

                switch (packetType)
                {
                    case PacketType.Message:
                        {
                            string message = packetContext.Reader.GetString();
                            break;
                        }
                    case PacketType.Position:
                        {
                            float posX = packetContext.Reader.GetFloat();
                            float posY = packetContext.Reader.GetFloat();
                            float posZ = packetContext.Reader.GetFloat();
                            break;
                        }
                    case PacketType.GoMatch:
                        {
                            break;
                        }
                    case PacketType.MatchSuccess:
                        {
                            OnReceiveMatchSuccess?.Invoke();
                            break;
                        }
                    default:
                        {
                            Console.WriteLine($"[警告] 知らないパケットIDが届きました: {packetType}");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"データ受信エラー: {ex.Message}");
            }
        }
    }
}
