using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using Move.Packet;

namespace Move.Server
{
    public sealed class NetworkManager : INetEventListener
    {
        NetManager _server;

        readonly Matchmaking _matchmaking = new Matchmaking();    //  マッチングクラス

        readonly Dictionary<NetPeer, int> _connectedPlayers = new Dictionary<NetPeer, int>();    //  PeerがKeyのID辞書
        static readonly Dictionary<int, NetPeer> _idToPeers = new Dictionary<int, NetPeer>();    //  IDがKeyのPeer辞書

        int _assignPlayerId = 1;    //  プレイヤーに割り振るID

        public static NetworkManager Instance { get; private set; }

        //  特定の回線を取得する関数
        public static NetPeer GetPeer(int playerId)
        {
            return _idToPeers.TryGetValue(playerId, out var peer) ? peer : null;
        }

        //  特定のPlayerIDを取得する関数
        public static int GetPlayerId(NetPeer peer)
        {
            if(Instance != null && Instance._connectedPlayers.TryGetValue(peer, out int id))
            {
                return id;
            }
            return -1;
        }

        //  サーバー起動
        public void StartServer(int port)
        {
            Instance = this;
            _server = new NetManager(this);

            _server.Start(port);
            Console.WriteLine($"サーバーがポート {port} で起動しました。");
        }

        //  サーバー処理(計算で毎秒正確に休ませる)
        public void UpdateLoop()
        {
            const float targetTickRate = 60f;
            const int targetMsPerTick = (int)(1000f / targetTickRate); // 約16ミリ秒

            var stopwatch = new System.Diagnostics.Stopwatch();

            while (true)
            {
                stopwatch.Restart();

                _server.PollEvents();

                int processTime = (int)stopwatch.ElapsedMilliseconds;
                int sleepTime = targetMsPerTick - processTime;

                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime); // 正確に12ミリ秒だけ休憩させる
                }
            }
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            //  今の所制限なし
            request.Accept();
        }
        public void OnPeerConnected(NetPeer peer)
        {
            int assignedId = _assignPlayerId++;
            _connectedPlayers.Add(peer, assignedId);
            _idToPeers.Add(assignedId, peer);

            Console.WriteLine($"[接続受付] 通信ID:{peer.Id} → ゲーム用確定ID「{assignedId}」を発行しました。");

            SendAssignIdPacket(peer, assignedId);
        }

        //  データ受信処理
        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            try
            {
                PacketType packetType = (PacketType)reader.GetByte();

                switch (packetType)
                {
                    case PacketType.Message:
                        {
                            //  メッセージ受信処理
                            HandleMessageReceive(peer, reader);
                            break;
                        }
                    case PacketType.Position:
                        {
                            HandlePositionReceive(peer, reader);
                            break;
                        }
                    case PacketType.GoMatch:
                        {
                            HandleGoMatchReceive(peer);
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
            reader.Recycle();
        }
        
        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (_connectedPlayers.TryGetValue(peer, out int id))
            {
                _matchmaking.HandleDisconnect(peer);
                _connectedPlayers.Remove(peer);
                Console.WriteLine($"[切断] ゲーム用ID【{id}】のプレイヤーが退出しました。理由: {disconnectInfo.Reason}");
            }
        }
        
        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Console.WriteLine($"[エラー] ネットワークエラー: {socketError}");
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        //  メッセージを受信処理
        void HandleMessageReceive(NetPeer peer, NetPacketReader reader)
        {
            string message = reader.GetString();
            Console.WriteLine($"[データ受信] プレイヤー {peer.Id} から: {message}");
        }

        //  座標受信処理
        void HandlePositionReceive(NetPeer peer, NetPacketReader reader)
        {
            float posX = reader.GetFloat();
            float posY = reader.GetFloat();
            float posZ = reader.GetFloat();
            Console.WriteLine($"[受信成功] プレイヤー {GetPlayerId(peer)} -> 位置:({posX:F2}, {posY:F2}, {posZ:F2})");
        }

        //  マッチング受信処理
        void HandleGoMatchReceive(NetPeer peer)
        {
            _matchmaking.AddWaitiongList(peer);
        }

        //  プレイヤーID送信
        void SendAssignIdPacket(NetPeer peer, int assignedId)
        {
            NetDataWriter writer = new NetDataWriter();

            writer.Put((byte)PacketType.AssignPlayerId);
            writer.Put(assignedId);
            
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }
}