using LiteNetLib;
using Move.Packet;

namespace Move.Server
{
    //  データを受け取った時の処理をするクラス
    public sealed class OnDataReceive
    {
        public event Action<NetPeer> OnGoMatchReceived;

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
                            Console.WriteLine($"[データ受信] プレイヤー {packetContext.Peer.Id} から: {message}");
                            break;
                        }
                    case PacketType.Position:
                        {
                            float posX = packetContext.Reader.GetFloat();
                            float posY = packetContext.Reader.GetFloat();
                            float posZ = packetContext.Reader.GetFloat();
                            Console.WriteLine($"[受信成功] プレイヤー {packetContext.Peer.Id} -> 位置:({posX:F2}, {posY:F2}, {posZ:F2})");
                            break;
                        }
                    case PacketType.GoMatch:
                        {
                            OnGoMatchReceived?.Invoke(packetContext.Peer);
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
