using System.Net;
using System.Net.Sockets;
using LiteNetLib;

namespace Move.Server
{
    public sealed class NetworkManager : INetEventListener
    {
        NetManager _server;

        readonly OnDataReceive _onDataReceive = new OnDataReceive();    //  情報を受信した時のクラス
        readonly Matchmaking _matchmaking = new Matchmaking();    //  マッチングクラス

        //  サーバー起動
        public void StartServer(int port)
        {
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
            Console.WriteLine($"[接続] プレイヤーが入室しました。ID: {peer.Id}");
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
            //  切断処理
            _matchmaking.HandleDisconnect(peer);
            Console.WriteLine($"[切断] プレイヤーが退室しました。ID: {peer.Id} 理由: {disconnectInfo.Reason}");
            
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
    }
}