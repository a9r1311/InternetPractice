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

        readonly OnDataReceive _onDataReceive = new OnDataReceive();    //  情報を受信した時のクラス
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
            _onDataReceive.OnGoMatchReceived += _matchmaking.AddWaitiongList;

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
            PacketContext context = new PacketContext(peer, reader);
            _onDataReceive.Receive(context);

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