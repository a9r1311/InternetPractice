using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Move.Packet;
using Move.Server;

namespace Move.Client
{
    //  クライアントクラス
    public sealed class NetworkClient : MonoBehaviour, INetEventListener
    {
        NetManager _client;     //  自分自身がリスナー
        NetPeer _serverPeer;    // 接続先

        NetworkSender _sender;
        readonly NetDataWriter _writer = new NetDataWriter();    //  データ書き込み用クラス
        OnDataReceive _onDataReceive = new OnDataReceive(); 　   //  データ受け取りクラス

        [SerializeField] string _ipAddress = "127.0.0.1";    //  ローカルホスト
        [SerializeField] int _port = 9050;                   // サーバーポート

        //  外部クラスからマッチ成功時処理に触るためのプロパティ
        public event Action OnMatchmakingSuccess
        {
            add => _onDataReceive.OnReceiveMatchSuccess += value;
            remove => _onDataReceive.OnReceiveMatchSuccess -= value;
        }

        //  接続確認プロパティ
        public bool IsConnected =>(
            _serverPeer != null &&
            _serverPeer.ConnectionState == ConnectionState.Connected);
        
        void Awake()
        {
            _client = new NetManager(this);

            _sender = new NetworkSender(this, _serverPeer);
        }

        void Start()
        {
            //  ポート番号確保
            _client.Start();

            //  接続開始
            _client.Connect(_ipAddress, _port, "");
            Debug.Log($"[通信] サーバー（{_ipAddress}:{_port}）への接続を開始しました...");
        }

        void Update()
        {
            _client.PollEvents();
        }
        void OnDestroy()
        {
            if (_client != null)
            {
                _client.Stop();
            }
        }

        //  座標を送信
        public void SendPosition(Vector3 pos)
        { _sender.SendPosition(pos);}

        //  メッセージ送信
        public void SendTestMessage(string message)
        {
            if (_serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected)
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put(message);

                _serverPeer.Send(writer, DeliveryMethod.Unreliable);
            }
        }

        //  マッチングリクエストを送る
        public void SendMatchingsSignal()
        {
            _sender.SendMatchingSignal();
        }

        //  接続成功時処理
        public void OnPeerConnected(NetPeer peer)
        {
            _serverPeer = peer;
            _sender.UpdateServerPeer(_serverPeer);

            Debug.Log($"[通信成功] サーバーに接続しました！ あなたの仮ID: {peer.Id}");

        }

        //  サーバーからデータを受信した時の処理
        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            PacketContext context = new PacketContext(peer, reader);
            _onDataReceive.Receive(context);
            
            reader.Recycle();
        }

        //  サーバーから切断された時の処理
        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.LogWarning($"[通信切断] サーバーから切断されました。理由: {disconnectInfo.Reason}");

            _serverPeer = null;
            _sender.UpdateServerPeer(_serverPeer);
        }

        //  ネットワークエラー処理
        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Debug.LogError($"[通信エラー] ネットワークエラーが発生しました: {socketError}");
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            // サーバーとの往復ラグ（Ping）が更新された時。ここにミリ秒（ms）が入る
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            // 未接続状態でのデータ受信（空でOK）
        }
    }
}