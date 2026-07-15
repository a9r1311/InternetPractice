using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Move.Character;
using Move.Core;
using Move.Packet;
using Move.Player;

namespace Move.Client
{
    //  クライアントクラス
    [DisallowMultipleComponent]
    public sealed class NetworkClient : MonoBehaviour, INetEventListener
    {
        public static NetworkClient Instance { get; private set; }

        int _playerID;  // サーバーから割り振られたプレイヤーID
        NetManager _client;  //  自分自身がリスナー
        NetworkSender _sender;  // 送信専門クラス
        NetPeer _serverPeer;  // 接続先

        int _connetctedPlayerID;  // 通信相手のキャラクターID
        readonly Dictionary<int, GameObject> _spawnedPlayers = new Dictionary<int, GameObject>();


        //  外部クラスからマッチ成功時処理に触るためのプロパティ
        public static event Action OnMatchmakingSuccess;

        //  接続確認プロパティ
        public bool IsConnected =>(
            _serverPeer != null &&
            _serverPeer.ConnectionState == ConnectionState.Connected);

        public ProcessDisplayer ProcessDisplay;  // 授業提出用のUIクラス

        void Awake()
        {
            Instance = this;

            _client = new NetManager(this);
            _sender = new NetworkSender(this, _serverPeer);
        }

        async void Start()
        {
            //  メモリ確保
            _client.Start();

            Debug.Log("[通信] マッチメーカーにインゲームサーバーの割り当てを要請中...");
            ProcessDisplay.ChangeProcessText("Request Ingame server allocation to MatchMake server");

            ServerInfo info = await ClientMatchmaker.RequestMatchServerAsync();

            if (!string.IsNullOrEmpty(info.ipAddress) && info.port != 0)
            {
                _client.Connect(info.ipAddress, info.port, "");
                Debug.Log($"[通信] 割り当てられた試合サーバー（{info.ipAddress}:{info.port}）への接続を開始しました");
                ProcessDisplay.ChangeProcessText($"Start Connection to allocation server（{info.ipAddress}:{info.port}）");
            }
            else
            {
                Debug.LogError("[通信] 有効な試合サーバーの情報を取得できなかったため、接続を断念しました。");
                ProcessDisplay.ChangeProcessText("Stop try connect, because couldn't get active server info");
            }
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

            _playerID = peer.Id;
            Debug.Log($"[通信] サーバーに接続しました！ あなたの仮ID: {peer.Id}");
            ProcessDisplay.ChangeProcessText($"Connect server. Your Temporary ID = {peer.Id}");
        }

        //  サーバーからデータを受信した時の処理
        public void OnNetworkReceive(
            NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod
            )
        {
            try
            {
                PacketType packetType = (PacketType)reader.GetByte();

                switch (packetType)
                {
                    case PacketType.AssignPlayerId:    //  プレイヤーID受信
                        {
                            HandleAssignPlayerID(reader);
                            break;
                        }
                    case PacketType.Position:    //  プレイヤー座標
                        {
                            HandlePosition(reader);
                            break;
                        }
                    case PacketType.MatchSuccess:    //  マッチ成功受信
                        {
                            HandleMatchSuccess(reader);
                            OnMatchmakingSuccess?.Invoke();
                            break;
                        }
                    case PacketType.SpawnPlayer:    //  プレイヤー生成受信
                        {
                            HandleSpawnPlayer(peer, reader);
                            break;
                        }
                    default:
                        {
                            Debug.LogWarning($"[通信] 知らないパケットIDが届きました: {packetType}");
                            ProcessDisplay.ChangePacketText($"Receive unknow packet: {packetType}");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"データ受信エラー: {ex.Message}");
                ProcessDisplay.ChangeProcessText($"Data Receive Error: {ex.Message}");
            }
            reader.Recycle();
        }

        //  サーバーから切断された時の処理
        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.LogWarning($"[通信切断] サーバーから切断されました。理由: {disconnectInfo.Reason}");
            ProcessDisplay.ChangeProcessText($"Disconnect from server Reason : {disconnectInfo.Reason}");

            _serverPeer = null;
            _sender.UpdateServerPeer(_serverPeer);
        }

        //  ネットワークエラー処理
        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Debug.LogError($"[通信エラー] ネットワークエラーが発生しました: {socketError}");
            ProcessDisplay.ChangeProcessText($"Hapen NetWorkError: {socketError}");
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            // サーバーとの往復ラグ（Ping）が更新された時。ここにミリ秒（ms）が入る
        }

        public void OnNetworkReceiveUnconnected(
            IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType
            )
        {
            // 未接続状態でのデータ受信（空でOK）
        }

        //  ID割り当てられ処理
        void HandleAssignPlayerID(NetPacketReader reader)
        {
            _playerID = reader.GetInt();
            ProcessDisplay.ChangePacketText($"RecievePacket PakcetType : {PacketType.AssignPlayerId} (ID = {_playerID})");
        }

        //  座標受信時処理
        void HandlePosition(NetPacketReader reader)
        {
            int id = reader.GetInt();
            float x = reader.GetFloat();
            float y = reader.GetFloat();
            float z = reader.GetFloat();

            if (id == _playerID) return;

            Vector3 targetPos = new Vector3(x, y, z);

            GameManager.Instance.UpdateRemotePlayerPosition(id, targetPos);
            ProcessDisplay.ChangePacketText($"RecievePacket PakcetType : {PacketType.Position} (RemotePlayerPosition = {targetPos})");
        }

        //  マッチ成功受信処理
        void HandleMatchSuccess(NetPacketReader reader)
        {
            int RoomID = reader.GetInt();
            _connetctedPlayerID = reader.GetInt();
            Debug.Log($"マッチ成功 対戦相手ID → [{_connetctedPlayerID}]");
            ProcessDisplay.ChangePacketText($"RecievePacket PakcetType : {PacketType.MatchSuccess} (RemotePlayerID = {_connetctedPlayerID})");
        }

        //  プレイヤースポーン処理
        void HandleSpawnPlayer(NetPeer peer, NetPacketReader reader)
        {
            int id = reader.GetInt();

            Vector3 spwanPos = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            GameObject characterObj = PlayerSpawner.Instance.Spawn(spwanPos);

            _spawnedPlayers[id] = characterObj;

            if (id == _playerID)
            {
                characterObj.AddComponent<PlayerInputController>();
                Debug.Log($" 自身のObjectに、操作スクリプトをアタッチしました。");
            }
            else
            {
                var remoteCtrl = characterObj.AddComponent<RemoteCharacterController>();
                GameManager.Instance.RegisterRemotePlayer(_connetctedPlayerID, remoteCtrl);
            }
            
            ProcessDisplay.ChangePacketText($"RecievePacket PakcetType : {PacketType.SpawnPlayer}");

            //OnPlayerSpawnedWithInstance?.Invoke(id, playerObj);
        }
    }
}