using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using Move.Packet;

namespace Move.Client
{
    public sealed class NetworkSender
    {
        readonly NetworkClient _client;    //  コンストラクタで取得
        NetPeer _serverPeer;               //  コンストラクタで取得
        readonly NetDataWriter _writer = new NetDataWriter();

        public NetworkSender(NetworkClient client, NetPeer serverPeer)
        {
            _client = client;
            _serverPeer = serverPeer;
        }

        //  座標を送る
        public void SendPosition(Vector3 pos)
        {
            if (!_client.IsConnected)
            {
                Debug.LogWarning("サーバーに接続されていない状態で座標を送ろうとしています。");
                return;
            }

            _writer.Reset();
            Debug.Log((byte)PacketType.Position);

            //  パケットタイプを入れる
            _writer.Put((byte)PacketType.Position);
         
            //  座標データ入力
            _writer.Put(pos.x);
            _writer.Put(pos.y);
            _writer.Put(pos.z);

            //  送信
            _serverPeer.Send(_writer, DeliveryMethod.Unreliable);
        }

        public void SendMatchingSignal()
        {
            if (!_client.IsConnected)
            {
                Debug.LogWarning("サーバーに接続されていない状態でマッチング指令を送ろうとしています。");
                return;
            }

            _writer.Reset();

            _writer.Put((byte)PacketType.GoMatch);
            
            //  送信
            _serverPeer.Send(_writer, DeliveryMethod.Unreliable);
            Debug.LogWarning("送信");

        }
        //  接続先が変わった時に_serverPeerを更新
        public void UpdateServerPeer(NetPeer serverPeer)
        {
            _serverPeer = serverPeer;
        }
    }
}