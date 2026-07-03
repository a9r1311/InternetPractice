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
        readonly Dictionary<int, List<NetPeer>> _activeRooms = new Dictionary<int, List<NetPeer>>();    //  ゲーム中のマッチ部屋

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

        //  マッチング
        void TriggerMatchSuccess()
        {
            int count = _waitingList.Count;
            NetPeer player1 = _waitingList[count - 1];
            NetPeer player2 = _waitingList[count - 2];

            int roomId = _nextRoomId++;
            var roomPlayers = new List<NetPeer>(2) { player1, player2 };
            
            _activeRooms.Add(roomId, roomPlayers);
            Console.WriteLine($"[マッチング成功] 部屋番号【{roomId}】が作成されました！ メンバー: Peer[{player1.Id}], Peer[{player2.Id}]");

            // playerへマッチ結果を送信
            SendMatchResult(player1, roomId, opponentId: player2.Id);
            SendMatchResult(player2, roomId, opponentId: player1.Id);

            //  Player生成命令を発信
            SendSpawnCommand(player1, player1.Id, spawnX: -5f, spawnY: 0f, spawnZ: 0f);
            SendSpawnCommand(player2, player2.Id, spawnX:  5f, spawnY: 0f, spawnZ: 0f);

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
