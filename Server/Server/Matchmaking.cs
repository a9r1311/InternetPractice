using LiteNetLib;
using LiteNetLib.Utils;
using Move.Packet;

namespace Move.Server
{
    //  マッチング担当クラス
    public sealed class Matchmaking
    {
        readonly List<NetPeer> _waitingList = new List<NetPeer>(100);    //  マッチ待ち列

        int _nextRoomId = 1;
        readonly Dictionary<int, NetPeer[]> _activeRooms = new Dictionary<int, NetPeer[]>();    //  ゲーム中のマッチ部屋

        readonly NetDataWriter _cachedWriter = new NetDataWriter();

        //  マッチ待ち列制御
        public void AddWaitiongList(NetPeer peer)
        {
            if (_waitingList.Contains(peer))
            {
                return;
            }

            _waitingList.Add(peer);
            Console.WriteLine($"[キュー] プレイヤー {peer.Id} がマッチング待ちに入りました。 現在の人数: {_waitingList.Count}人");

            if (_waitingList.Count >= 2)
            {
                //  マッチング
                TriggerMatchSuccess();
            }
        }

        public NetPeer[] GetRoomPlayers(NetPeer peer)
        {
            // 現在アクティブな全部屋をループして、引数のpeerが含まれている部屋を探す
            foreach (var room in _activeRooms.Values)
            {
                // 配列なので最速で判定可能
                if (room[0] == peer || room[1] == peer)
                {
                    return room;
                }
            }
            return null;
        }

        //  マッチング
        void TriggerMatchSuccess()
        {
            int count = _waitingList.Count;
            NetPeer player1 = _waitingList[count - 1];
            NetPeer player2 = _waitingList[count - 2];
            int player1ID = NetworkManager.GetPlayerId(player1);
            int player2ID = NetworkManager.GetPlayerId(player2);

            int roomId = _nextRoomId++;
            var roomPlayers = new NetPeer[2] { player1, player2 };

            _activeRooms.Add(roomId, roomPlayers);
            Console.WriteLine($"[マッチング成功] 部屋番号【{roomId}】が作成されました！ メンバー: Peer[{player1.Id}], Peer[{player2.Id}]");

            // playerへマッチ結果を送信
            SendMatchResult(player1, roomId, opponentId: player2ID);
            SendMatchResult(player2, roomId, opponentId: player1ID);

            foreach (var toPeer in roomPlayers)
            {
                SendSpawnCommand(toPeer, player1ID, spawnX: -5f, spawnY: 0f, spawnZ: 0f);
                SendSpawnCommand(toPeer, player2ID, spawnX: 5f, spawnY: 0f, spawnZ: 0f);
            }

            _waitingList.RemoveRange(count - 2, 2);
        }

        //  マッチング結果を送る
        void SendMatchResult(NetPeer peer, int roomId, int opponentId)
        {
            _cachedWriter.Reset();
            _cachedWriter.Put((byte)PacketType.MatchSuccess);
            _cachedWriter.Put(roomId);
            _cachedWriter.Put(opponentId);

            peer.Send(_cachedWriter, DeliveryMethod.ReliableOrdered);
        }

        //  マッチ開始時のプレイヤー生成命令発信
        void SendSpawnCommand(
            NetPeer toPeer, int playerId, float spawnX, float spawnY, float spawnZ
            )
        {
            _cachedWriter.Reset();
            _cachedWriter.Put((byte)PacketType.SpawnPlayer);
            _cachedWriter.Put(playerId);
            _cachedWriter.Put(spawnX);
            _cachedWriter.Put(spawnY);
            _cachedWriter.Put(spawnZ);

            toPeer.Send(_cachedWriter, DeliveryMethod.ReliableOrdered);
        }

        //  切断制御クラス
        public void HandleDisconnect(NetPeer peer)
        {
            if (_waitingList.Remove(peer))
            {
                Console.WriteLine($"[キュー] プレイヤー {peer.Id} が切断したため、マッチ待機リストから削除しました。");
            }
        }
    }
}
