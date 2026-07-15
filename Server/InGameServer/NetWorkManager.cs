using InGameServer;
using LiteNetLib;
using LiteNetLib.Utils;
using Move.Packet;
using System.Net;
using System.Net.Sockets;

namespace Move.Server
{
    public sealed class NetworkManager : INetEventListener
    {
        NetManager _server;

        readonly Matchmaking _matchmaking = new Matchmaking();    //  マッチングクラス
        readonly NetDataWriter _cachedWriter = new NetDataWriter();

        int _assignPlayerId = 1;    //  プレイヤーに割り振るID
        readonly Dictionary<NetPeer, int> _connectedPlayers = new Dictionary<NetPeer, int>();    //  PeerがKeyのID辞書
        static readonly Dictionary<int, NetPeer> _idToPeers = new Dictionary<int, NetPeer>();    //  IDがKeyのPeer辞書
        
        readonly Dictionary<NetPeer, PlayerState> _peerStates = new Dictionary<NetPeer, PlayerState>();  // Peerの接続状態辞書
        readonly Dictionary<NetPeer, int> _authorizedPeers = new Dictionary<NetPeer, int>();  // 認証が取れたユーザー辞書

        readonly int connectLimit = 50;  // 最大同時接続人数
        readonly float mapLimit = 10f;  // マップ端

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
            if(_connectedPlayers.Count >= connectLimit)
            {
                request.Reject();
            }
            request.Accept();
        }
        public void OnPeerConnected(NetPeer peer)
        {
            int assignedId = _assignPlayerId++;
            _connectedPlayers.Add(peer, assignedId);
            _idToPeers.Add(assignedId, peer);

            _peerStates.Add(peer, PlayerState.Connected);

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
                    case PacketType.Login:  // マッチング受信
                        {
                            HandleGoMatchReceive(peer);
                            break;
                        }
                    case PacketType.GoMatch:  // マッチング受信
                        {
                            HandleGoMatchReceive(peer);
                            break;
                        }
                    case PacketType.Position:  // 座標変化受信
                        {
                            HandlePositionReceive(peer, reader);
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
                _idToPeers.Remove(id);
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

        //  マッチング受信処理
        void HandleLoginReceive(NetPeer peer)
        {
            _matchmaking.AddWaitiongList(peer);
        }

        //  マッチング受信処理
        void HandleGoMatchReceive(NetPeer peer)
        {
            _matchmaking.AddWaitiongList(peer);
        }

        //  座標受信処理
        void HandlePositionReceive(NetPeer peer, NetPacketReader reader)
        {
            int senderID = GetPlayerId(peer);
            float posX = reader.GetFloat();
            float posY = reader.GetFloat();
            float posZ = reader.GetFloat();

            if(
                float.IsNaN(posX) || float.IsInfinity(posX) || Math.Abs(posX) > mapLimit ||
                float.IsNaN(posY) || float.IsInfinity(posY) || Math.Abs(posY) > mapLimit ||
                float.IsNaN(posZ) || float.IsInfinity(posZ) || Math.Abs(posZ) > mapLimit
                )
            {
                Console.WriteLine($"異常値パケットを受信しました: PlayerID {GetPlayerId(peer)}");
                return;
            }

            NetPeer[] roomPlayers = _matchmaking.GetRoomPlayers(peer);
            if (roomPlayers == null) return;

            _cachedWriter.Reset();
            _cachedWriter.Put((byte)PacketType.Position);
            _cachedWriter.Put(senderID);
            _cachedWriter.Put(posX);
            _cachedWriter.Put(posY);
            _cachedWriter.Put(posZ);

            foreach (var targetPeer in roomPlayers)
            {
                if (targetPeer == peer) continue;

                targetPeer.Send(_cachedWriter, DeliveryMethod.Unreliable);
            }
        }



        //  プレイヤーID送信
        void SendAssignIdPacket(NetPeer peer, int assignedId)
        {
            _cachedWriter.Reset();

            _cachedWriter.Put((byte)PacketType.AssignPlayerId);
            _cachedWriter.Put(assignedId);
            
            peer.Send(_cachedWriter, DeliveryMethod.ReliableOrdered);
        }
    }
}